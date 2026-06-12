namespace ConsultoraPro.Application.DTOs.AmbienteTestUsers;

public class AmbienteTestUserDto
{
    public Guid Id { get; set; }
    public Guid AmbienteId { get; set; }
    public string RolAplicacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordCifrado { get; set; } = string.Empty;
    public string? Notas { get; set; }
}

public class CreateAmbienteTestUserDto
{
    public Guid AmbienteId { get; set; }
    public string RolAplicacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Notas { get; set; }
}

public class UpdateAmbienteTestUserDto
{
    public string RolAplicacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? Notas { get; set; }
}
