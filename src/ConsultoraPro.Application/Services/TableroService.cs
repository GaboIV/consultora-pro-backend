using ConsultoraPro.Application.DTOs.Kanban;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Application.Kanban;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace ConsultoraPro.Application.Services;

public class TableroService : ITableroService
{
    private static readonly string[] ColumnasPorDefecto = { "Por hacer", "En progreso", "Hecho" };

    private readonly ITableroRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public TableroService(
        ITableroRepository repository,
        IProyectoRepository proyectoRepository,
        UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<TableroDto>> GetByProyectoAsync(Guid proyectoId)
    {
        var tableros = await _repository.GetByProyectoAsync(proyectoId);
        return tableros.Select(KanbanMappers.ToDto);
    }

    public async Task<TableroDetalleDto?> GetDetalleAsync(Guid id)
    {
        var tablero = await _repository.GetDetalleAsync(id);
        return tablero is null || !tablero.Activo ? null : KanbanMappers.ToDetalleDto(tablero);
    }

    public async Task<TableroDto> CreateAsync(CreateTableroDto dto)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(dto.ProyectoId)
            ?? throw new KeyNotFoundException($"Proyecto con ID {dto.ProyectoId} no encontrado");

        await EnsureProyectoClaveAsync(proyecto);

        var claveBase = string.IsNullOrWhiteSpace(dto.Clave)
            ? KanbanCodeHelper.DeriveKey(dto.Nombre, 3)
            : dto.Clave.Trim().ToUpperInvariant();

        var clave = await EnsureUniqueTableroClaveAsync(dto.ProyectoId, claveBase);
        var orden = await _repository.GetMaxOrdenAsync(dto.ProyectoId) + 1;

        var tablero = new Tablero
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            Nombre = dto.Nombre.Trim(),
            Clave = clave,
            Descripcion = dto.Descripcion?.Trim(),
            ColorClass = string.IsNullOrWhiteSpace(dto.ColorClass) ? "blue" : dto.ColorClass.Trim(),
            Orden = orden,
            SecuenciaActual = 0,
            Activo = true
        };

        if (dto.CrearColumnasPorDefecto)
        {
            for (var i = 0; i < ColumnasPorDefecto.Length; i++)
            {
                tablero.Columnas.Add(new ColumnaKanban
                {
                    Id = Guid.NewGuid(),
                    Nombre = ColumnasPorDefecto[i],
                    Orden = (i + 1) * FractionalOrder.Step,
                    Activo = true
                });
            }
        }

        var created = await _repository.CreateAsync(tablero);
        var reloaded = await _repository.GetDetalleAsync(created.Id);
        return KanbanMappers.ToDto(reloaded ?? created);
    }

    public async Task UpdateAsync(Guid id, UpdateTableroDto dto)
    {
        var tablero = await GetActiveTableroAsync(id);

        var clave = dto.Clave.Trim().ToUpperInvariant();
        if (await _repository.ClaveExistsAsync(tablero.ProyectoId, clave, id))
            throw new InvalidOperationException($"Ya existe un tablero con la clave '{clave}' en este proyecto.");

        tablero.Nombre = dto.Nombre.Trim();
        tablero.Clave = clave;
        tablero.Descripcion = dto.Descripcion?.Trim();
        tablero.ColorClass = string.IsNullOrWhiteSpace(dto.ColorClass) ? "blue" : dto.ColorClass.Trim();

        await _repository.UpdateAsync(tablero);
    }

    public async Task DeleteAsync(Guid id)
    {
        var tablero = await GetActiveTableroAsync(id);
        tablero.Activo = false;
        await _repository.UpdateAsync(tablero);
    }

    public async Task<IEnumerable<TableroMiembroDto>> UpdateMiembrosAsync(Guid id, UpdateMiembrosDto dto)
    {
        var tablero = await _repository.GetWithMiembrosAsync(id)
            ?? throw new KeyNotFoundException($"Tablero con ID {id} no encontrado");

        var inputByUser = dto.Miembros
            .GroupBy(m => m.UsuarioId)
            .ToDictionary(g => g.Key, g => g.Last());

        // Eliminar los que ya no están
        var toRemove = tablero.Miembros.Where(m => !inputByUser.ContainsKey(m.UsuarioId)).ToList();
        foreach (var m in toRemove)
            tablero.Miembros.Remove(m);

        // Añadir o actualizar rol
        var existingByUser = tablero.Miembros.ToDictionary(m => m.UsuarioId);
        foreach (var (usuarioId, input) in inputByUser)
        {
            if (existingByUser.TryGetValue(usuarioId, out var existing))
            {
                existing.Rol = input.Rol;
            }
            else
            {
                var user = await _userManager.FindByIdAsync(usuarioId.ToString());
                if (user is null || !user.Activo)
                    throw new KeyNotFoundException($"Usuario con ID {usuarioId} no encontrado o inactivo");

                tablero.Miembros.Add(new TableroMiembro
                {
                    Id = Guid.NewGuid(),
                    TableroId = tablero.Id,
                    UsuarioId = usuarioId,
                    Rol = input.Rol
                });
            }
        }

        await _repository.UpdateAsync(tablero);

        var reloaded = await _repository.GetWithMiembrosAsync(id);
        return (reloaded?.Miembros ?? tablero.Miembros).Select(KanbanMappers.ToDto).ToList();
    }

    public async Task<IEnumerable<EtiquetaDto>> GetEtiquetasAsync(Guid tableroId)
    {
        var tablero = await _repository.GetWithEtiquetasAsync(tableroId)
            ?? throw new KeyNotFoundException($"Tablero con ID {tableroId} no encontrado");

        return tablero.Etiquetas.Where(e => e.Activo).OrderBy(e => e.Nombre).Select(KanbanMappers.ToDto).ToList();
    }

    public async Task<EtiquetaDto> CreateEtiquetaAsync(Guid tableroId, CreateEtiquetaDto dto)
    {
        var tablero = await _repository.GetWithEtiquetasAsync(tableroId)
            ?? throw new KeyNotFoundException($"Tablero con ID {tableroId} no encontrado");

        var etiqueta = new EtiquetaKanban
        {
            Id = Guid.NewGuid(),
            TableroId = tableroId,
            Nombre = dto.Nombre.Trim(),
            ColorClass = string.IsNullOrWhiteSpace(dto.ColorClass) ? "blue" : dto.ColorClass.Trim(),
            Activo = true
        };

        tablero.Etiquetas.Add(etiqueta);
        await _repository.UpdateAsync(tablero);
        return KanbanMappers.ToDto(etiqueta);
    }

    public async Task<EtiquetaDto> UpdateEtiquetaAsync(Guid tableroId, Guid etiquetaId, UpdateEtiquetaDto dto)
    {
        var tablero = await _repository.GetWithEtiquetasAsync(tableroId)
            ?? throw new KeyNotFoundException($"Tablero con ID {tableroId} no encontrado");

        var etiqueta = tablero.Etiquetas.FirstOrDefault(e => e.Id == etiquetaId && e.Activo)
            ?? throw new KeyNotFoundException($"Etiqueta con ID {etiquetaId} no encontrada");

        etiqueta.Nombre = dto.Nombre.Trim();
        etiqueta.ColorClass = string.IsNullOrWhiteSpace(dto.ColorClass) ? "blue" : dto.ColorClass.Trim();

        await _repository.UpdateAsync(tablero);
        return KanbanMappers.ToDto(etiqueta);
    }

    public async Task DeleteEtiquetaAsync(Guid tableroId, Guid etiquetaId)
    {
        var tablero = await _repository.GetWithEtiquetasAsync(tableroId)
            ?? throw new KeyNotFoundException($"Tablero con ID {tableroId} no encontrado");

        var etiqueta = tablero.Etiquetas.FirstOrDefault(e => e.Id == etiquetaId && e.Activo)
            ?? throw new KeyNotFoundException($"Etiqueta con ID {etiquetaId} no encontrada");

        etiqueta.Activo = false;
        await _repository.UpdateAsync(tablero);
    }

    private async Task<Tablero> GetActiveTableroAsync(Guid id)
    {
        var tablero = await _repository.GetByIdAsync(id);
        if (tablero is null || !tablero.Activo)
            throw new KeyNotFoundException($"Tablero con ID {id} no encontrado");
        return tablero;
    }

    private async Task<string> EnsureUniqueTableroClaveAsync(Guid proyectoId, string claveBase)
    {
        var clave = claveBase;
        var suffix = 1;
        while (await _repository.ClaveExistsAsync(proyectoId, clave))
        {
            clave = $"{claveBase}{suffix}";
            suffix++;
        }
        return clave;
    }

    private async Task EnsureProyectoClaveAsync(Proyecto proyecto)
    {
        if (!string.IsNullOrWhiteSpace(proyecto.Clave))
            return;

        var existing = (await _proyectoRepository.GetAllAsync())
            .Select(p => p.Clave)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var baseKey = KanbanCodeHelper.DeriveKey(proyecto.Nombre, 3);
        var clave = baseKey;
        var suffix = 1;
        while (existing.Contains(clave))
        {
            clave = $"{baseKey}{suffix}";
            suffix++;
        }

        proyecto.Clave = clave;
        await _proyectoRepository.UpdateAsync(proyecto);
    }
}
