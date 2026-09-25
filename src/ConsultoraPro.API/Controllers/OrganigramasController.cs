using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Organigramas;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/organigramas")]
public class OrganigramasController : ControllerBase
{
    private readonly IOrganigramaService _organigramaService;

    public OrganigramasController(IOrganigramaService organigramaService)
    {
        _organigramaService = organigramaService;
    }

    [HttpGet]
    [Authorize(Policy = "organigramas.ver")]
    public async Task<ActionResult<ApiResponse<List<OrganigramaResumenDto>>>> GetAll()
    {
        var data = await _organigramaService.GetAllAsync();
        return Ok(new ApiResponse<List<OrganigramaResumenDto>> { Success = true, Data = data });
    }

    /// <summary>Usuarios que se pueden vincular a una posición (sin requerir el permiso usuarios.ver).</summary>
    [HttpGet("usuarios-disponibles")]
    [Authorize(Policy = "organigramas.editar")]
    public async Task<ActionResult<ApiResponse<List<OrganigramaUsuarioDto>>>> GetUsuariosDisponibles()
    {
        var data = await _organigramaService.GetUsuariosDisponiblesAsync();
        return Ok(new ApiResponse<List<OrganigramaUsuarioDto>> { Success = true, Data = data });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "organigramas.ver")]
    public async Task<ActionResult<ApiResponse<OrganigramaDto>>> GetById(Guid id)
    {
        var data = await _organigramaService.GetByIdAsync(id);
        if (data == null)
            return NotFound(new ApiResponse<OrganigramaDto> { Success = false, Message = $"Organigrama con ID {id} no encontrado" });
        return Ok(new ApiResponse<OrganigramaDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "organigramas.editar")]
    public async Task<ActionResult<ApiResponse<OrganigramaDto>>> Create([FromBody] GuardarOrganigramaDto dto)
    {
        var data = await _organigramaService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<OrganigramaDto> { Success = true, Data = data, Message = "Organigrama creado exitosamente" });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "organigramas.editar")]
    public async Task<ActionResult<ApiResponse<OrganigramaDto>>> Update(Guid id, [FromBody] GuardarOrganigramaDto dto)
    {
        var data = await _organigramaService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<OrganigramaDto> { Success = true, Data = data, Message = "Organigrama guardado exitosamente" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "organigramas.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _organigramaService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Organigrama eliminado exitosamente" });
    }
}
