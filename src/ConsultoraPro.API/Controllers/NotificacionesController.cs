using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Notificaciones;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

/// <summary>
/// Bandeja de notificaciones y preferencias del usuario autenticado.
/// No requiere permisos especiales: cada usuario solo ve y administra lo suyo.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificacionService;
    private readonly ICurrentUserService _currentUser;

    public NotificacionesController(INotificacionService notificacionService, ICurrentUserService currentUser)
    {
        _notificacionService = notificacionService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResultDto<NotificacionDto>>>> GetMisNotificaciones(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        [FromQuery] bool soloNoLeidas = false)
    {
        var data = await _notificacionService.GetMisNotificacionesAsync(GetUserId(), page, pageSize, soloNoLeidas);
        return Ok(new ApiResponse<PagedResultDto<NotificacionDto>> { Success = true, Data = data });
    }

    /// <summary>Endpoint liviano para el polling de la campana (solo el contador).</summary>
    [HttpGet("resumen")]
    public async Task<ActionResult<ApiResponse<ResumenNotificacionesDto>>> GetResumen()
    {
        var data = await _notificacionService.GetResumenAsync(GetUserId());
        return Ok(new ApiResponse<ResumenNotificacionesDto> { Success = true, Data = data });
    }

    [HttpPut("{id:guid}/leer")]
    public async Task<ActionResult<ApiResponse<object>>> MarcarLeida(Guid id)
    {
        var encontrada = await _notificacionService.MarcarLeidaAsync(GetUserId(), id);
        if (!encontrada)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Notificación no encontrada" });
        return Ok(new ApiResponse<object> { Success = true });
    }

    [HttpPut("leer-todas")]
    public async Task<ActionResult<ApiResponse<object>>> MarcarTodasLeidas()
    {
        await _notificacionService.MarcarTodasLeidasAsync(GetUserId());
        return Ok(new ApiResponse<object> { Success = true });
    }

    [HttpGet("preferencias")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PreferenciaNotificacionDto>>>> GetPreferencias()
    {
        var data = await _notificacionService.GetPreferenciasAsync(GetUserId());
        return Ok(new ApiResponse<IReadOnlyList<PreferenciaNotificacionDto>> { Success = true, Data = data });
    }

    [HttpPut("preferencias")]
    public async Task<ActionResult<ApiResponse<object>>> UpdatePreferencias(
        [FromBody] UpdatePreferenciasNotificacionDto dto)
    {
        await _notificacionService.ActualizarPreferenciasAsync(GetUserId(), dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Preferencias actualizadas exitosamente" });
    }

    private Guid GetUserId()
        => _currentUser.UserId ?? throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado");
}
