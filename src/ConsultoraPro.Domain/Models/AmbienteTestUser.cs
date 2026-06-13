namespace ConsultoraPro.Domain.Models;

public class AmbienteTestUser
{
    public Guid Id { get; set; }
    public Guid AmbienteId { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
    public string RolAplicacion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordCifrado { get; set; } = string.Empty;
    public string? Notas { get; set; }
    public bool Activo { get; set; } = true;
}
