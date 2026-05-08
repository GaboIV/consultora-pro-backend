using ConsultoraPro.Application.DTOs.Auth;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.API.Interfaces;
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
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var data = await _authService.LoginAsync(dto);
        return Ok(new ApiResponse<AuthResponseDto> { Success = true, Data = data, Message = "Inicio de sesión exitoso" });
    }
}
