using ConsultoraPro.Application.DTOs.Kanban;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Application.Kanban;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class ColumnaService : IColumnaService
{
    private readonly IColumnaKanbanRepository _repository;
    private readonly ITableroRepository _tableroRepository;
    private readonly IKanbanAccessGuard _accessGuard;

    public ColumnaService(
        IColumnaKanbanRepository repository,
        ITableroRepository tableroRepository,
        IKanbanAccessGuard accessGuard)
    {
        _repository = repository;
        _tableroRepository = tableroRepository;
        _accessGuard = accessGuard;
    }

    public async Task<ColumnaDto> CreateAsync(CreateColumnaDto dto)
    {
        await _accessGuard.EnsureTableroAccessAsync(dto.TableroId);

        var tablero = await _tableroRepository.GetByIdAsync(dto.TableroId);
        if (tablero is null || !tablero.Activo)
            throw new KeyNotFoundException($"Tablero con ID {dto.TableroId} no encontrado");

        var maxOrden = await _repository.GetMaxOrdenAsync(dto.TableroId);

        var columna = new ColumnaKanban
        {
            Id = Guid.NewGuid(),
            TableroId = dto.TableroId,
            Nombre = dto.Nombre.Trim(),
            LimiteWip = dto.LimiteWip,
            Orden = maxOrden + FractionalOrder.Step,
            Activo = true
        };

        var created = await _repository.CreateAsync(columna);
        return KanbanMappers.ToDto(created);
    }

    public async Task UpdateAsync(Guid id, UpdateColumnaDto dto)
    {
        var columna = await GetActiveColumnaAsync(id);
        await _accessGuard.EnsureTableroAccessAsync(columna.TableroId);
        columna.Nombre = dto.Nombre.Trim();
        columna.LimiteWip = dto.LimiteWip;
        await _repository.UpdateAsync(columna);
    }

    public async Task ReordenarAsync(Guid id, ReordenarColumnaDto dto)
    {
        var columna = await GetActiveColumnaAsync(id);
        await _accessGuard.EnsureTableroAccessAsync(columna.TableroId);

        var hermanas = await _repository.GetActiveByTableroAsync(columna.TableroId);
        double? antes = dto.AntesDeColumnaId is { } a
            ? hermanas.FirstOrDefault(c => c.Id == a)?.Orden
            : null;
        double? despues = dto.DespuesDeColumnaId is { } d
            ? hermanas.FirstOrDefault(c => c.Id == d)?.Orden
            : null;

        columna.Orden = FractionalOrder.Between(antes, despues);
        await _repository.UpdateAsync(columna);
    }

    public async Task DeleteAsync(Guid id)
    {
        var columna = await GetActiveColumnaAsync(id);
        await _accessGuard.EnsureTableroAccessAsync(columna.TableroId);

        var tarjetas = await _repository.CountTarjetasActivasAsync(id);
        if (tarjetas > 0)
            throw new InvalidOperationException("No se puede eliminar una columna con tarjetas. Mueva o archive las tarjetas primero.");

        columna.Activo = false;
        await _repository.UpdateAsync(columna);
    }

    private async Task<ColumnaKanban> GetActiveColumnaAsync(Guid id)
    {
        var columna = await _repository.GetByIdAsync(id);
        if (columna is null || !columna.Activo)
            throw new KeyNotFoundException($"Columna con ID {id} no encontrada");
        return columna;
    }
}
