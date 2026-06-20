using System.Security.Claims;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Kanban;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TablerosController : ControllerBase
{
    private readonly ITableroService _tableroService;

    public TablerosController(ITableroService tableroService)
    {
        _tableroService = tableroService;
    }

    [HttpGet("proyecto/{proyectoId}")]
    [Authorize(Policy = "kanban.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TableroDto>>>> GetByProyecto(Guid proyectoId)
    {
        var data = await _tableroService.GetByProyectoAsync(proyectoId);
        return Ok(new ApiResponse<IEnumerable<TableroDto>> { Success = true, Data = data });
    }

    [HttpGet("mis-tableros")]
    [Authorize(Policy = "kanban.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TableroDto>>>> GetMisTableros()
    {
        var data = await _tableroService.GetByUsuarioAsync(GetUserId());
        return Ok(new ApiResponse<IEnumerable<TableroDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "kanban.ver")]
    public async Task<ActionResult<ApiResponse<TableroDetalleDto>>> GetById(Guid id)
    {
        var data = await _tableroService.GetDetalleAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<TableroDetalleDto> { Success = false, Message = $"Tablero con ID {id} no encontrado" });

        return Ok(new ApiResponse<TableroDetalleDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "kanban.crear")]
    public async Task<ActionResult<ApiResponse<TableroDto>>> Create([FromBody] CreateTableroDto dto)
    {
        var data = await _tableroService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<TableroDto> { Success = true, Data = data });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("No se pudo identificar al usuario");
        return userId;
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateTableroDto dto)
    {
        await _tableroService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Tablero actualizado." });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "kanban.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _tableroService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Tablero archivado." });
    }

    [HttpPut("{id}/miembros")]
    [Authorize(Policy = "kanban.gestionar")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TableroMiembroDto>>>> UpdateMiembros(Guid id, [FromBody] UpdateMiembrosDto dto)
    {
        var data = await _tableroService.UpdateMiembrosAsync(id, dto);
        return Ok(new ApiResponse<IEnumerable<TableroMiembroDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}/etiquetas")]
    [Authorize(Policy = "kanban.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EtiquetaDto>>>> GetEtiquetas(Guid id)
    {
        var data = await _tableroService.GetEtiquetasAsync(id);
        return Ok(new ApiResponse<IEnumerable<EtiquetaDto>> { Success = true, Data = data });
    }

    [HttpPost("{id}/etiquetas")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<EtiquetaDto>>> CreateEtiqueta(Guid id, [FromBody] CreateEtiquetaDto dto)
    {
        var data = await _tableroService.CreateEtiquetaAsync(id, dto);
        return Ok(new ApiResponse<EtiquetaDto> { Success = true, Data = data });
    }

    [HttpPut("{id}/etiquetas/{etiquetaId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<EtiquetaDto>>> UpdateEtiqueta(Guid id, Guid etiquetaId, [FromBody] UpdateEtiquetaDto dto)
    {
        var data = await _tableroService.UpdateEtiquetaAsync(id, etiquetaId, dto);
        return Ok(new ApiResponse<EtiquetaDto> { Success = true, Data = data });
    }

    [HttpDelete("{id}/etiquetas/{etiquetaId}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEtiqueta(Guid id, Guid etiquetaId)
    {
        await _tableroService.DeleteEtiquetaAsync(id, etiquetaId);
        return Ok(new ApiResponse<object> { Success = true, Message = "Etiqueta eliminada." });
    }
}
