using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface ITableroRepository
{
    Task<IEnumerable<Tablero>> GetByProyectoAsync(Guid proyectoId);
    Task<Tablero?> GetByIdAsync(Guid id);
    Task<Tablero?> GetDetalleAsync(Guid id);
    Task<Tablero?> GetWithMiembrosAsync(Guid id);
    Task<Tablero?> GetWithEtiquetasAsync(Guid id);
    Task<Tablero> CreateAsync(Tablero tablero);
    Task UpdateAsync(Tablero tablero);
    Task<bool> ClaveExistsAsync(Guid proyectoId, string clave, Guid? excludeId = null);
    Task<int> GetMaxOrdenAsync(Guid proyectoId);
    Task<EtiquetaKanban?> GetEtiquetaAsync(Guid etiquetaId);
}
