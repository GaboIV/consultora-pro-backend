using ConsultoraPro.Application.DTOs.Auth;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        var data = await _authService.RegisterAsync(dto);
        return Ok(new ApiResponse<AuthResponseDto> { Success = true, Data = data, Message = "Usuario registrado exitosamente" });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var data = await _authService.LoginAsync(dto);
        return Ok(data);
    }

    [Authorize]
    [HttpPost("me")]
    public async Task<ActionResult<AuthUserDto>> Me()
    {
        var userId = GetUserId();
        var data = await _authService.GetCurrentUserAsync(userId);
        return Ok(data);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Contraseña actualizada exitosamente" });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst("userId")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Token inválido");

        return userId;
    }
}
