using System.Security.Claims;
using ConsultoraPro.API.Authorization;
using ConsultoraPro.API.Services;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.DTOs.Auth;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.API.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AuthOptions _authOptions;

    public AuthController(IAuthService authService, IOptions<AuthOptions> authOptions)
    {
        _authService = authService;
        _authOptions = authOptions.Value;
    }

    [HttpGet("config")]
    public ActionResult<AuthConfigDto> GetConfig()
    {
        return Ok(new AuthConfigDto
        {
            CredentialsEnabled = _authOptions.CredentialsEnabled,
            GoogleEnabled = _authOptions.Google.Enabled
                && !string.IsNullOrWhiteSpace(_authOptions.Google.ClientId)
        });
    }

    // Inicia el flujo OAuth: redirige al consentimiento de Google. Tras autenticar, Google
    // vuelve al CallbackPath del middleware, que a su vez redirige a la acción "complete".
    [HttpGet("google/start")]
    public IActionResult GoogleStart()
    {
        if (!_authOptions.Google.Enabled)
            return NotFound();

        var props = new AuthenticationProperties { RedirectUri = "/api/auth/google/complete" };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    // Lee los claims externos depositados por el handler de Google, encuentra/aprovisiona al
    // usuario, emite el JWT propio y redirige al frontend con el token en el fragment de la URL.
    [HttpGet("google/complete")]
    public async Task<IActionResult> GoogleComplete()
    {
        if (!_authOptions.Google.Enabled)
            return NotFound();

        var frontend = _authOptions.Google.FrontendBaseUrl.TrimEnd('/');
        var result = await HttpContext.AuthenticateAsync(AuthSchemes.External);

        if (!result.Succeeded || result.Principal is null)
            return Redirect($"{frontend}/login?error=google_failed");

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var name = result.Principal.FindFirstValue(ClaimTypes.Name);
        var picture = result.Principal.FindFirstValue("urn:google:picture");

        try
        {
            var auth = await _authService.LoginWithGoogleAsync(email!, name, picture);
            await HttpContext.SignOutAsync(AuthSchemes.External);

            var token = Uri.EscapeDataString(auth.Token);
            var expires = Uri.EscapeDataString(auth.ExpiresAt.ToString("o"));
            return Redirect($"{frontend}/auth/callback#token={token}&expiresAt={expires}");
        }
        catch (GoogleDomainNotAllowedException)
        {
            await HttpContext.SignOutAsync(AuthSchemes.External);
            return Redirect($"{frontend}/login?error=domain_not_allowed");
        }
        catch (UnauthorizedAccessException)
        {
            await HttpContext.SignOutAsync(AuthSchemes.External);
            return Redirect($"{frontend}/login?error=google_failed");
        }
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
    public async Task<ActionResult<AuthUserDto>> MePost()
    {
        var userId = GetUserId();
        var data = await _authService.GetCurrentUserAsync(userId);
        return Ok(data);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthUserDto>> MeGet()
    {
        var userId = GetUserId();
        var data = await _authService.GetCurrentUserAsync(userId);
        return Ok(data);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePasswordPost([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Contraseña actualizada exitosamente" });
    }

    [Authorize]
    [HttpPut("cambiar-password")]
    public async Task<ActionResult<ApiResponse<object>>> CambiarPasswordPut([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Contraseña actualizada exitosamente" });
    }

    [Authorize]
    [HttpPut("perfil")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> UpdatePerfil([FromBody] UpdatePerfilDto dto)
    {
        var userId = GetUserId();
        var data = await _authService.UpdatePerfilAsync(userId, dto);
        return Ok(new ApiResponse<AuthResponseDto> { Success = true, Data = data, Message = "Perfil actualizado exitosamente" });
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
