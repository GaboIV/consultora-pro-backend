namespace ConsultoraPro.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AuthUserDto User { get; set; } = new();
}

public class AuthUserDto
{
    public Guid Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public IReadOnlyList<string> Permisos { get; set; } = Array.Empty<string>();
}
