using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.TiposSolucion;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/tipos-solucion")]
public class TiposSolucionController : ControllerBase
{
    private readonly ITipoSolucionService _tipoSolucionService;

    public TiposSolucionController(ITipoSolucionService tipoSolucionService)
    {
        _tipoSolucionService = tipoSolucionService;
    }

    [HttpGet]
    [Authorize(Policy = "tipos-solucion.ver")]
    public async Task<ActionResult<ApiResponse<List<TipoSolucionDto>>>> GetAll()
    {
        var data = await _tipoSolucionService.GetAllAsync();
        return Ok(new ApiResponse<List<TipoSolucionDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "tipos-solucion.ver")]
    public async Task<ActionResult<ApiResponse<TipoSolucionDto>>> GetById(Guid id)
    {
        var data = await _tipoSolucionService.GetByIdAsync(id);
        if (data == null)
            return NotFound(new ApiResponse<TipoSolucionDto> { Success = false, Message = $"Tipo de solución con ID {id} no encontrado" });
        return Ok(new ApiResponse<TipoSolucionDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "tipos-solucion.crear")]
    public async Task<ActionResult<ApiResponse<TipoSolucionDto>>> Create([FromBody] CreateTipoSolucionDto dto)
    {
        var data = await _tipoSolucionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<TipoSolucionDto> { Success = true, Data = data, Message = "Tipo de solución creado exitosamente" });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "tipos-solucion.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateTipoSolucionDto dto)
    {
        await _tipoSolucionService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Tipo de solución actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "tipos-solucion.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _tipoSolucionService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Tipo de solución eliminado exitosamente" });
    }
}
