using ConsultoraPro.Application.DTOs.AmbienteTestUsers;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/ambientes/{ambienteId}/test-users")]
public class AmbienteTestUsersController : ControllerBase
{
    private readonly IAmbienteTestUserService _service;

    public AmbienteTestUsersController(IAmbienteTestUserService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AmbienteTestUserDto>>>> GetAll(Guid ambienteId)
    {
        var data = await _service.GetByAmbienteAsync(ambienteId);
        return Ok(new ApiResponse<IEnumerable<AmbienteTestUserDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<AmbienteTestUserDto>>> GetById(Guid ambienteId, Guid id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<AmbienteTestUserDto> { Success = false, Message = $"Usuario de prueba con ID {id} no encontrado" });

        return Ok(new ApiResponse<AmbienteTestUserDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<AmbienteTestUserDto>>> Create(Guid ambienteId, [FromBody] CreateAmbienteTestUserDto dto)
    {
        dto.AmbienteId = ambienteId;
        var data = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { ambienteId, id = data.Id }, new ApiResponse<AmbienteTestUserDto>
        {
            Success = true,
            Data = data,
            Message = "Usuario de prueba creado exitosamente"
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid ambienteId, Guid id, [FromBody] UpdateAmbienteTestUserDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario de prueba actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid ambienteId, Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario de prueba desactivado exitosamente" });
    }
}
