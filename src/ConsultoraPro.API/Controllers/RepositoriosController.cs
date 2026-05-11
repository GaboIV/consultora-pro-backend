using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Repositorios;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepositoriosController : ControllerBase
{
    private readonly IRepositorioService _repositorioService;

    public RepositoriosController(IRepositorioService repositorioService)
    {
        _repositorioService = repositorioService;
    }

    [HttpGet]
    [Authorize(Policy = "proyectos.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RepositorioDto>>>> GetAll([FromQuery] Guid? proyectoId)
    {
        var data = await _repositorioService.GetAllAsync(proyectoId);
        return Ok(new ApiResponse<IEnumerable<RepositorioDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "proyectos.ver")]
    public async Task<ActionResult<ApiResponse<RepositorioDto>>> GetById(Guid id)
    {
        var data = await _repositorioService.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<RepositorioDto> { Success = false, Message = $"Repositorio con ID {id} no encontrado" });

        return Ok(new ApiResponse<RepositorioDto> { Success = true, Data = data });
    }

    [HttpGet("proyecto/{proyectoId}")]
    [Authorize(Policy = "proyectos.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RepositorioDto>>>> GetByProject(Guid proyectoId)
    {
        var data = await _repositorioService.GetAllAsync(proyectoId);
        return Ok(new ApiResponse<IEnumerable<RepositorioDto>> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "proyectos.editar")]
    public async Task<ActionResult<ApiResponse<RepositorioDto>>> Create([FromBody] CreateRepositorioDto dto)
    {
        var data = await _repositorioService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<RepositorioDto>
        {
            Success = true,
            Data = data,
            Message = "Repositorio creado exitosamente"
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "proyectos.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateRepositorioDto dto)
    {
        await _repositorioService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Repositorio actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "proyectos.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _repositorioService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Repositorio eliminado exitosamente" });
    }
}
