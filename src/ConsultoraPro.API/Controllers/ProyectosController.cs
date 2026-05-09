using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Proyectos;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProyectosController : ControllerBase
{
    private readonly IProyectoService _proyectoService;

    public ProyectosController(IProyectoService proyectoService)
    {
        _proyectoService = proyectoService;
    }

    [HttpGet]
    [Authorize(Policy = "proyectos.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProyectoDto>>>> GetAll()
    {
        var data = await _proyectoService.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<ProyectoDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "proyectos.ver")]
    public async Task<ActionResult<ApiResponse<ProyectoDto>>> GetById(Guid id)
    {
        var data = await _proyectoService.GetByIdAsync(id);
        if (data == null)
            return NotFound(new ApiResponse<ProyectoDto> { Success = false, Message = $"Proyecto con ID {id} no encontrado" });
        return Ok(new ApiResponse<ProyectoDto> { Success = true, Data = data });
    }

    [HttpGet("cliente/{clienteId}")]
    [Authorize(Policy = "proyectos.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProyectoDto>>>> GetByClienteId(Guid clienteId)
    {
        var data = await _proyectoService.GetByClienteIdAsync(clienteId);
        return Ok(new ApiResponse<IEnumerable<ProyectoDto>> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "proyectos.crear")]
    public async Task<ActionResult<ApiResponse<ProyectoDto>>> Create([FromBody] CreateProyectoDto dto)
    {
        var data = await _proyectoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<ProyectoDto> { Success = true, Data = data, Message = "Proyecto creado exitosamente" });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "proyectos.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateProyectoDto dto)
    {
        await _proyectoService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Proyecto actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "proyectos.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _proyectoService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Proyecto eliminado exitosamente" });
    }
}
