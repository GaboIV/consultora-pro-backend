using System.Security.Claims;
using ConsultoraPro.Application.DTOs.Despliegues;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DesplieguesController : ControllerBase
{
    private readonly IDespliegueService _despliegueService;

    public DesplieguesController(IDespliegueService despliegueService)
    {
        _despliegueService = despliegueService;
    }

    [HttpGet]
    [Authorize(Policy = "despliegues.historial")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<DespliegueListDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var data = await _despliegueService.GetAllAsync(page, pageSize);
        return Ok(new ApiResponse<PagedResultDto<DespliegueListDto>> { Success = true, Data = data });
    }

    [HttpGet("proyecto/{proyectoId}")]
    [Authorize(Policy = "despliegues.historial")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<DespliegueListDto>>>> GetByProject(
        Guid proyectoId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var data = await _despliegueService.GetByProjectAsync(proyectoId, page, pageSize);
        return Ok(new ApiResponse<PagedResultDto<DespliegueListDto>> { Success = true, Data = data });
    }

    [HttpGet("recientes")]
    [Authorize(Policy = "despliegues.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DespliegueListDto>>>> GetRecent()
    {
        var data = await _despliegueService.GetRecentAsync(10);
        return Ok(new ApiResponse<IEnumerable<DespliegueListDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "despliegues.ver")]
    public async Task<ActionResult<ApiResponse<DespliegueDto>>> GetById(Guid id)
    {
        var data = await _despliegueService.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<DespliegueDto> { Success = false, Message = $"Despliegue con ID {id} no encontrado" });

        return Ok(new ApiResponse<DespliegueDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "despliegues.ejecutar")]
    public async Task<ActionResult<ApiResponse<DespliegueDto>>> Create([FromBody] CreateDespliegueDto dto)
    {
        var userIdValue = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var ejecutadoPorId))
            return Unauthorized(new ApiResponse<DespliegueDto> { Success = false, Message = "No se pudo identificar al usuario" });

        try
        {
            var data = await _despliegueService.CreateAsync(dto, ejecutadoPorId);
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<DespliegueDto> { Success = true, Data = data });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<DespliegueDto> { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id}/estado")]
    [Authorize(Policy = "despliegues.ejecutar")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEstado(Guid id, [FromBody] UpdateDespliegueEstadoDto dto)
    {
        try
        {
            await _despliegueService.UpdateEstadoAsync(id, dto);
            return Ok(new ApiResponse<object> { Success = true, Message = "Estado de despliegue actualizado." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
