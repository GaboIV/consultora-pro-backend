using ConsultoraPro.Application.DTOs.TiposSolucion;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class TipoSolucionService : ITipoSolucionService
{
    private readonly ITipoSolucionRepository _repository;

    public TipoSolucionService(ITipoSolucionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TipoSolucionDto>> GetAllAsync()
    {
        var tipos = await _repository.GetAllAsync();
        var counts = await _repository.GetProyectoCountsAsync();

        return tipos
            .Select(t => new TipoSolucionDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Activo = t.Activo,
                TotalProyectos = counts.TryGetValue(t.Id, out var total) ? total : 0
            })
            .ToList();
    }

    public async Task<TipoSolucionDto?> GetByIdAsync(Guid id)
    {
        var tipo = await _repository.GetByIdAsync(id);
        if (tipo == null) return null;

        return new TipoSolucionDto
        {
            Id = tipo.Id,
            Nombre = tipo.Nombre,
            Activo = tipo.Activo,
            TotalProyectos = await _repository.CountProyectosAsync(id)
        };
    }

    public async Task<TipoSolucionDto> CreateAsync(CreateTipoSolucionDto dto)
    {
        var nombre = dto.Nombre.Trim();

        if (await _repository.ExistsByNombreAsync(nombre))
            throw new InvalidOperationException($"Ya existe un tipo de solución con el nombre '{nombre}'.");

        var tipo = new TipoSolucion
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Activo = dto.Activo
        };

        var created = await _repository.CreateAsync(tipo);

        return new TipoSolucionDto
        {
            Id = created.Id,
            Nombre = created.Nombre,
            Activo = created.Activo,
            TotalProyectos = 0
        };
    }

    public async Task UpdateAsync(Guid id, UpdateTipoSolucionDto dto)
    {
        var tipo = await _repository.GetByIdAsync(id);
        if (tipo == null)
            throw new KeyNotFoundException($"Tipo de solución con ID {id} no encontrado");

        var nombre = dto.Nombre.Trim();

        if (await _repository.ExistsByNombreAsync(nombre, id))
            throw new InvalidOperationException($"Ya existe un tipo de solución con el nombre '{nombre}'.");

        tipo.Nombre = nombre;
        tipo.Activo = dto.Activo;
        await _repository.UpdateAsync(tipo);
    }

    public async Task DeleteAsync(Guid id)
    {
        var tipo = await _repository.GetByIdAsync(id);
        if (tipo == null)
            throw new KeyNotFoundException($"Tipo de solución con ID {id} no encontrado");

        // Protección de integridad: no se elimina un tipo que tenga proyectos asociados,
        // para no romper la relación TipoSolucion -> Proyecto.
        var proyectosAsociados = await _repository.CountProyectosAsync(id);
        if (proyectosAsociados > 0)
            throw new InvalidOperationException(
                $"No se puede eliminar el tipo de solución porque tiene {proyectosAsociados} proyecto(s) asociado(s).");

        await _repository.DeleteAsync(tipo);
    }
}
