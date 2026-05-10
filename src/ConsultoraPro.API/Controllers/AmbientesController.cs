using ConsultoraPro.Application.DTOs.Ambientes;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AmbientesController : ControllerBase
{
    private readonly IAmbienteService _ambienteService;

    public AmbientesController(IAmbienteService ambienteService)
    {
        _ambienteService = ambienteService;
    }

    [HttpGet]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AmbienteDto>>>> GetAll([FromQuery] Guid? proyectoId)
    {
        var data = await _ambienteService.GetAllAsync(proyectoId);
        return Ok(new ApiResponse<IEnumerable<AmbienteDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<AmbienteDto>>> GetById(Guid id)
    {
        var data = await _ambienteService.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<AmbienteDto> { Success = false, Message = $"Ambiente con ID {id} no encontrado" });

        return Ok(new ApiResponse<AmbienteDto> { Success = true, Data = data });
    }

    [HttpGet("proyecto/{proyectoId}")]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AmbienteDto>>>> GetByProject(Guid proyectoId)
    {
        var data = await _ambienteService.GetAllAsync(proyectoId);
        return Ok(new ApiResponse<IEnumerable<AmbienteDto>> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "ambientes.crear")]
    public async Task<ActionResult<ApiResponse<AmbienteDto>>> Create([FromBody] CreateAmbienteDto dto)
    {
        var data = await _ambienteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<AmbienteDto>
        {
            Success = true,
            Data = data,
            Message = "Ambiente creado exitosamente"
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateAmbienteDto dto)
    {
        await _ambienteService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Ambiente actualizado exitosamente" });
    }

    [HttpPut("{id}/estado")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEstado(Guid id, [FromBody] UpdateAmbienteEstadoDto dto)
    {
        await _ambienteService.UpdateEstadoAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Estado de ambiente actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _ambienteService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Ambiente desactivado exitosamente" });
    }
}
