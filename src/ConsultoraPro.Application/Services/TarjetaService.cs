using ConsultoraPro.Application.DTOs.Kanban;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Application.Kanban;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace ConsultoraPro.Application.Services;

public class TarjetaService : ITarjetaService
{
    private readonly ITarjetaRepository _repository;
    private readonly IColumnaKanbanRepository _columnaRepository;
    private readonly ITableroRepository _tableroRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStorageService _storageService;
    private readonly IFileUrlResolver _urlResolver;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly ICurrentUserService _currentUserService;

    public TarjetaService(
        ITarjetaRepository repository,
        IColumnaKanbanRepository columnaRepository,
        ITableroRepository tableroRepository,
        UserManager<ApplicationUser> userManager,
        IStorageService storageService,
        IFileUrlResolver urlResolver,
        IProyectoRepository proyectoRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _columnaRepository = columnaRepository;
        _tableroRepository = tableroRepository;
        _userManager = userManager;
        _storageService = storageService;
        _urlResolver = urlResolver;
        _proyectoRepository = proyectoRepository;
        _currentUserService = currentUserService;
    }

    public async Task<TarjetaDetalleDto?> GetByIdAsync(Guid id)
    {
        var tarjeta = await _repository.GetDetalleAsync(id);
        if (tarjeta is null || !tarjeta.Activo)
            return null;

        if (_currentUserService.IsInRole("Soporte") && tarjeta.Tablero.ProyectoId.HasValue)
        {
            var proyecto = await _proyectoRepository.GetByIdAsync(tarjeta.Tablero.ProyectoId.Value);
            var isMember = proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId) ?? false;
            if (!isMember) return null;
        }

        var dto = KanbanMappers.ToDetalleDto(tarjeta);
        // Las keys de almacenamiento se firman (SAS) en lectura: adjuntos, portada e imágenes inline.
        dto.Descripcion = await _urlResolver.ResolveContentAsync(dto.Descripcion);
        dto.PortadaAdjuntoUrl = await _urlResolver.ResolveAsync(dto.PortadaAdjuntoUrl);
        foreach (var adjunto in dto.Adjuntos)
            adjunto.Url = await _urlResolver.ResolveAsync(adjunto.Url) ?? adjunto.Url;
        foreach (var comentario in dto.Comentarios)
            comentario.Texto = await _urlResolver.ResolveContentAsync(comentario.Texto) ?? comentario.Texto;

        return dto;
    }

    public async Task<TarjetaDetalleDto> CreateAsync(CreateTarjetaDto dto, Guid usuarioId)
    {
        var columna = await _columnaRepository.GetByIdAsync(dto.ColumnaId);
        if (columna is null || !columna.Activo)
            throw new KeyNotFoundException($"Columna con ID {dto.ColumnaId} no encontrada");
        if (columna.Tablero is null || !columna.Tablero.Activo)
            throw new KeyNotFoundException("El tablero asociado no está disponible");

        await ValidateTableroAccessAsync(columna.TableroId);

        var existentes = await _repository.GetActiveByColumnaAsync(dto.ColumnaId);
        var orden = (existentes.LastOrDefault()?.Orden ?? 0d) + FractionalOrder.Step;

        var tarjeta = new Tarjeta
        {
            Id = Guid.NewGuid(),
            ColumnaId = dto.ColumnaId,
            TableroId = columna.TableroId,
            Titulo = dto.Titulo.Trim(),
            // Normaliza URLs de imágenes inline a placeholders estables antes de persistir.
            Descripcion = _urlResolver.ToStoragePlaceholders(dto.Descripcion?.Trim()),
            Prioridad = dto.Prioridad,
            FechaLimite = ToUtc(dto.FechaLimite),
            FechaInicio = ToUtc(dto.FechaInicio),
            Orden = orden,
            CreadaPorId = usuarioId,
            Activo = true
        };

        var responsableIds = dto.ResponsableIds ?? new List<Guid>();
        if (columna.Tablero.ProyectoId == null)
        {
            var list = responsableIds.ToList();
            if (!list.Contains(usuarioId))
            {
                list.Add(usuarioId);
            }
            responsableIds = list;
        }

        foreach (var responsableId in responsableIds.Distinct())
        {
            await EnsureUsuarioActivoAsync(responsableId);
            tarjeta.Responsables.Add(new TarjetaResponsable
            {
                Id = Guid.NewGuid(),
                TarjetaId = tarjeta.Id,
                UsuarioId = responsableId
            });
        }

        foreach (var etiquetaId in dto.EtiquetaIds.Distinct())
        {
            await EnsureEtiquetaPerteneceAsync(etiquetaId, columna.TableroId);
            tarjeta.Etiquetas.Add(new TarjetaEtiqueta
            {
                TarjetaId = tarjeta.Id,
                EtiquetaId = etiquetaId
            });
        }

        tarjeta.Actividades.Add(new ActividadTarjeta
        {
            Id = Guid.NewGuid(),
            TarjetaId = tarjeta.Id,
            UsuarioId = usuarioId,
            Tipo = TipoActividadTarjeta.Creada,
            Detalle = $"Creó la tarjeta «{tarjeta.Titulo}»"
        });

        var created = await _repository.CreateAsync(tarjeta);
        var reloaded = await _repository.GetDetalleAsync(created.Id);
        return KanbanMappers.ToDetalleDto(reloaded ?? created);
    }

    public async Task UpdateAsync(Guid id, UpdateTarjetaDto dto, Guid usuarioId)
    {
        var tarjeta = await GetActiveTarjetaAsync(id);
        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var antesCompletada = tarjeta.Completada;

        tarjeta.Titulo = dto.Titulo.Trim();
        tarjeta.Descripcion = _urlResolver.ToStoragePlaceholders(dto.Descripcion?.Trim());
        tarjeta.Prioridad = dto.Prioridad;
        tarjeta.FechaLimite = ToUtc(dto.FechaLimite);
        tarjeta.FechaInicio = ToUtc(dto.FechaInicio);
        tarjeta.Completada = dto.Completada;

        await _repository.UpdateAsync(tarjeta);

        if (antesCompletada != dto.Completada)
            await LogAsync(id, usuarioId, dto.Completada ? TipoActividadTarjeta.Completada : TipoActividadTarjeta.Reabierta);
        else
            await LogAsync(id, usuarioId, TipoActividadTarjeta.Editada);
    }

    public async Task MoverAsync(Guid id, MoverTarjetaDto dto, Guid usuarioId)
    {
        var tarjeta = await GetActiveTarjetaAsync(id);
        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var destino = await _columnaRepository.GetByIdAsync(dto.ColumnaDestinoId);
        if (destino is null || !destino.Activo)
            throw new KeyNotFoundException($"Columna destino con ID {dto.ColumnaDestinoId} no encontrada");

        // F1: una tarjeta no cambia de tablero (su código queda ligado al tablero original, D4).
        if (destino.TableroId != tarjeta.TableroId)
            throw new InvalidOperationException("No se puede mover una tarjeta a un tablero distinto.");

        var enDestino = await _repository.GetActiveByColumnaAsync(destino.Id);
        double? antes = dto.AntesDeTarjetaId is { } a
            ? enDestino.FirstOrDefault(t => t.Id == a)?.Orden
            : null;
        double? despues = dto.DespuesDeTarjetaId is { } d
            ? enDestino.FirstOrDefault(t => t.Id == d)?.Orden
            : null;

        tarjeta.ColumnaId = destino.Id;
        tarjeta.Orden = FractionalOrder.Between(antes, despues);

        // La columna "terminal" (mayor orden) marca la tarjeta como completada automáticamente.
        var columnas = await _columnaRepository.GetActiveByTableroAsync(tarjeta.TableroId);
        var terminal = columnas.OrderBy(c => c.Orden).LastOrDefault();
        if (terminal is not null)
            tarjeta.Completada = terminal.Id == destino.Id;

        await _repository.UpdateAsync(tarjeta);
        await LogAsync(id, usuarioId, TipoActividadTarjeta.Movida, $"Movió la tarjeta a «{destino.Nombre}»");
    }

    public async Task DeleteAsync(Guid id, Guid usuarioId)
    {
        var tarjeta = await GetActiveTarjetaAsync(id);
        await ValidateTableroAccessAsync(tarjeta.TableroId);
        tarjeta.Activo = false;
        await _repository.UpdateAsync(tarjeta);
        await LogAsync(id, usuarioId, TipoActividadTarjeta.Archivada);
    }

    public async Task<IEnumerable<ResponsableDto>> AsignarResponsablesAsync(Guid id, AsignarResponsablesDto dto, Guid usuarioId)
    {
        var tarjeta = await _repository.GetWithResponsablesAsync(id);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {id} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var deseados = dto.UsuarioIds.Distinct().ToList();

        var aQuitar = tarjeta.Responsables.Where(r => !deseados.Contains(r.UsuarioId)).ToList();
        foreach (var r in aQuitar)
            tarjeta.Responsables.Remove(r);

        var actuales = tarjeta.Responsables.Select(r => r.UsuarioId).ToHashSet();
        var nuevos = new List<object>();
        foreach (var usuario in deseados.Where(u => !actuales.Contains(u)))
        {
            await EnsureUsuarioActivoAsync(usuario);
            var responsable = new TarjetaResponsable
            {
                Id = Guid.NewGuid(),
                TarjetaId = tarjeta.Id,
                UsuarioId = usuario
            };
            tarjeta.Responsables.Add(responsable);
            nuevos.Add(responsable);
        }

        // AddChildrenAndSaveAsync inserta los responsables nuevos y, en el mismo SaveChanges,
        // aplica las eliminaciones marcadas arriba sobre el grafo rastreado.
        await _repository.AddChildrenAndSaveAsync(nuevos.ToArray());
        await LogAsync(id, usuarioId, TipoActividadTarjeta.Asignada, "Actualizó los responsables");

        var reloaded = await _repository.GetWithResponsablesAsync(id);
        return (reloaded?.Responsables ?? tarjeta.Responsables).Select(KanbanMappers.ToDto).ToList();
    }

    public async Task<IEnumerable<EtiquetaDto>> AsignarEtiquetasAsync(Guid id, AsignarEtiquetasDto dto, Guid usuarioId)
    {
        var tarjeta = await _repository.GetWithEtiquetasAsync(id);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {id} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var deseadas = dto.EtiquetaIds.Distinct().ToList();

        var aQuitar = tarjeta.Etiquetas.Where(e => !deseadas.Contains(e.EtiquetaId)).ToList();
        foreach (var e in aQuitar)
            tarjeta.Etiquetas.Remove(e);

        var actuales = tarjeta.Etiquetas.Select(e => e.EtiquetaId).ToHashSet();
        var nuevas = new List<object>();
        foreach (var etiquetaId in deseadas.Where(e => !actuales.Contains(e)))
        {
            await EnsureEtiquetaPerteneceAsync(etiquetaId, tarjeta.TableroId);
            var relacion = new TarjetaEtiqueta
            {
                TarjetaId = tarjeta.Id,
                EtiquetaId = etiquetaId
            };
            tarjeta.Etiquetas.Add(relacion);
            nuevas.Add(relacion);
        }

        // La clave de TarjetaEtiqueta es compuesta (TarjetaId, EtiquetaId) y siempre va asignada,
        // por lo que también requiere inserción explícita en estado Added.
        await _repository.AddChildrenAndSaveAsync(nuevas.ToArray());
        await LogAsync(id, usuarioId, TipoActividadTarjeta.EtiquetaAgregada, "Actualizó las etiquetas");

        var reloaded = await _repository.GetWithEtiquetasAsync(id);
        return (reloaded?.Etiquetas ?? tarjeta.Etiquetas)
            .Where(e => e.Etiqueta is not null)
            .Select(e => KanbanMappers.ToDto(e.Etiqueta))
            .ToList();
    }

    public async Task<ChecklistDto> AddChecklistAsync(Guid tarjetaId, CreateChecklistDto dto)
    {
        var tarjeta = await _repository.GetWithChecklistAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var maxOrden = tarjeta.Checklists.Count == 0 ? 0d : tarjeta.Checklists.Max(c => c.Orden);
        var checklist = new Checklist
        {
            Id = Guid.NewGuid(),
            TarjetaId = tarjetaId,
            Nombre = dto.Nombre.Trim(),
            Orden = maxOrden + FractionalOrder.Step
        };

        tarjeta.Checklists.Add(checklist);
        await _repository.AddChildrenAndSaveAsync(checklist);
        return KanbanMappers.ToDto(checklist);
    }

    public async Task<ChecklistDto> UpdateChecklistAsync(Guid tarjetaId, Guid checklistId, UpdateChecklistDto dto)
    {
        var tarjeta = await _repository.GetWithChecklistAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var checklist = GetChecklistOrThrow(tarjeta, checklistId);
        checklist.Nombre = dto.Nombre.Trim();

        await _repository.UpdateAsync(tarjeta);
        return KanbanMappers.ToDto(checklist);
    }

    public async Task DeleteChecklistAsync(Guid tarjetaId, Guid checklistId)
    {
        var tarjeta = await _repository.GetWithChecklistAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var checklist = GetChecklistOrThrow(tarjeta, checklistId);
        tarjeta.Checklists.Remove(checklist);
        await _repository.UpdateAsync(tarjeta);
    }

    public async Task<ChecklistItemDto> AddChecklistItemAsync(Guid tarjetaId, Guid checklistId, CreateChecklistItemDto dto)
    {
        var tarjeta = await _repository.GetWithChecklistAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var checklist = GetChecklistOrThrow(tarjeta, checklistId);

        var maxOrden = checklist.Items.Count == 0 ? 0d : checklist.Items.Max(c => c.Orden);
        var item = new ChecklistItem
        {
            Id = Guid.NewGuid(),
            ChecklistId = checklist.Id,
            Texto = dto.Texto.Trim(),
            Completado = false,
            Orden = maxOrden + FractionalOrder.Step
        };

        checklist.Items.Add(item);
        await _repository.AddChildrenAndSaveAsync(item);
        return KanbanMappers.ToDto(item);
    }

    public async Task<ChecklistItemDto> UpdateChecklistItemAsync(Guid tarjetaId, Guid checklistId, Guid itemId, UpdateChecklistItemDto dto)
    {
        var tarjeta = await _repository.GetWithChecklistAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var checklist = GetChecklistOrThrow(tarjeta, checklistId);
        var item = checklist.Items.FirstOrDefault(c => c.Id == itemId)
            ?? throw new KeyNotFoundException($"Ítem de checklist con ID {itemId} no encontrado");

        if (dto.Texto is not null)
            item.Texto = dto.Texto.Trim();
        if (dto.Completado.HasValue)
            item.Completado = dto.Completado.Value;

        await _repository.UpdateAsync(tarjeta);
        return KanbanMappers.ToDto(item);
    }

    public async Task DeleteChecklistItemAsync(Guid tarjetaId, Guid checklistId, Guid itemId)
    {
        var tarjeta = await _repository.GetWithChecklistAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var checklist = GetChecklistOrThrow(tarjeta, checklistId);
        var item = checklist.Items.FirstOrDefault(c => c.Id == itemId)
            ?? throw new KeyNotFoundException($"Ítem de checklist con ID {itemId} no encontrado");

        checklist.Items.Remove(item);
        await _repository.UpdateAsync(tarjeta);
    }

    private static Checklist GetChecklistOrThrow(Tarjeta tarjeta, Guid checklistId)
        => tarjeta.Checklists.FirstOrDefault(c => c.Id == checklistId)
            ?? throw new KeyNotFoundException($"Checklist con ID {checklistId} no encontrado");

    public async Task<ComentarioDto> AddComentarioAsync(Guid tarjetaId, CreateComentarioDto dto, Guid usuarioId)
    {
        var tarjeta = await _repository.GetWithComentariosAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var autor = await _userManager.FindByIdAsync(usuarioId.ToString())
            ?? throw new KeyNotFoundException("Usuario autor no encontrado");

        var comentario = new ComentarioTarjeta
        {
            Id = Guid.NewGuid(),
            TarjetaId = tarjetaId,
            AutorId = usuarioId,
            Texto = _urlResolver.ToStoragePlaceholders(dto.Texto.Trim()) ?? dto.Texto.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        var actividad = new ActividadTarjeta
        {
            Id = Guid.NewGuid(),
            TarjetaId = tarjetaId,
            UsuarioId = usuarioId,
            Tipo = TipoActividadTarjeta.Comentada
        };

        // Insertamos el comentario y la actividad como entidades nuevas (Added). Añadirlas solo
        // a las colecciones de navegación no basta: EF las trataría como UPDATE por tener clave.
        tarjeta.Comentarios.Add(comentario);
        tarjeta.Actividades.Add(actividad);
        await _repository.AddChildrenAndSaveAsync(comentario, actividad);

        return new ComentarioDto
        {
            Id = comentario.Id,
            Texto = await _urlResolver.ResolveContentAsync(comentario.Texto) ?? comentario.Texto,
            AutorId = usuarioId,
            AutorNombre = $"{autor.Nombres} {autor.Apellidos}".Trim(),
            AutorIniciales = autor.Iniciales,
            FechaCreacion = comentario.FechaCreacion,
            EditadoEn = comentario.EditadoEn
        };
    }

    public async Task<ComentarioDto> UpdateComentarioAsync(Guid tarjetaId, Guid comentarioId, UpdateComentarioDto dto, Guid usuarioId)
    {
        var tarjeta = await _repository.GetWithComentariosAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var comentario = tarjeta.Comentarios.FirstOrDefault(c => c.Id == comentarioId)
            ?? throw new KeyNotFoundException($"Comentario con ID {comentarioId} no encontrado");

        if (comentario.AutorId != usuarioId)
            throw new UnauthorizedAccessException("No tienes permisos para editar este comentario.");

        if (comentario.FechaCreacion.Date != DateTime.UtcNow.Date)
            throw new InvalidOperationException("Solo se pueden editar comentarios durante el día de su creación.");

        comentario.Texto = _urlResolver.ToStoragePlaceholders(dto.Texto.Trim()) ?? dto.Texto.Trim();
        comentario.EditadoEn = DateTime.UtcNow;

        await _repository.UpdateAsync(tarjeta);

        var autor = await _userManager.FindByIdAsync(usuarioId.ToString())
            ?? throw new KeyNotFoundException("Usuario autor no encontrado");

        return new ComentarioDto
        {
            Id = comentario.Id,
            Texto = await _urlResolver.ResolveContentAsync(comentario.Texto) ?? comentario.Texto,
            AutorId = usuarioId,
            AutorNombre = $"{autor.Nombres} {autor.Apellidos}".Trim(),
            AutorIniciales = autor.Iniciales,
            FechaCreacion = comentario.FechaCreacion,
            EditadoEn = comentario.EditadoEn
        };
    }

    public async Task DeleteComentarioAsync(Guid tarjetaId, Guid comentarioId)
    {
        var tarjeta = await _repository.GetWithComentariosAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var comentario = tarjeta.Comentarios.FirstOrDefault(c => c.Id == comentarioId)
            ?? throw new KeyNotFoundException($"Comentario con ID {comentarioId} no encontrado");

        tarjeta.Comentarios.Remove(comentario);
        await _repository.UpdateAsync(tarjeta);
    }

    public async Task<AdjuntoDto> AddAdjuntoAsync(Guid tarjetaId, string nombre, string storageKey, string? contentType, long tamanoBytes, Guid usuarioId)
    {
        var tarjeta = await _repository.GetWithAdjuntosAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());

        var adjunto = new AdjuntoTarjeta
        {
            Id = Guid.NewGuid(),
            TarjetaId = tarjetaId,
            Nombre = nombre.Trim(),
            StorageKey = storageKey,
            ContentType = contentType,
            TamanoBytes = tamanoBytes,
            SubidoPorId = usuarioId,
            FechaSubida = DateTime.UtcNow
        };

        tarjeta.Adjuntos.Add(adjunto);
        await _repository.AddChildrenAndSaveAsync(adjunto);

        return new AdjuntoDto
        {
            Id = adjunto.Id,
            Nombre = adjunto.Nombre,
            Url = await _urlResolver.ResolveAsync(adjunto.StorageKey) ?? adjunto.StorageKey,
            ContentType = adjunto.ContentType,
            TamanoBytes = adjunto.TamanoBytes,
            SubidoPorId = usuarioId,
            SubidoPorNombre = usuario is null ? null : $"{usuario.Nombres} {usuario.Apellidos}".Trim(),
            FechaSubida = adjunto.FechaSubida
        };
    }

    public async Task DeleteAdjuntoAsync(Guid tarjetaId, Guid adjuntoId)
    {
        var tarjeta = await _repository.GetWithAdjuntosAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var adjunto = tarjeta.Adjuntos.FirstOrDefault(a => a.Id == adjuntoId)
            ?? throw new KeyNotFoundException($"Adjunto con ID {adjuntoId} no encontrado");

        await _storageService.DeleteFileAsync(adjunto.StorageKey);
        tarjeta.Adjuntos.Remove(adjunto);
        await _repository.UpdateAsync(tarjeta);
    }

    public async Task<IEnumerable<ActividadDto>> GetActividadAsync(Guid tarjetaId)
    {
        var tarjeta = await _repository.GetByIdAsync(tarjetaId);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {tarjetaId} no encontrada");

        await ValidateTableroAccessAsync(tarjeta.TableroId);

        var actividades = await _repository.GetActividadAsync(tarjetaId);
        return actividades.Select(KanbanMappers.ToDto).ToList();
    }

    private async Task<Tarjeta> GetActiveTarjetaAsync(Guid id)
    {
        var tarjeta = await _repository.GetByIdAsync(id);
        if (tarjeta is null || !tarjeta.Activo)
            throw new KeyNotFoundException($"Tarjeta con ID {id} no encontrada");
        return tarjeta;
    }

    private async Task EnsureUsuarioActivoAsync(Guid usuarioId)
    {
        var user = await _userManager.FindByIdAsync(usuarioId.ToString());
        if (user is null || !user.Activo)
            throw new KeyNotFoundException($"Usuario con ID {usuarioId} no encontrado o inactivo");
    }

    private async Task EnsureEtiquetaPerteneceAsync(Guid etiquetaId, Guid tableroId)
    {
        var etiqueta = await _tableroRepository.GetEtiquetaAsync(etiquetaId);
        if (etiqueta is null || !etiqueta.Activo || etiqueta.TableroId != tableroId)
            throw new KeyNotFoundException($"Etiqueta con ID {etiquetaId} no pertenece al tablero de la tarjeta");
    }

    private Task LogAsync(Guid tarjetaId, Guid usuarioId, TipoActividadTarjeta tipo, string? detalle = null)
    {
        return _repository.AddActividadAsync(new ActividadTarjeta
        {
            Id = Guid.NewGuid(),
            TarjetaId = tarjetaId,
            UsuarioId = usuarioId,
            Tipo = tipo,
            Detalle = detalle
        });
    }

    private async Task ValidateTableroAccessAsync(Guid tableroId)
    {
        if (_currentUserService.IsInRole("Soporte"))
        {
            var tablero = await _tableroRepository.GetByIdAsync(tableroId);
            if (tablero?.ProyectoId.HasValue == true)
            {
                var proyecto = await _proyectoRepository.GetByIdAsync(tablero.ProyectoId.Value);
                var isMember = proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId) ?? false;
                if (!isMember)
                    throw new UnauthorizedAccessException("No tienes acceso a este proyecto.");
            }
        }
    }

    private static DateTime? ToUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}
