using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Kanban;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ColumnasController : ControllerBase
{
    private readonly IColumnaService _columnaService;

    public ColumnasController(IColumnaService columnaService)
    {
        _columnaService = columnaService;
    }

    [HttpPost]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<ColumnaDto>>> Create([FromBody] CreateColumnaDto dto)
    {
        var data = await _columnaService.CreateAsync(dto);
        return Ok(new ApiResponse<ColumnaDto> { Success = true, Data = data });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateColumnaDto dto)
    {
        await _columnaService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Columna actualizada." });
    }

    [HttpPut("{id}/reordenar")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Reordenar(Guid id, [FromBody] ReordenarColumnaDto dto)
    {
        await _columnaService.ReordenarAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Columna reordenada." });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "kanban.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _columnaService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Columna eliminada." });
    }
}
