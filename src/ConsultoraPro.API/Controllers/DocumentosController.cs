using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Documentos;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentosController : ControllerBase
{
    // Holgura sobre Storage:Limits:MaxDocumentBytes (50 MB) para el overhead multipart; el tope
    // real por archivo lo valida el servicio. Kestrel limita a 30 MB por defecto.
    private const long MaxRequestBytes = 60 * 1024 * 1024;

    private readonly IDocumentoService _documentoService;

    public DocumentosController(IDocumentoService documentoService)
    {
        _documentoService = documentoService;
    }

    [HttpGet("proyecto/{proyectoId}")]
    [Authorize(Policy = "documentos.ver")]
    public async Task<ActionResult<ApiResponse<DocumentacionProyectoDto>>> GetByProyecto(Guid proyectoId)
    {
        var data = await _documentoService.GetProyectoAsync(proyectoId);
        return Ok(new ApiResponse<DocumentacionProyectoDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "documentos.editar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<ApiResponse<DocumentoDto>>> Upload(
        [FromForm] Guid proyectoId,
        [FromForm] Guid carpetaId,
        [FromForm] string? titulo,
        [FromForm] TipoDocumento tipo,
        [FromForm] EstadoDocumento estado,
        [FromForm] string? version,
        [FromForm] string? descripcion,
        [FromForm] List<string>? etiquetas,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ApiResponse<DocumentoDto> { Success = false, Message = "No se proporcionó ningún archivo" });

        await using var stream = file.OpenReadStream();
        var data = await _documentoService.SubirAsync(new SubirDocumentoCommand
        {
            ProyectoId = proyectoId,
            CarpetaId = carpetaId,
            Titulo = titulo,
            Tipo = tipo,
            Estado = estado,
            Version = version,
            Descripcion = descripcion,
            Etiquetas = etiquetas ?? new(),
            Archivo = ToArchivo(file, stream)
        });

        return Ok(new ApiResponse<DocumentoDto> { Success = true, Data = data, Message = "Documento subido exitosamente" });
    }

    [HttpPost("{id}/versiones")]
    [Authorize(Policy = "documentos.editar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<ApiResponse<DocumentoDto>>> UploadVersion(
        Guid id,
        [FromForm] string? version,
        [FromForm] string? nota,
        [FromForm] EstadoDocumento? estado,
        IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ApiResponse<DocumentoDto> { Success = false, Message = "No se proporcionó ningún archivo" });

        await using var stream = file.OpenReadStream();
        var data = await _documentoService.NuevaVersionAsync(id, new NuevaVersionCommand
        {
            Version = version,
            Nota = nota,
            Estado = estado,
            Archivo = ToArchivo(file, stream)
        });

        return Ok(new ApiResponse<DocumentoDto> { Success = true, Data = data, Message = "Nueva versión registrada" });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "documentos.editar")]
    public async Task<ActionResult<ApiResponse<DocumentoDto>>> Update(Guid id, [FromBody] ActualizarDocumentoDto dto)
    {
        var data = await _documentoService.ActualizarAsync(id, dto);
        return Ok(new ApiResponse<DocumentoDto> { Success = true, Data = data, Message = "Documento actualizado" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "documentos.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _documentoService.EliminarAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Documento eliminado" });
    }

    /// <summary>
    /// Descarga con el nombre original del archivo. Se sirve desde la API (y no con la URL SAS)
    /// para conservar el nombre y aplicar el control de acceso por proyecto en cada descarga.
    /// </summary>
    [HttpGet("{id}/descargar")]
    [Authorize(Policy = "documentos.ver")]
    public async Task<IActionResult> Download(Guid id, [FromQuery] Guid? versionId)
    {
        var descarga = await _documentoService.DescargarAsync(id, versionId);
        return File(descarga.Contenido, descarga.ContentType, descarga.NombreArchivo);
    }

    [HttpPost("carpetas")]
    [Authorize(Policy = "documentos.editar")]
    public async Task<ActionResult<ApiResponse<CarpetaDocumentoDto>>> CreateFolder([FromBody] CrearCarpetaDto dto)
    {
        var data = await _documentoService.CrearCarpetaAsync(dto);
        return Ok(new ApiResponse<CarpetaDocumentoDto> { Success = true, Data = data, Message = "Carpeta creada" });
    }

    [HttpPut("carpetas/{id}")]
    [Authorize(Policy = "documentos.editar")]
    public async Task<ActionResult<ApiResponse<CarpetaDocumentoDto>>> RenameFolder(Guid id, [FromBody] RenombrarCarpetaDto dto)
    {
        var data = await _documentoService.RenombrarCarpetaAsync(id, dto);
        return Ok(new ApiResponse<CarpetaDocumentoDto> { Success = true, Data = data, Message = "Carpeta actualizada" });
    }

    [HttpDelete("carpetas/{id}")]
    [Authorize(Policy = "documentos.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteFolder(Guid id)
    {
        await _documentoService.EliminarCarpetaAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Carpeta eliminada" });
    }

    private static ArchivoEntrante ToArchivo(IFormFile file, Stream stream) => new(
        stream,
        file.FileName,
        string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
        file.Length);
}
