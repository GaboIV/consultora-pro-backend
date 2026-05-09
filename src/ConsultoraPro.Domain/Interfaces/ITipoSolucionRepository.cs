using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface ITipoSolucionRepository
{
    Task<IEnumerable<TipoSolucion>> GetAllAsync();
    Task<TipoSolucion?> GetByIdAsync(Guid id);
}
