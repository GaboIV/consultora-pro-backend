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

    /// <summary>
    /// Marca las entidades hijas indicadas como nuevas (estado Added) y persiste, junto con
    /// cualquier otro cambio pendiente en la unidad de trabajo (ediciones, eliminaciones).
    /// Es necesario porque añadir una entidad con su clave (Guid) ya asignada a la colección
    /// de navegación de una entidad ya rastreada hace que EF la interprete como una
    /// modificación (UPDATE ... WHERE Id = nuevo) en lugar de una inserción, lo que provoca
    /// DbUpdateConcurrencyException ("expected to affect 1 row, but actually affected 0").
    /// </summary>
    Task AddChildrenAndSaveAsync(params object[] entidadesNuevas);

    Task AddActividadAsync(ActividadTarjeta actividad);
    Task<List<ActividadTarjeta>> GetActividadAsync(Guid tarjetaId);
}
