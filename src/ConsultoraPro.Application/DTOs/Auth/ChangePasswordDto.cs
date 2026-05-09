namespace ConsultoraPro.Application.DTOs.Auth;

public class ChangePasswordDto
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}
