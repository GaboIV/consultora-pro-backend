using ConsultoraPro.Application.DTOs.AmbienteComponentes;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/ambientes/{ambienteId}/componentes")]
public class AmbienteComponentesController : ControllerBase
{
    private readonly IAmbienteComponenteService _service;

    public AmbienteComponentesController(IAmbienteComponenteService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AmbienteComponenteDto>>>> GetAll(Guid ambienteId)
    {
        var data = await _service.GetByAmbienteAsync(ambienteId);
        return Ok(new ApiResponse<IEnumerable<AmbienteComponenteDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<AmbienteComponenteDto>>> GetById(Guid ambienteId, Guid id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<AmbienteComponenteDto> { Success = false, Message = $"Componente con ID {id} no encontrado" });

        return Ok(new ApiResponse<AmbienteComponenteDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<AmbienteComponenteDto>>> Create(Guid ambienteId, [FromBody] CreateAmbienteComponenteDto dto)
    {
        dto.AmbienteId = ambienteId;
        var data = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { ambienteId, id = data.Id }, new ApiResponse<AmbienteComponenteDto>
        {
            Success = true,
            Data = data,
            Message = "Componente creado exitosamente"
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid ambienteId, Guid id, [FromBody] UpdateAmbienteComponenteDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Componente actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid ambienteId, Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Componente desactivado exitosamente" });
    }
}
