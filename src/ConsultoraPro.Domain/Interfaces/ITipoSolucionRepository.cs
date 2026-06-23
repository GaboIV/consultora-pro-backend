using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface ITipoSolucionRepository
{
    Task<IEnumerable<TipoSolucion>> GetAllAsync();
    Task<TipoSolucion?> GetByIdAsync(Guid id);
    Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null);
    Task<int> CountProyectosAsync(Guid id);
    Task<IReadOnlyDictionary<Guid, int>> GetProyectoCountsAsync();
    Task<TipoSolucion> CreateAsync(TipoSolucion tipoSolucion);
    Task UpdateAsync(TipoSolucion tipoSolucion);
    Task DeleteAsync(TipoSolucion tipoSolucion);
}
