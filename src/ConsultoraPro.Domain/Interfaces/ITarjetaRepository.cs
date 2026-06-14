using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface ITarjetaRepository
{
    Task<Tarjeta?> GetByIdAsync(Guid id);
    Task<Tarjeta?> GetDetalleAsync(Guid id);
    Task<Tarjeta?> GetWithResponsablesAsync(Guid id);
    Task<Tarjeta?> GetWithEtiquetasAsync(Guid id);
    Task<Tarjeta?> GetWithChecklistAsync(Guid id);
    Task<Tarjeta?> GetWithComentariosAsync(Guid id);
    Task<Tarjeta?> GetWithAdjuntosAsync(Guid id);
    Task<List<Tarjeta>> GetActiveByColumnaAsync(Guid columnaId);

    /// <summary>
    /// Crea la tarjeta generando su código de forma atómica: bloquea la fila del tablero
    /// (SELECT ... FOR UPDATE), incrementa la secuencia y persiste todo en una transacción.
    /// </summary>
    Task<Tarjeta> CreateAsync(Tarjeta tarjeta);

    Task UpdateAsync(Tarjeta tarjeta);

    Task AddActividadAsync(ActividadTarjeta actividad);
    Task<List<ActividadTarjeta>> GetActividadAsync(Guid tarjetaId);
}
