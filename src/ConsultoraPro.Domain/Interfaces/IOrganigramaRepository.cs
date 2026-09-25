using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IOrganigramaRepository
{
    Task<IReadOnlyList<Organigrama>> GetAllAsync();

    /// <summary>Por organigrama: total de nodos y cuántos tienen una persona asignada.</summary>
    Task<IReadOnlyDictionary<Guid, (int Nodos, int Asignados)>> GetNodoStatsAsync();

    /// <summary>Obtiene el organigrama con sus nodos y usuarios vinculados, rastreado para edición.</summary>
    Task<Organigrama?> GetByIdAsync(Guid id);

    Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null);

    /// <summary>Devuelve los ids de nodo indicados que ya pertenecen a otro organigrama.</summary>
    Task<IReadOnlyList<Guid>> GetNodoIdsEnOtrosOrganigramasAsync(IEnumerable<Guid> nodoIds, Guid organigramaId);

    Task<IReadOnlyList<ApplicationUser>> GetUsuariosAsync(IEnumerable<Guid> ids);
    Task<IReadOnlyList<ApplicationUser>> GetUsuariosDisponiblesAsync();

    Task CreateAsync(Organigrama organigrama);

    /// <summary>
    /// Persiste los datos del organigrama y reemplaza su árbol por <paramref name="nodos"/>:
    /// actualiza los nodos existentes, agrega los nuevos y elimina los que ya no están.
    /// </summary>
    Task UpdateAsync(Organigrama organigrama, IReadOnlyList<OrganigramaNodo> nodos);

    Task DeleteAsync(Organigrama organigrama);
}
