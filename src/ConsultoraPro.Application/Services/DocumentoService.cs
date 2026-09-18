using System.Text.RegularExpressions;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.DTOs.Documentos;
using ConsultoraPro.Application.Exceptions;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Documentos;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.Application.Services;

/// <summary>
/// Repositorio documental por proyecto: estructura corporativa de carpetas, codificación
/// automática, versionado y etiquetas. El acceso se limita a quienes ven todos los proyectos o
/// son miembros del proyecto (mismo criterio que Screenshots).
/// </summary>
public partial class DocumentoService : IDocumentoService
{
    private const string StorageCategory = "documentos";

    // Serializa la creación de la estructura corporativa: dos lecturas simultáneas de un proyecto
    // nuevo (conteo de la pestaña + la pestaña) duplicarían las carpetas. Suficiente con una sola
    // instancia del backend, que es el despliegue actual.
    private static readonly SemaphoreSlim ProvisionLock = new(1, 1);

    private readonly IDocumentoRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IStorageService _storage;
    private readonly IProjectScope _projectScope;
    private readonly ICurrentUserService _currentUser;
    private readonly StorageLimits _limits;

    public DocumentoService(
        IDocumentoRepository repository,
        IProyectoRepository proyectoRepository,
        IStorageService storage,
        IProjectScope projectScope,
        ICurrentUserService currentUser,
        IOptions<StorageOptions> storageOptions)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
        _storage = storage;
        _projectScope = projectScope;
        _currentUser = currentUser;
        _limits = storageOptions.Value.Limits;
    }

    public async Task<DocumentacionProyectoDto> GetProyectoAsync(Guid proyectoId)
    {
        EnsureAcceso(proyectoId);

        var carpetas = await _repository.GetCarpetasAsync(proyectoId);
        if (carpetas.Count == 0)
        {
            await ProvisionLock.WaitAsync();
            try
            {
                carpetas = await _repository.GetCarpetasAsync(proyectoId);
                if (carpetas.Count == 0)
                    carpetas = await ProvisionarEstructuraAsync(proyectoId);
            }
            finally
            {
                ProvisionLock.Release();
            }
        }

        var documentos = await _repository.GetByProyectoAsync(proyectoId);
        var dtos = new List<DocumentoDto>(documentos.Count);
        foreach (var documento in documentos)
            dtos.Add(await ToDtoAsync(documento));

        return new DocumentacionProyectoDto
        {
            Carpetas = carpetas.Select(ToDto).ToList(),
            Documentos = dtos,
            MaxBytes = _limits.MaxDocumentBytes,
            ExtensionesPermitidas = _limits.AllowedDocumentExtensions.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        };
    }

    public async Task<DocumentoDto> SubirAsync(SubirDocumentoCommand command)
    {
        EnsureAcceso(command.ProyectoId);
        var (nombreArchivo, extension) = ValidarArchivo(command.Archivo);

        var proyecto = await _proyectoRepository.GetByIdAsync(command.ProyectoId)
            ?? throw new KeyNotFoundException("Proyecto no encontrado");
        var carpeta = await GetCarpetaDelProyectoAsync(command.CarpetaId, command.ProyectoId);
        var userId = RequireUserId();

        var stored = await _storage.SaveFileAsync(
            command.Archivo.Contenido, nombreArchivo, command.Archivo.ContentType, $"{StorageCategory}/{proyecto.Id}");
        stored = ConTamano(stored, command.Archivo);

        var correlativo = await _repository.CountByTipoAsync(proyecto.Id, command.Tipo) + 1;
        var prefijo = string.IsNullOrWhiteSpace(proyecto.Cliente?.Iniciales) ? proyecto.Clave : proyecto.Cliente.Iniciales;
        var version = NormalizarVersion(command.Version) ?? "1.0";
        var titulo = Recortar(command.Titulo, 200);

        var documento = new Documento
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyecto.Id,
            CarpetaId = carpeta.Id,
            Codigo = DocumentoCatalogo.GenerarCodigo(prefijo, command.Tipo, correlativo),
            Titulo = string.IsNullOrEmpty(titulo) ? Path.GetFileNameWithoutExtension(nombreArchivo) : titulo,
            Descripcion = Recortar(command.Descripcion, 1000),
            Tipo = command.Tipo,
            Estado = command.Estado,
            CreadoPorId = userId,
            ActualizadoPorId = userId
        };
        AplicarArchivo(documento, stored, nombreArchivo, extension, version);
        documento.Versiones.Add(NuevaVersion(documento, 1, version, stored, nombreArchivo, extension, "Versión inicial", userId));
        foreach (var etiqueta in NormalizarEtiquetas(command.Etiquetas))
            documento.Etiquetas.Add(new DocumentoEtiqueta { DocumentoId = documento.Id, Nombre = etiqueta });

        try
        {
            await _repository.AddAsync(documento);
        }
        catch
        {
            // Sin registro en BD el archivo quedaría huérfano en el almacenamiento.
            await _storage.DeleteFileAsync(stored.Key);
            throw;
        }

        return await RecargarDtoAsync(documento.Id);
    }

    public async Task<DocumentoDto> NuevaVersionAsync(Guid documentoId, NuevaVersionCommand command)
    {
        var documento = await GetDocumentoAsync(documentoId);
        var (nombreArchivo, extension) = ValidarArchivo(command.Archivo);
        var userId = RequireUserId();

        var stored = await _storage.SaveFileAsync(
            command.Archivo.Contenido, nombreArchivo, command.Archivo.ContentType, $"{StorageCategory}/{documento.ProyectoId}");
        stored = ConTamano(stored, command.Archivo);

        var numero = documento.Versiones.Count == 0 ? 1 : documento.Versiones.Max(v => v.Numero) + 1;
        var version = NormalizarVersion(command.Version) ?? SugerirSiguienteVersion(documento.VersionActual);

        AplicarArchivo(documento, stored, nombreArchivo, extension, version);
        documento.Versiones.Add(NuevaVersion(documento, numero, version, stored, nombreArchivo, extension, Recortar(command.Nota, 500), userId));
        if (command.Estado.HasValue)
            documento.Estado = command.Estado.Value;
        documento.ActualizadoPorId = userId;

        try
        {
            await _repository.SaveChangesAsync();
        }
        catch
        {
            await _storage.DeleteFileAsync(stored.Key);
            throw;
        }

        return await RecargarDtoAsync(documento.Id);
    }

    public async Task<DocumentoDto> ActualizarAsync(Guid documentoId, ActualizarDocumentoDto dto)
    {
        var documento = await GetDocumentoAsync(documentoId);

        var titulo = Recortar(dto.Titulo, 200);
        if (string.IsNullOrEmpty(titulo))
            throw new InvalidOperationException("El título es obligatorio.");

        if (dto.CarpetaId != documento.CarpetaId)
            documento.CarpetaId = (await GetCarpetaDelProyectoAsync(dto.CarpetaId, documento.ProyectoId)).Id;

        documento.Titulo = titulo;
        documento.Descripcion = Recortar(dto.Descripcion, 1000);
        documento.Estado = dto.Estado;
        // El código conserva la sigla con la que nació: cambiar el tipo no renumera el documento
        // (los códigos se citan en actas y correos y deben ser estables).
        documento.Tipo = dto.Tipo;
        documento.ActualizadoPorId = RequireUserId();

        var nuevas = NormalizarEtiquetas(dto.Etiquetas);
        foreach (var sobrante in documento.Etiquetas.Where(e => !nuevas.Contains(e.Nombre, StringComparer.OrdinalIgnoreCase)).ToList())
            documento.Etiquetas.Remove(sobrante);
        foreach (var etiqueta in nuevas.Where(n => !documento.Etiquetas.Any(e => e.Nombre.Equals(n, StringComparison.OrdinalIgnoreCase))))
            documento.Etiquetas.Add(new DocumentoEtiqueta { DocumentoId = documento.Id, Nombre = etiqueta });

        await _repository.SaveChangesAsync();
        return await RecargarDtoAsync(documento.Id);
    }

    public async Task EliminarAsync(Guid documentoId)
    {
        var documento = await GetDocumentoAsync(documentoId);
        // Borrado lógico: los archivos se conservan por trazabilidad (auditorías, conformidades).
        documento.Activo = false;
        documento.ActualizadoPorId = RequireUserId();
        await _repository.SaveChangesAsync();
    }

    public async Task<DescargaDocumento> DescargarAsync(Guid documentoId, Guid? versionId)
    {
        var documento = await GetDocumentoAsync(documentoId);

        string key = documento.StorageKey, contentType = documento.ContentType, nombre = documento.NombreArchivo;
        if (versionId.HasValue)
        {
            var version = await _repository.GetVersionAsync(documento.Id, versionId.Value)
                ?? throw new KeyNotFoundException("Versión no encontrada");
            (key, contentType, nombre) = (version.StorageKey, version.ContentType, version.NombreArchivo);
        }

        var stream = await _storage.OpenReadAsync(key)
            ?? throw new KeyNotFoundException("El archivo no está disponible en el almacenamiento");

        return new DescargaDocumento(stream, string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType, nombre);
    }

    public async Task<CarpetaDocumentoDto> CrearCarpetaAsync(CrearCarpetaDto dto)
    {
        EnsureAcceso(dto.ProyectoId);
        var nombre = ValidarNombreCarpeta(dto.Nombre);

        var carpetas = await _repository.GetCarpetasAsync(dto.ProyectoId);
        if (dto.ParentId.HasValue)
        {
            var parent = carpetas.FirstOrDefault(c => c.Id == dto.ParentId.Value)
                ?? throw new KeyNotFoundException("Carpeta padre no encontrada");
            if (Profundidad(parent, carpetas) + 1 > DocumentoCatalogo.MaxProfundidadCarpetas)
                throw new InvalidOperationException($"Se permiten como máximo {DocumentoCatalogo.MaxProfundidadCarpetas} niveles de carpetas.");
        }

        var hermanas = carpetas.Where(c => c.ParentId == dto.ParentId).ToList();
        if (hermanas.Any(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe una carpeta con ese nombre en esta ubicación.");

        var carpeta = new CarpetaDocumento
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            ParentId = dto.ParentId,
            Nombre = nombre,
            Descripcion = Recortar(dto.Descripcion, 300),
            Orden = hermanas.Count == 0 ? 1 : hermanas.Max(c => c.Orden) + 1,
            EsSistema = false,
            CreadoPorId = _currentUser.UserId
        };
        await _repository.AddCarpetasAsync([carpeta]);
        return ToDto(carpeta);
    }

    public async Task<CarpetaDocumentoDto> RenombrarCarpetaAsync(Guid carpetaId, RenombrarCarpetaDto dto)
    {
        var carpeta = await _repository.GetCarpetaAsync(carpetaId)
            ?? throw new KeyNotFoundException("Carpeta no encontrada");
        EnsureAcceso(carpeta.ProyectoId);
        if (carpeta.EsSistema)
            throw new InvalidOperationException("Las carpetas de la estructura corporativa no se pueden renombrar.");

        var nombre = ValidarNombreCarpeta(dto.Nombre);
        var hermanas = (await _repository.GetCarpetasAsync(carpeta.ProyectoId))
            .Where(c => c.ParentId == carpeta.ParentId && c.Id != carpeta.Id);
        if (hermanas.Any(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe una carpeta con ese nombre en esta ubicación.");

        carpeta.Nombre = nombre;
        carpeta.Descripcion = Recortar(dto.Descripcion, 300);
        await _repository.SaveChangesAsync();
        return ToDto(carpeta);
    }

    public async Task EliminarCarpetaAsync(Guid carpetaId)
    {
        var carpeta = await _repository.GetCarpetaAsync(carpetaId)
            ?? throw new KeyNotFoundException("Carpeta no encontrada");
        EnsureAcceso(carpeta.ProyectoId);
        if (carpeta.EsSistema)
            throw new InvalidOperationException("Las carpetas de la estructura corporativa no se pueden eliminar.");
        if (await _repository.CarpetaTieneContenidoAsync(carpeta.Id))
            throw new InvalidOperationException("Solo se pueden eliminar carpetas vacías (sin documentos ni subcarpetas, incluidos los eliminados).");

        await _repository.DeleteCarpetaAsync(carpeta);
    }

    // ---------------------------------------------------------------- helpers

    private void EnsureAcceso(Guid proyectoId)
    {
        if (!_projectScope.VeTodos("proyectos") && !_projectScope.EsMiembro(proyectoId))
            throw new AccesoDenegadoException();
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedAccessException("Usuario no válido");

    private async Task<Documento> GetDocumentoAsync(Guid documentoId)
    {
        var documento = await _repository.GetByIdAsync(documentoId)
            ?? throw new KeyNotFoundException("Documento no encontrado");
        EnsureAcceso(documento.ProyectoId);
        return documento;
    }

    private async Task<CarpetaDocumento> GetCarpetaDelProyectoAsync(Guid carpetaId, Guid proyectoId)
    {
        var carpeta = await _repository.GetCarpetaAsync(carpetaId);
        if (carpeta is null || carpeta.ProyectoId != proyectoId)
            throw new KeyNotFoundException("Carpeta no encontrada en el proyecto");
        return carpeta;
    }

    private async Task<List<CarpetaDocumento>> ProvisionarEstructuraAsync(Guid proyectoId)
    {
        _ = await _proyectoRepository.GetByIdAsync(proyectoId)
            ?? throw new KeyNotFoundException("Proyecto no encontrado");

        var carpetas = new List<CarpetaDocumento>();
        void Agregar(IReadOnlyList<CarpetaPlantilla> plantillas, Guid? parentId)
        {
            for (var i = 0; i < plantillas.Count; i++)
            {
                var p = plantillas[i];
                var carpeta = new CarpetaDocumento
                {
                    Id = Guid.NewGuid(),
                    ProyectoId = proyectoId,
                    ParentId = parentId,
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Orden = i + 1,
                    EsSistema = true
                };
                carpetas.Add(carpeta);
                Agregar(p.Hijas, carpeta.Id);
            }
        }
        Agregar(DocumentoCatalogo.EstructuraCorporativa, null);

        await _repository.AddCarpetasAsync(carpetas);
        return carpetas;
    }

    private (string NombreArchivo, string Extension) ValidarArchivo(ArchivoEntrante? archivo)
    {
        if (archivo is null || archivo.TamanoBytes <= 0)
            throw new InvalidOperationException("No se proporcionó ningún archivo.");
        if (archivo.TamanoBytes > _limits.MaxDocumentBytes)
            throw new InvalidOperationException($"El archivo supera el tamaño máximo de {_limits.MaxDocumentBytes / (1024 * 1024)} MB.");

        var nombre = Path.GetFileName(archivo.NombreArchivo ?? string.Empty).Trim();
        if (nombre.Length == 0)
            throw new InvalidOperationException("El archivo no tiene nombre.");
        if (nombre.Length > 255)
            nombre = nombre[^255..];

        var extension = Path.GetExtension(nombre).ToLowerInvariant();
        if (!_limits.AllowedDocumentExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Tipo de archivo no permitido ({(extension.Length == 0 ? "sin extensión" : extension)}).");

        return (nombre, extension);
    }

    /// <summary>Algunos proveedores no conocen el tamaño si el stream no es seekable.</summary>
    private static StoredFile ConTamano(StoredFile stored, ArchivoEntrante archivo) =>
        stored.SizeBytes > 0 ? stored : stored with { SizeBytes = archivo.TamanoBytes };

    private static void AplicarArchivo(Documento documento, StoredFile stored, string nombreArchivo, string extension, string version)
    {
        documento.StorageKey = stored.Key;
        documento.NombreArchivo = nombreArchivo;
        documento.Extension = extension;
        documento.ContentType = string.IsNullOrWhiteSpace(stored.ContentType) ? "application/octet-stream" : stored.ContentType;
        documento.TamanoBytes = stored.SizeBytes;
        documento.VersionActual = version;
    }

    private static DocumentoVersion NuevaVersion(Documento documento, int numero, string version, StoredFile stored,
        string nombreArchivo, string extension, string nota, Guid userId) => new()
    {
        // Sin Id explícito: EF lo genera al insertar. Con un Guid preasignado, un hijo añadido a la
        // colección de un Documento ya rastreado se interpretaría como existente (UPDATE, no INSERT).
        DocumentoId = documento.Id,
        Numero = numero,
        Version = version,
        NombreArchivo = nombreArchivo,
        Extension = extension,
        ContentType = documento.ContentType,
        TamanoBytes = stored.SizeBytes,
        StorageKey = stored.Key,
        Nota = nota,
        SubidoPorId = userId,
        FechaSubida = DateTime.UtcNow
    };

    private static List<string> NormalizarEtiquetas(IEnumerable<string>? etiquetas)
    {
        var result = new List<string>();
        foreach (var raw in etiquetas ?? [])
        {
            foreach (var parte in (raw ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var etiqueta = EspaciosRegex().Replace(parte.Trim().TrimStart('#'), " ");
                if (etiqueta.Length == 0)
                    continue;
                if (etiqueta.Length > 50)
                    etiqueta = etiqueta[..50];
                if (!result.Contains(etiqueta, StringComparer.OrdinalIgnoreCase))
                    result.Add(etiqueta);
            }
        }

        if (result.Count > DocumentoCatalogo.MaxEtiquetasPorDocumento)
            throw new InvalidOperationException($"Se permiten como máximo {DocumentoCatalogo.MaxEtiquetasPorDocumento} etiquetas por documento.");
        return result;
    }

    private static string? NormalizarVersion(string? version)
    {
        var v = version?.Trim().TrimStart('v', 'V');
        if (string.IsNullOrEmpty(v))
            return null;
        return v.Length > 20 ? v[..20] : v;
    }

    /// <summary>"1.0" → "1.1"; "2" → "2.1"; cualquier otro formato → "{actual}-r".</summary>
    private static string SugerirSiguienteVersion(string actual)
    {
        var match = VersionRegex().Match(actual ?? string.Empty);
        if (!match.Success)
            return string.IsNullOrWhiteSpace(actual) ? "1.0" : NormalizarVersion($"{actual}-r")!;
        var mayor = int.Parse(match.Groups[1].Value);
        var menor = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) + 1 : 1;
        return $"{mayor}.{menor}";
    }

    private static string ValidarNombreCarpeta(string? nombre)
    {
        var n = EspaciosRegex().Replace((nombre ?? string.Empty).Trim(), " ");
        if (n.Length == 0)
            throw new InvalidOperationException("El nombre de la carpeta es obligatorio.");
        if (n.Length > 120)
            throw new InvalidOperationException("El nombre de la carpeta admite como máximo 120 caracteres.");
        if (n.IndexOfAny(['/', '\\']) >= 0)
            throw new InvalidOperationException("El nombre de la carpeta no puede contener '/' ni '\\'.");
        return n;
    }

    private static int Profundidad(CarpetaDocumento carpeta, List<CarpetaDocumento> todas)
    {
        var nivel = 1;
        var actual = carpeta;
        while (actual.ParentId.HasValue && nivel <= DocumentoCatalogo.MaxProfundidadCarpetas)
        {
            actual = todas.FirstOrDefault(c => c.Id == actual.ParentId.Value);
            if (actual is null) break;
            nivel++;
        }
        return nivel;
    }

    private static string Recortar(string? value, int max)
    {
        var v = value?.Trim() ?? string.Empty;
        return v.Length > max ? v[..max] : v;
    }

    private async Task<DocumentoDto> RecargarDtoAsync(Guid documentoId)
    {
        var documento = await _repository.GetByIdAsync(documentoId)
            ?? throw new KeyNotFoundException("Documento no encontrado");
        return await ToDtoAsync(documento);
    }

    private async Task<DocumentoDto> ToDtoAsync(Documento d)
    {
        var actual = d.Versiones.Count == 0 ? 0 : d.Versiones.Max(v => v.Numero);
        return new DocumentoDto
        {
            Id = d.Id,
            ProyectoId = d.ProyectoId,
            CarpetaId = d.CarpetaId,
            Codigo = d.Codigo,
            Titulo = d.Titulo,
            Descripcion = d.Descripcion,
            Tipo = d.Tipo,
            Estado = d.Estado,
            VersionActual = d.VersionActual,
            NombreArchivo = d.NombreArchivo,
            Extension = d.Extension,
            ContentType = d.ContentType,
            TamanoBytes = d.TamanoBytes,
            Url = await _storage.GetAccessUrlAsync(d.StorageKey),
            Etiquetas = d.Etiquetas.Select(e => e.Nombre).OrderBy(e => e, StringComparer.OrdinalIgnoreCase).ToList(),
            CreadoPorNombre = NombreCompleto(d.CreadoPor),
            ActualizadoPorNombre = NombreCompleto(d.ActualizadoPor),
            FechaCreacion = d.FechaCreacion,
            UpdatedAt = d.UpdatedAt,
            Versiones = d.Versiones
                .OrderByDescending(v => v.Numero)
                .Select(v => new DocumentoVersionDto
                {
                    Id = v.Id,
                    Numero = v.Numero,
                    Version = v.Version,
                    NombreArchivo = v.NombreArchivo,
                    TamanoBytes = v.TamanoBytes,
                    Nota = v.Nota,
                    SubidoPorNombre = NombreCompleto(v.SubidoPor),
                    FechaSubida = v.FechaSubida,
                    EsActual = v.Numero == actual
                })
                .ToList()
        };
    }

    private static CarpetaDocumentoDto ToDto(CarpetaDocumento c) => new()
    {
        Id = c.Id,
        ParentId = c.ParentId,
        Codigo = c.Codigo,
        Nombre = c.Nombre,
        Descripcion = c.Descripcion,
        Orden = c.Orden,
        EsSistema = c.EsSistema
    };

    private static string NombreCompleto(ApplicationUser? user) =>
        user is null ? string.Empty : $"{user.Nombres} {user.Apellidos}".Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosRegex();

    [GeneratedRegex(@"^(\d+)(?:\.(\d+))?$")]
    private static partial Regex VersionRegex();
}
