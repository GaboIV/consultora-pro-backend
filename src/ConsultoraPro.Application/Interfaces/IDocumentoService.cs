using ConsultoraPro.Application.DTOs.Documentos;

namespace ConsultoraPro.Application.Interfaces;

public interface IDocumentoService
{
    /// <summary>Carpetas y documentos del proyecto. Provisiona la estructura corporativa la primera vez.</summary>
    Task<DocumentacionProyectoDto> GetProyectoAsync(Guid proyectoId);
    Task<DocumentoDto> SubirAsync(SubirDocumentoCommand command);
    Task<DocumentoDto> NuevaVersionAsync(Guid documentoId, NuevaVersionCommand command);
    Task<DocumentoDto> ActualizarAsync(Guid documentoId, ActualizarDocumentoDto dto);
    Task EliminarAsync(Guid documentoId);
    Task<DescargaDocumento> DescargarAsync(Guid documentoId, Guid? versionId);

    Task<CarpetaDocumentoDto> CrearCarpetaAsync(CrearCarpetaDto dto);
    Task<CarpetaDocumentoDto> RenombrarCarpetaAsync(Guid carpetaId, RenombrarCarpetaDto dto);
    Task EliminarCarpetaAsync(Guid carpetaId);
}
