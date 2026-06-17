using System.IO;
using System.Security.Claims;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Kanban;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TarjetasController : ControllerBase
{
    private const long MaxAdjuntoBytes = 10 * 1024 * 1024; // 10 MB
    private const long MaxImagenBytes = 5 * 1024 * 1024;   // 5 MB

    private readonly ITarjetaService _tarjetaService;
    private readonly IStorageService _storageService;

    public TarjetasController(ITarjetaService tarjetaService, IStorageService storageService)
    {
        _tarjetaService = tarjetaService;
        _storageService = storageService;
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

    // ---- Checklist ----

    [HttpPost("{id}/checklist")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<ChecklistItemDto>>> AddChecklist(Guid id, [FromBody] CreateChecklistItemDto dto)
    {
        var data = await _tarjetaService.AddChecklistItemAsync(id, dto);
        return Ok(new ApiResponse<ChecklistItemDto> { Success = true, Data = data });
    }

    [HttpPut("{id}/checklist/{itemId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<ChecklistItemDto>>> UpdateChecklist(Guid id, Guid itemId, [FromBody] UpdateChecklistItemDto dto)
    {
        var data = await _tarjetaService.UpdateChecklistItemAsync(id, itemId, dto);
        return Ok(new ApiResponse<ChecklistItemDto> { Success = true, Data = data });
    }

    [HttpDelete("{id}/checklist/{itemId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteChecklist(Guid id, Guid itemId)
    {
        await _tarjetaService.DeleteChecklistItemAsync(id, itemId);
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

        if (file.Length > MaxAdjuntoBytes)
            return BadRequest(new ApiResponse<AdjuntoDto> { Success = false, Message = "El tamaño máximo permitido es de 10MB" });

        string url;
        using (var stream = file.OpenReadStream())
        {
            url = await _storageService.SaveFileAsync(stream, file.FileName, file.ContentType);
        }

        var data = await _tarjetaService.AddAdjuntoAsync(
            id,
            Path.GetFileName(file.FileName),
            url,
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

        if (file.Length > MaxImagenBytes)
            return BadRequest(new ApiResponse<ImagenInlineDto> { Success = false, Message = "El tamaño máximo permitido es 5 MB" });

        string url;
        using (var stream = file.OpenReadStream())
        {
            url = await _storageService.SaveFileAsync(stream, file.FileName, file.ContentType);
        }

        return Ok(new ApiResponse<ImagenInlineDto> { Success = true, Data = new ImagenInlineDto { Url = url } });
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
