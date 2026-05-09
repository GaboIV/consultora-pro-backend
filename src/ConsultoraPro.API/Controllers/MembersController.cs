using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Members;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    [Authorize(Policy = "equipo.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MemberDto>>>> GetAll()
    {
        var data = await _memberService.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<MemberDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "equipo.ver")]
    public async Task<ActionResult<ApiResponse<MemberDto>>> GetById(Guid id)
    {
        var data = await _memberService.GetByIdAsync(id);
        if (data == null)
            return NotFound(new ApiResponse<MemberDto> { Success = false, Message = $"Miembro con ID {id} no encontrado" });
        return Ok(new ApiResponse<MemberDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "equipo.crear")]
    public async Task<ActionResult<ApiResponse<MemberDto>>> Create([FromBody] CreateMemberDto dto)
    {
        var data = await _memberService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<MemberDto> { Success = true, Data = data, Message = "Miembro creado exitosamente" });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "equipo.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateMemberDto dto)
    {
        await _memberService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Miembro actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "equipo.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _memberService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Miembro eliminado exitosamente" });
    }
}
