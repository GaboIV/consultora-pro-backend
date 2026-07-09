using System.IO;
using System.Security.Claims;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Kanban;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TarjetasController : ControllerBase
{
    private readonly ITarjetaService _tarjetaService;
    private readonly ITarjetaExportService _exportService;
    private readonly IStorageService _storageService;
    private readonly IFileUrlResolver _urlResolver;
    private readonly StorageOptions _storage;

    public TarjetasController(
        ITarjetaService tarjetaService,
        ITarjetaExportService exportService,
        IStorageService storageService,
        IFileUrlResolver urlResolver,
        IOptions<StorageOptions> storageOptions)
    {
        _tarjetaService = tarjetaService;
        _exportService = exportService;
        _storageService = storageService;
        _urlResolver = urlResolver;
        _storage = storageOptions.Value;
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "kanban.ver")]
    public async Task<ActionResult<ApiResponse<TarjetaDetalleDto>>> GetById(Guid id)
    {
        var data = await _tarjetaService.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<TarjetaDetalleDto> { Success = false, Message = $"Tarjeta con ID {id} no encontrada" });

        return Ok(new ApiResponse<TarjetaDetalleDto> { Success = true, Data = data });
    }

    /// <summary>
    /// Descarga un ZIP con todo el contexto de la tarjeta (descripción, checklist, datos generales,
    /// actividad, comentarios, imágenes inline y adjuntos).
    /// </summary>
    [HttpGet("{id}/export")]
    [Authorize(Policy = "kanban.ver")]
    public async Task<IActionResult> Export(Guid id)
    {
        var result = await _exportService.ExportarAsync(id);
        if (result is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = $"Tarjeta con ID {id} no encontrada" });

        return File(result.Contenido, "application/zip", result.NombreArchivo);
    }

    [HttpPost]
    [Authorize(Policy = "kanban.crear")]
    public async Task<ActionResult<ApiResponse<TarjetaDetalleDto>>> Create([FromBody] CreateTarjetaDto dto)
    {
        var data = await _tarjetaService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<TarjetaDetalleDto> { Success = true, Data = data });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateTarjetaDto dto)
    {
        await _tarjetaService.UpdateAsync(id, dto, GetUserId());
        return Ok(new ApiResponse<object> { Success = true, Message = "Tarjeta actualizada." });
    }

    [HttpPut("{id}/mover")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Mover(Guid id, [FromBody] MoverTarjetaDto dto)
    {
        await _tarjetaService.MoverAsync(id, dto, GetUserId());
        return Ok(new ApiResponse<object> { Success = true, Message = "Tarjeta movida." });
    }

    [HttpPut("{id}/responsables")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ResponsableDto>>>> Responsables(Guid id, [FromBody] AsignarResponsablesDto dto)
    {
        var data = await _tarjetaService.AsignarResponsablesAsync(id, dto, GetUserId());
        return Ok(new ApiResponse<IEnumerable<ResponsableDto>> { Success = true, Data = data });
    }

    [HttpPut("{id}/etiquetas")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EtiquetaDto>>>> Etiquetas(Guid id, [FromBody] AsignarEtiquetasDto dto)
    {
        var data = await _tarjetaService.AsignarEtiquetasAsync(id, dto, GetUserId());
        return Ok(new ApiResponse<IEnumerable<EtiquetaDto>> { Success = true, Data = data });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "kanban.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _tarjetaService.DeleteAsync(id, GetUserId());
        return Ok(new ApiResponse<object> { Success = true, Message = "Tarjeta archivada." });
    }

    // ---- Checklists ----

    [HttpPost("{id}/checklists")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<ChecklistDto>>> AddChecklist(Guid id, [FromBody] CreateChecklistDto dto)
    {
        var data = await _tarjetaService.AddChecklistAsync(id, dto);
        return Ok(new ApiResponse<ChecklistDto> { Success = true, Data = data });
    }

    [HttpPut("{id}/checklists/{checklistId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<ChecklistDto>>> UpdateChecklist(Guid id, Guid checklistId, [FromBody] UpdateChecklistDto dto)
    {
        var data = await _tarjetaService.UpdateChecklistAsync(id, checklistId, dto);
        return Ok(new ApiResponse<ChecklistDto> { Success = true, Data = data });
    }

    [HttpDelete("{id}/checklists/{checklistId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteChecklist(Guid id, Guid checklistId)
    {
        await _tarjetaService.DeleteChecklistAsync(id, checklistId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Checklist eliminado." });
    }

    // ---- Ítems de checklist ----

    [HttpPost("{id}/checklists/{checklistId}/items")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<ChecklistItemDto>>> AddChecklistItem(Guid id, Guid checklistId, [FromBody] CreateChecklistItemDto dto)
    {
        var data = await _tarjetaService.AddChecklistItemAsync(id, checklistId, dto);
        return Ok(new ApiResponse<ChecklistItemDto> { Success = true, Data = data });
    }

    [HttpPut("{id}/checklists/{checklistId}/items/{itemId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<ChecklistItemDto>>> UpdateChecklistItem(Guid id, Guid checklistId, Guid itemId, [FromBody] UpdateChecklistItemDto dto)
    {
        var data = await _tarjetaService.UpdateChecklistItemAsync(id, checklistId, itemId, dto);
        return Ok(new ApiResponse<ChecklistItemDto> { Success = true, Data = data });
    }

    [HttpDelete("{id}/checklists/{checklistId}/items/{itemId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteChecklistItem(Guid id, Guid checklistId, Guid itemId)
    {
        await _tarjetaService.DeleteChecklistItemAsync(id, checklistId, itemId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Ítem eliminado." });
    }

    // ---- Comentarios ----

    [HttpPost("{id}/comentarios")]
    [Authorize(Policy = "kanban.comentar")]
    public async Task<ActionResult<ApiResponse<ComentarioDto>>> AddComentario(Guid id, [FromBody] CreateComentarioDto dto)
    {
        var data = await _tarjetaService.AddComentarioAsync(id, dto, GetUserId());
        return Ok(new ApiResponse<ComentarioDto> { Success = true, Data = data });
    }

    [HttpPut("{id}/comentarios/{comentarioId}")]
    [Authorize(Policy = "kanban.comentar")]
    public async Task<ActionResult<ApiResponse<ComentarioDto>>> UpdateComentario(Guid id, Guid comentarioId, [FromBody] UpdateComentarioDto dto)
    {
        var data = await _tarjetaService.UpdateComentarioAsync(id, comentarioId, dto, GetUserId());
        return Ok(new ApiResponse<ComentarioDto> { Success = true, Data = data });
    }

    [HttpDelete("{id}/comentarios/{comentarioId}")]
    [Authorize(Policy = "kanban.comentar")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteComentario(Guid id, Guid comentarioId)
    {
        await _tarjetaService.DeleteComentarioAsync(id, comentarioId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Comentario eliminado." });
    }

    // ---- Adjuntos ----

    [HttpPost("{id}/adjuntos")]
    [Authorize(Policy = "kanban.editar")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<AdjuntoDto>>> AddAdjunto(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ApiResponse<AdjuntoDto> { Success = false, Message = "No se proporcionó ningún archivo" });

        if (file.Length > _storage.Limits.MaxAttachmentBytes)
            return BadRequest(new ApiResponse<AdjuntoDto> { Success = false, Message = $"El tamaño máximo permitido es de {_storage.Limits.MaxAttachmentBytes / (1024 * 1024)}MB" });

        StoredFile stored;
        using (var stream = file.OpenReadStream())
        {
            stored = await _storageService.SaveFileAsync(stream, file.FileName, file.ContentType, "adjuntos");
        }

        var data = await _tarjetaService.AddAdjuntoAsync(
            id,
            Path.GetFileName(file.FileName),
            stored.Key,
            file.ContentType,
            file.Length,
            GetUserId());

        return Ok(new ApiResponse<AdjuntoDto> { Success = true, Data = data });
    }

    [HttpPost("{id}/imagenes")]
    [Authorize(Policy = "kanban.editar")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ImagenInlineDto>>> AddImagenInline(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ApiResponse<ImagenInlineDto> { Success = false, Message = "No se proporcionó ningún archivo" });

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new ApiResponse<ImagenInlineDto> { Success = false, Message = "Solo se permiten archivos de imagen" });

        if (file.Length > _storage.Limits.MaxImageBytes)
            return BadRequest(new ApiResponse<ImagenInlineDto> { Success = false, Message = $"El tamaño máximo permitido es {_storage.Limits.MaxImageBytes / (1024 * 1024)} MB" });

        StoredFile stored;
        using (var stream = file.OpenReadStream())
        {
            stored = await _storageService.SaveFileAsync(stream, file.FileName, file.ContentType, "inline");
        }

        // El editor persiste el placeholder estable (Key) en la descripción; Url es para previsualizar ya.
        var placeholder = $"{FileUrlResolver.PlaceholderScheme}{stored.Key}";
        var previewUrl = await _storageService.GetAccessUrlAsync(stored.Key);
        return Ok(new ApiResponse<ImagenInlineDto> { Success = true, Data = new ImagenInlineDto { Key = placeholder, Url = previewUrl } });
    }

    [HttpDelete("{id}/adjuntos/{adjuntoId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAdjunto(Guid id, Guid adjuntoId)
    {
        await _tarjetaService.DeleteAdjuntoAsync(id, adjuntoId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Adjunto eliminado." });
    }

    // ---- Actividad ----

    [HttpGet("{id}/actividad")]
    [Authorize(Policy = "kanban.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ActividadDto>>>> Actividad(Guid id)
    {
        var data = await _tarjetaService.GetActividadAsync(id);
        return Ok(new ApiResponse<IEnumerable<ActividadDto>> { Success = true, Data = data });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("No se pudo identificar al usuario");
        return userId;
    }
}
