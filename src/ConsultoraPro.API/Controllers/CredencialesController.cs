using System.Security.Claims;
using ConsultoraPro.Application.DTOs.Credenciales;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Exceptions;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CredencialesController : ControllerBase
{
    private readonly ICredencialService _credencialService;
    private readonly ICurrentUserService _currentUserService;

    public CredencialesController(
        ICredencialService credencialService,
        ICurrentUserService currentUserService)
    {
        _credencialService = credencialService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Policy = "credenciales.ver")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<CredencialListDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? proyectoId = null)
    {
        var data = await _credencialService.GetAllAsync(page, pageSize, proyectoId);
        return Ok(new ApiResponse<PagedResultDto<CredencialListDto>> { Success = true, Data = data });
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

    // Política relajada a "credenciales.ver": un usuario de nivel básico puede ALCANZAR el endpoint,
    // pero la revelación real se decide en servidor (permiso directo de revelar o aprobación vigente).
    [HttpGet("{id}/revelar")]
    [Authorize(Policy = "credenciales.ver")]
    [EndpointDescription("Devuelve temporalmente el valor descifrado de la credencial y registra una auditoría automática de acceso.")]
    public async Task<ActionResult<ApiResponse<CredencialRevealDto>>> Reveal(Guid id)
    {
        try
        {
            var data = await _credencialService.RevealAsync(
                id,
                GetCurrentUserId(),
                _currentUserService.HasPermission("credenciales.revelar"),
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                Request.Headers.UserAgent.ToString());

            return Ok(new ApiResponse<CredencialRevealDto>
            {
                Success = true,
                Data = data,
                Message = "Credencial revelada y auditada"
            });
        }
        catch (RevelacionRequiereSolicitudException ex)
        {
            // 409 (no 403) para evitar el redirect global del frontend ante 403; el código permite
            // al cliente ofrecer la creación de una solicitud de revelación.
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message,
                Errors = [RevelacionRequiereSolicitudException.Code]
            });
        }
    }

    [HttpPost("{id}/copiado")]
    [Authorize(Policy = "credenciales.ver")]
    [EndpointDescription("Registra en la auditoría que el usuario copió un dato de la credencial al portapapeles.")]
    public async Task<ActionResult<ApiResponse<object>>> RegistrarCopiado(Guid id, [FromBody] RegistrarCopiadoDto dto)
    {
        await _credencialService.RegistrarCopiadoAsync(
            id,
            GetCurrentUserId(),
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Request.Headers.UserAgent.ToString(),
            dto.Campo);

        return Ok(new ApiResponse<object> { Success = true, Message = "Copiado auditado" });
    }

    [HttpPost("importar")]
    [Authorize(Policy = "credenciales.crear")]
    [EndpointDescription("Importa un lote de credenciales validando fila por fila; devuelve los errores por fila sin abortar el resto.")]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> Importar([FromBody] ImportCredencialesDto dto)
    {
        var data = await _credencialService.ImportAsync(dto, GetCurrentUserId());
        return Ok(new ApiResponse<ImportResultDto>
        {
            Success = true,
            Data = data,
            Message = $"{data.Importadas} de {data.Total} credenciales importadas"
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

    // ---------------------------------------------------------------------------------------------
    // Flujo de solicitud de revelación (nivel básico solicita; nivel full/aprobador resuelve).
    // ---------------------------------------------------------------------------------------------

    [HttpPost("{id}/solicitudes")]
    [Authorize(Policy = "credenciales.ver")]
    [EndpointDescription("Crea una solicitud para revelar los secretos de una credencial (nivel básico).")]
    public async Task<ActionResult<ApiResponse<SolicitudRevelacionDto>>> CrearSolicitud(Guid id, [FromBody] CrearSolicitudRevelacionDto dto)
    {
        var data = await _credencialService.CrearSolicitudAsync(id, GetCurrentUserId(), dto.Motivo);
        return Ok(new ApiResponse<SolicitudRevelacionDto>
        {
            Success = true,
            Data = data,
            Message = "Solicitud de revelación registrada"
        });
    }

    [HttpGet("mis-solicitudes")]
    [Authorize(Policy = "credenciales.ver")]
    [EndpointDescription("Lista las solicitudes de revelación creadas por el usuario actual.")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SolicitudRevelacionDto>>>> GetMisSolicitudes()
    {
        var data = await _credencialService.GetMisSolicitudesAsync(GetCurrentUserId());
        return Ok(new ApiResponse<IEnumerable<SolicitudRevelacionDto>> { Success = true, Data = data });
    }

    [HttpGet("solicitudes")]
    [Authorize(Policy = "credenciales.solicitud.aprobar")]
    [EndpointDescription("Bandeja del aprobador: lista solicitudes de revelación, filtrables por estado.")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SolicitudRevelacionDto>>>> GetSolicitudes(
        [FromQuery] EstadoSolicitudRevelacion? estado = null)
    {
        var data = await _credencialService.GetSolicitudesAsync(estado);
        return Ok(new ApiResponse<IEnumerable<SolicitudRevelacionDto>> { Success = true, Data = data });
    }

    [HttpPost("solicitudes/{solicitudId}/aprobar")]
    [Authorize(Policy = "credenciales.solicitud.aprobar")]
    public async Task<ActionResult<ApiResponse<SolicitudRevelacionDto>>> AprobarSolicitud(Guid solicitudId, [FromBody] ResolverSolicitudRevelacionDto? dto)
    {
        var data = await _credencialService.ResolverSolicitudAsync(solicitudId, GetCurrentUserId(), aprobar: true, dto?.Nota);
        return Ok(new ApiResponse<SolicitudRevelacionDto> { Success = true, Data = data, Message = "Solicitud aprobada" });
    }

    [HttpPost("solicitudes/{solicitudId}/rechazar")]
    [Authorize(Policy = "credenciales.solicitud.aprobar")]
    public async Task<ActionResult<ApiResponse<SolicitudRevelacionDto>>> RechazarSolicitud(Guid solicitudId, [FromBody] ResolverSolicitudRevelacionDto? dto)
    {
        var data = await _credencialService.ResolverSolicitudAsync(solicitudId, GetCurrentUserId(), aprobar: false, dto?.Nota);
        return Ok(new ApiResponse<SolicitudRevelacionDto> { Success = true, Data = data, Message = "Solicitud rechazada" });
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var id))
            throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado");

        return id;
    }
}
