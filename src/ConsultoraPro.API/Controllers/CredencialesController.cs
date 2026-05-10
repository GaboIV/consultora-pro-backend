using System.Security.Claims;
using ConsultoraPro.Application.DTOs.Credenciales;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CredencialesController : ControllerBase
{
    private readonly ICredencialService _credencialService;

    public CredencialesController(ICredencialService credencialService)
    {
        _credencialService = credencialService;
    }

    [HttpGet]
    [Authorize(Policy = "credenciales.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CredencialListDto>>>> GetAll([FromQuery] Guid? proyectoId)
    {
        var data = await _credencialService.GetAllAsync(proyectoId);
        return Ok(new ApiResponse<IEnumerable<CredencialListDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "credenciales.ver")]
    public async Task<ActionResult<ApiResponse<CredencialDetalleDto>>> GetById(Guid id)
    {
        var data = await _credencialService.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<CredencialDetalleDto> { Success = false, Message = $"Credencial con ID {id} no encontrada" });

        return Ok(new ApiResponse<CredencialDetalleDto> { Success = true, Data = data });
    }

    [HttpGet("{id}/revelar")]
    [Authorize(Policy = "credenciales.revelar")]
    [EndpointDescription("Devuelve temporalmente el valor descifrado de la credencial y registra una auditoría automática de acceso.")]
    public async Task<ActionResult<ApiResponse<CredencialRevealDto>>> Reveal(Guid id)
    {
        var data = await _credencialService.RevealAsync(
            id,
            GetCurrentUserId(),
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Request.Headers.UserAgent.ToString());

        return Ok(new ApiResponse<CredencialRevealDto>
        {
            Success = true,
            Data = data,
            Message = "Credencial revelada y auditada"
        });
    }

    [HttpPost]
    [Authorize(Policy = "credenciales.crear")]
    public async Task<ActionResult<ApiResponse<CredencialListDto>>> Create([FromBody] CreateCredencialDto dto)
    {
        var data = await _credencialService.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<CredencialListDto>
        {
            Success = true,
            Data = data,
            Message = "Credencial creada exitosamente"
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "credenciales.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateCredencialDto dto)
    {
        await _credencialService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Credencial actualizada exitosamente" });
    }

    [HttpPut("{id}/valor")]
    [Authorize(Policy = "credenciales.editar")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateValor(Guid id, [FromBody] UpdateCredencialValorDto dto)
    {
        await _credencialService.UpdateValorAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Valor de credencial actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "credenciales.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _credencialService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Credencial desactivada exitosamente" });
    }

    [HttpGet("{id}/auditoria")]
    [Authorize(Policy = "credenciales.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AuditoriaCredencialDto>>>> GetAudit(Guid id)
    {
        var data = await _credencialService.GetAuditAsync(id);
        return Ok(new ApiResponse<IEnumerable<AuditoriaCredencialDto>>
        {
            Success = true,
            Data = data
        });
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var id))
            throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado");

        return id;
    }
}
