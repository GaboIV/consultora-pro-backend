using ConsultoraPro.Application.DTOs.Auth;

namespace ConsultoraPro.API.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    /// <summary>
    /// Encuentra o aprovisiona al usuario autenticado por Google y emite el JWT propio.
    /// Lanza <see cref="ConsultoraPro.API.Services.GoogleDomainNotAllowedException"/> si el
    /// dominio del email no está en la lista permitida.
    /// </summary>
    Task<AuthResponseDto> LoginWithGoogleAsync(string email, string? fullName, string? pictureUrl);
    Task<AuthUserDto> GetCurrentUserAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<AuthResponseDto> UpdatePerfilAsync(Guid userId, UpdatePerfilDto dto);
}
