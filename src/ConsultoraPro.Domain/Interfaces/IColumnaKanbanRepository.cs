using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IColumnaKanbanRepository
{
    Task<ColumnaKanban?> GetByIdAsync(Guid id);
    Task<List<ColumnaKanban>> GetActiveByTableroAsync(Guid tableroId);
    Task<ColumnaKanban> CreateAsync(ColumnaKanban columna);
    Task UpdateAsync(ColumnaKanban columna);
    Task<int> CountTarjetasActivasAsync(Guid columnaId);
    Task<double> GetMaxOrdenAsync(Guid tableroId);
}
