using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

/// <summary>
/// Empaqueta todo el contexto de una tarjeta en un ZIP:
/// Datos_Generales.md, Descripcion.md (con checklist e imágenes inline), Actividad.md,
/// carpeta Comentarios/ (con imágenes de comentarios) y carpeta Adjuntos/ con los archivos.
/// </summary>
public class TarjetaExportService : ITarjetaExportService
{
    private readonly ITarjetaRepository _repository;
    private readonly IStorageService _storageService;
    private readonly IFileUrlResolver _urlResolver;
    private readonly IKanbanAccessGuard _accessGuard;

    // Perú no aplica horario de verano: un desfase fijo de -5h basta para mostrar las mismas
    // horas que la UI (America/Lima) sin depender de bases de zonas horarias del SO.
    private static readonly TimeSpan LimaOffset = TimeSpan.FromHours(-5);
    private static readonly CultureInfo EsPe = CultureInfo.GetCultureInfo("es-PE");

    public TarjetaExportService(
        ITarjetaRepository repository,
        IStorageService storageService,
        IFileUrlResolver urlResolver,
        IKanbanAccessGuard accessGuard)
    {
        _repository = repository;
        _storageService = storageService;
        _urlResolver = urlResolver;
        _accessGuard = accessGuard;
    }

    public async Task<TarjetaExportResult?> ExportarAsync(Guid tarjetaId)
    {
        var tarjeta = await _repository.GetDetalleAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            return null;

        await _accessGuard.EnsureTableroAccessAsync(tarjeta.TableroId);

        var actividades = await _repository.GetActividadAsync(tarjetaId);

        // Entradas de texto (ruta -> contenido) y binarias (ruta -> bytes) a escribir en el ZIP.
        var textos = new List<(string Path, string Content)>();
        var binarios = new List<(string Path, byte[] Bytes)>();
        var faltantes = new List<string>();
        var usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // --- Descripción + imágenes inline + checklist ---
        var descripcion = await BuildDescripcionAsync(tarjeta, binarios, usados, faltantes);
        textos.Add(("Descripcion.md", descripcion));

        // --- Datos generales ---
        textos.Add(("Datos_Generales.md", BuildDatosGenerales(tarjeta)));

        // --- Actividad ---
        textos.Add(("Actividad.md", BuildActividad(actividades)));

        // --- Comentarios + imágenes de comentarios ---
        if (tarjeta.Comentarios.Count > 0)
            textos.Add(("Comentarios/Comentarios.md", await BuildComentariosAsync(tarjeta, binarios, usados, faltantes)));

        // --- Adjuntos ---
        await AppendAdjuntosAsync(tarjeta, binarios, usados, faltantes);

        // --- LEEME (guía para la IA + archivos que no se pudieron incluir) ---
        textos.Insert(0, ("LEEME.md", BuildLeeme(tarjeta, faltantes)));

        var zipBytes = BuildZip(textos, binarios);
        var nombre = $"{Sanitizar(tarjeta.Codigo)}.zip";
        return new TarjetaExportResult(zipBytes, nombre);
    }

    // ---------------------------------------------------------------- Descripción

    private async Task<string> BuildDescripcionAsync(
        Tarjeta tarjeta,
        List<(string Path, byte[] Bytes)> binarios,
        HashSet<string> usados,
        List<string> faltantes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {tarjeta.Codigo} — {tarjeta.Titulo}");
        sb.AppendLine();

        var contenido = _urlResolver.ToStoragePlaceholders(tarjeta.Descripcion) ?? tarjeta.Descripcion;
        if (string.IsNullOrWhiteSpace(contenido))
        {
            sb.AppendLine("_(Sin descripción)_");
        }
        else
        {
            var imagenes = _urlResolver.ExtractStorageImages(contenido);
            var i = 0;
            foreach (var img in imagenes)
            {
                i++;
                var ext = ExtensionDeKey(img.Key);
                var nombre = $"imagen_{i:00}{ext}";
                var bytes = await LeerBytesAsync(img.Key);
                if (bytes is null)
                {
                    faltantes.Add($"Descripcion_Imagenes/{nombre} (imagen inline de la descripción)");
                    contenido = contenido!.Replace(img.Placeholder, "(imagen no disponible)");
                    continue;
                }
                var ruta = UnicoEnCarpeta("Descripcion_Imagenes", nombre, usados);
                binarios.Add((ruta, bytes));
                // Enlace relativo desde la raíz del ZIP para que la IA asocie la imagen.
                contenido = contenido!.Replace(img.Placeholder, ruta);
            }
            sb.AppendLine(contenido);
        }

        // Checklist(s) al pie de la descripción.
        var checklists = tarjeta.Checklists.OrderBy(c => c.Orden).ToList();
        if (checklists.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Checklist");
            foreach (var cl in checklists)
            {
                var items = cl.Items.OrderBy(x => x.Orden).ToList();
                var hechos = items.Count(x => x.Completado);
                sb.AppendLine();
                sb.AppendLine($"### {cl.Nombre} ({hechos}/{items.Count})");
                foreach (var item in items)
                    sb.AppendLine($"- [{(item.Completado ? "x" : " ")}] {item.Texto}");
            }
        }

        return sb.ToString();
    }

    // ---------------------------------------------------------------- Datos generales

    private static string BuildDatosGenerales(Tarjeta t)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Datos Generales — {t.Codigo}");
        sb.AppendLine();
        sb.AppendLine($"**Código:** {t.Codigo}");
        sb.AppendLine($"**Título:** {t.Titulo}");
        sb.AppendLine($"**Estado:** {t.Columna?.Nombre ?? "—"}");
        if (t.Tablero is not null)
        {
            var ubicacion = t.Tablero.Proyecto is null
                ? t.Tablero.Nombre
                : $"{t.Tablero.Proyecto.Nombre} · {t.Tablero.Nombre}";
            sb.AppendLine($"**Ubicación:** {ubicacion}");
        }
        sb.AppendLine($"**Prioridad:** {PrioridadTexto(t.Prioridad)}");
        sb.AppendLine($"**Completada:** {(t.Completada ? "Sí" : "No")}");
        sb.AppendLine($"**Reportero:** {NombreUsuario(t.CreadaPor)}");
        sb.AppendLine($"**Fecha de creación:** {FechaLarga(t.FechaCreacion)}");
        sb.AppendLine($"**Última actualización:** {FechaLarga(t.UpdatedAt)}");
        sb.AppendLine($"**Fecha de inicio:** {(t.FechaInicio.HasValue ? FechaLarga(t.FechaInicio.Value) : "—")}");
        sb.AppendLine($"**Fecha límite:** {(t.FechaLimite.HasValue ? FechaLarga(t.FechaLimite.Value) : "—")}");

        var responsables = t.Responsables.Select(r => NombreUsuario(r.Usuario)).Where(n => n.Length > 0).ToList();
        sb.AppendLine($"**Responsables:** {(responsables.Count > 0 ? string.Join(", ", responsables) : "—")}");

        var etiquetas = t.Etiquetas.Select(e => e.Etiqueta?.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        sb.AppendLine($"**Etiquetas:** {(etiquetas.Count > 0 ? string.Join(", ", etiquetas) : "—")}");

        sb.AppendLine($"**Comentarios:** {t.Comentarios.Count}");
        sb.AppendLine($"**Adjuntos:** {t.Adjuntos.Count}");

        return sb.ToString();
    }

    // ---------------------------------------------------------------- Actividad

    private static string BuildActividad(IReadOnlyList<ActividadTarjeta> actividades)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Actividad");
        sb.AppendLine();
        if (actividades.Count == 0)
        {
            sb.AppendLine("_(Sin actividad registrada)_");
            return sb.ToString();
        }

        // El repositorio devuelve de más reciente a más antigua; para una línea de tiempo
        // la mostramos en orden cronológico ascendente.
        foreach (var a in actividades.OrderBy(x => x.Fecha))
        {
            var quien = NombreUsuario(a.Usuario);
            if (quien.Length == 0) quien = "El sistema";
            var detalle = string.IsNullOrWhiteSpace(a.Detalle) ? string.Empty : $" ({a.Detalle})";
            sb.AppendLine($"- {FechaCorta(a.Fecha)} — {quien} {ActividadTexto(a.Tipo)}{detalle}");
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- Comentarios

    private async Task<string> BuildComentariosAsync(
        Tarjeta tarjeta,
        List<(string Path, byte[] Bytes)> binarios,
        HashSet<string> usados,
        List<string> faltantes)
    {
        var comentarios = tarjeta.Comentarios.OrderBy(c => c.FechaCreacion).ToList();
        var sb = new StringBuilder();
        sb.AppendLine($"# Comentarios ({comentarios.Count})");
        sb.AppendLine();

        var n = 0;
        foreach (var c in comentarios)
        {
            n++;
            var autor = NombreUsuario(c.Autor);
            var editado = c.EditadoEn.HasValue ? $" (editado {FechaCorta(c.EditadoEn.Value)})" : string.Empty;
            sb.AppendLine($"#{n} — {FechaLarga(c.FechaCreacion)} — {autor}{editado}");

            var texto = _urlResolver.ToStoragePlaceholders(c.Texto) ?? c.Texto;
            var imagenes = _urlResolver.ExtractStorageImages(texto);
            var img = 0;
            foreach (var im in imagenes)
            {
                img++;
                var ext = ExtensionDeKey(im.Key);
                // Convención pedida: {n}_{fecha}_{autor}.ext dentro de la carpeta Comentarios/.
                var baseName = $"{n}_{FechaArchivo(c.FechaCreacion)}_{Ascii(autor)}";
                var nombre = imagenes.Count > 1 ? $"{baseName}_{img}{ext}" : $"{baseName}{ext}";
                var bytes = await LeerBytesAsync(im.Key);
                if (bytes is null)
                {
                    faltantes.Add($"Comentarios/{nombre} (imagen del comentario #{n})");
                    texto = texto!.Replace(im.Placeholder, "(imagen no disponible)");
                    continue;
                }
                var ruta = UnicoEnCarpeta("Comentarios", nombre, usados);
                binarios.Add((ruta, bytes));
                // El .md vive dentro de Comentarios/, así que basta el nombre de archivo relativo.
                texto = texto!.Replace(im.Placeholder, System.IO.Path.GetFileName(ruta));
            }

            sb.AppendLine(string.IsNullOrWhiteSpace(texto) ? "_(sin texto)_" : texto.Trim());
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- Adjuntos

    private async Task AppendAdjuntosAsync(
        Tarjeta tarjeta,
        List<(string Path, byte[] Bytes)> binarios,
        HashSet<string> usados,
        List<string> faltantes)
    {
        var adjuntos = tarjeta.Adjuntos.OrderBy(a => a.FechaSubida).ToList();
        if (adjuntos.Count == 0)
            return;

        foreach (var a in adjuntos)
        {
            var nombre = Sanitizar(string.IsNullOrWhiteSpace(a.Nombre) ? "adjunto" : a.Nombre);
            var bytes = await LeerBytesAsync(a.StorageKey);
            if (bytes is null)
            {
                faltantes.Add($"Adjuntos/{nombre} (adjunto no encontrado en almacenamiento)");
                continue;
            }
            var ruta = UnicoEnCarpeta("Adjuntos", nombre, usados);
            binarios.Add((ruta, bytes));
        }
    }

    // ---------------------------------------------------------------- LEEME

    private static string BuildLeeme(Tarjeta t, IReadOnlyList<string> faltantes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {t.Codigo} — Contexto de la tarjeta");
        sb.AppendLine();
        sb.AppendLine($"Paquete generado el {FechaLarga(DateTime.UtcNow)} (hora de Perú) para compartir el contexto completo del ticket.");
        sb.AppendLine();
        sb.AppendLine("## Contenido");
        sb.AppendLine("- **Datos_Generales.md** — estado, prioridad, responsables, fechas y etiquetas.");
        sb.AppendLine("- **Descripcion.md** — descripción del ticket, checklist e imágenes referenciadas.");
        sb.AppendLine("- **Actividad.md** — línea de tiempo de cambios de la tarjeta.");
        sb.AppendLine("- **Comentarios/** — todos los comentarios y sus imágenes.");
        sb.AppendLine("- **Descripcion_Imagenes/** — imágenes incrustadas en la descripción.");
        sb.AppendLine("- **Adjuntos/** — archivos adjuntos del ticket.");

        if (faltantes.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Archivos no incluidos");
            sb.AppendLine("Estos archivos estaban referenciados pero no se pudieron leer del almacenamiento:");
            foreach (var f in faltantes)
                sb.AppendLine($"- {f}");
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- ZIP

    private static byte[] BuildZip(
        IReadOnlyList<(string Path, string Content)> textos,
        IReadOnlyList<(string Path, byte[] Bytes)> binarios)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (path, content) in textos)
            {
                var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
                using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
                writer.Write(content);
            }
            foreach (var (path, bytes) in binarios)
            {
                var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
                using var es = entry.Open();
                es.Write(bytes, 0, bytes.Length);
            }
        }
        return ms.ToArray();
    }

    // ---------------------------------------------------------------- Helpers

    private async Task<byte[]?> LeerBytesAsync(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;
        await using var s = await _storageService.OpenReadAsync(key);
        if (s is null)
            return null;
        using var ms = new MemoryStream();
        await s.CopyToAsync(ms);
        return ms.ToArray();
    }

    private static string UnicoEnCarpeta(string carpeta, string nombre, HashSet<string> usados)
    {
        var baseName = System.IO.Path.GetFileNameWithoutExtension(nombre);
        var ext = System.IO.Path.GetExtension(nombre);
        var candidato = $"{carpeta}/{nombre}";
        var i = 1;
        while (!usados.Add(candidato))
        {
            i++;
            candidato = $"{carpeta}/{baseName}_{i}{ext}";
        }
        return candidato;
    }

    private static string ExtensionDeKey(string key)
    {
        var ext = System.IO.Path.GetExtension(key);
        return string.IsNullOrWhiteSpace(ext) ? ".png" : ext;
    }

    private static string NombreUsuario(ApplicationUser? u)
        => u is null ? string.Empty : $"{u.Nombres} {u.Apellidos}".Trim();

    private static string PrioridadTexto(PrioridadTarjeta p) => p switch
    {
        PrioridadTarjeta.Baja => "Baja",
        PrioridadTarjeta.Media => "Media",
        PrioridadTarjeta.Alta => "Alta",
        PrioridadTarjeta.Critica => "Crítica",
        _ => p.ToString()
    };

    private static string ActividadTexto(TipoActividadTarjeta tipo) => tipo switch
    {
        TipoActividadTarjeta.Creada => "creó la tarjeta",
        TipoActividadTarjeta.Editada => "editó la tarjeta",
        TipoActividadTarjeta.Movida => "movió la tarjeta",
        TipoActividadTarjeta.Asignada => "actualizó responsables",
        TipoActividadTarjeta.Desasignada => "quitó un responsable",
        TipoActividadTarjeta.Comentada => "comentó",
        TipoActividadTarjeta.EtiquetaAgregada => "actualizó etiquetas",
        TipoActividadTarjeta.EtiquetaQuitada => "quitó una etiqueta",
        TipoActividadTarjeta.Completada => "completó la tarjeta",
        TipoActividadTarjeta.Reabierta => "reabrió la tarjeta",
        TipoActividadTarjeta.Archivada => "archivó la tarjeta",
        TipoActividadTarjeta.Restaurada => "restauró la tarjeta",
        _ => tipo.ToString()
    };

    private static DateTime ToLima(DateTime utc)
        => DateTime.SpecifyKind(utc, DateTimeKind.Utc).Add(LimaOffset);

    /// <summary>"9 de junio de 2026 15:03".</summary>
    private static string FechaLarga(DateTime utc)
        => ToLima(utc).ToString("d 'de' MMMM 'de' yyyy HH:mm", EsPe);

    /// <summary>"06 jun 2026 22:57".</summary>
    private static string FechaCorta(DateTime utc)
        => ToLima(utc).ToString("dd MMM yyyy HH:mm", EsPe);

    /// <summary>"9_junio_2026_15_03" para nombres de archivo.</summary>
    private static string FechaArchivo(DateTime utc)
        => ToLima(utc).ToString("d_MMMM_yyyy_HH_mm", EsPe);

    /// <summary>Quita caracteres inválidos para nombre de archivo, preservando el original.</summary>
    private static string Sanitizar(string nombre)
    {
        var limpio = new string(nombre.Where(c => !System.IO.Path.GetInvalidFileNameChars().Contains(c)).ToArray()).Trim();
        return limpio.Length == 0 ? "archivo" : limpio;
    }

    /// <summary>Nombre ASCII sin acentos ni espacios, para nombres de archivo generados.</summary>
    private static string Ascii(string valor)
    {
        var normalizado = valor.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);
        foreach (var c in normalizado)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(c))
                sb.Append(c);
            else if (c is ' ' or '_' or '-')
                sb.Append('_');
        }
        var res = sb.ToString().Trim('_');
        return res.Length == 0 ? "usuario" : res;
    }
}
