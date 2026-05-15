using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IDespliegueRepository
{
    Task<IEnumerable<Despliegue>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<IEnumerable<Despliegue>> GetByProjectAsync(Guid proyectoId, int page = 1, int pageSize = 50);
    Task<IEnumerable<Despliegue>> GetRecentAsync(int count);
    Task<Despliegue?> GetByIdAsync(Guid id);
    Task<Despliegue> CreateAsync(Despliegue despliegue);
    Task UpdateAsync(Despliegue despliegue);
    Task<int> GetTotalCountAsync(Guid? proyectoId = null);
    Task<(int total, int exitosos)> GetMonthlyStatsAsync();
}
