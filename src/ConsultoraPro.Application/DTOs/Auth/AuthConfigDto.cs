namespace ConsultoraPro.Application.DTOs.Auth;

/// <summary>
/// Configuración pública de autenticación que consume el frontend para decidir qué
/// métodos de login mostrar (formulario de credenciales y/o botón de Google).
/// </summary>
public class AuthConfigDto
{
    public bool CredentialsEnabled { get; set; }
    public bool GoogleEnabled { get; set; }
}
