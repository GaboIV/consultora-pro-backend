using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IDocumentoRepository
{
    // --- Carpetas ---
    Task<List<CarpetaDocumento>> GetCarpetasAsync(Guid proyectoId);
    Task<CarpetaDocumento?> GetCarpetaAsync(Guid id);
    Task AddCarpetasAsync(IEnumerable<CarpetaDocumento> carpetas);
    Task DeleteCarpetaAsync(CarpetaDocumento carpeta);
    Task<bool> CarpetaTieneContenidoAsync(Guid carpetaId);

    // --- Documentos ---
    Task<List<Documento>> GetByProyectoAsync(Guid proyectoId);
    Task<Documento?> GetByIdAsync(Guid id);
    Task<DocumentoVersion?> GetVersionAsync(Guid documentoId, Guid versionId);
    Task<int> CountByTipoAsync(Guid proyectoId, TipoDocumento tipo);
    Task AddAsync(Documento documento);

    /// <summary>
    /// Búsqueda para el buscador global. Cada término debe aparecer (LIKE) en título, código,
    /// archivo, descripción, carpeta o etiquetas. <paramref name="proyectoIds"/> null = sin restricción.
    /// </summary>
    Task<List<Documento>> SearchAsync(IReadOnlyCollection<string> terms, IReadOnlySet<Guid>? proyectoIds, int take);

    Task SaveChangesAsync();
}
