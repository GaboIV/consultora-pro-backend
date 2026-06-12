namespace ConsultoraPro.Domain.Models;

public class AmbienteComponente
{
    public Guid Id { get; set; }
    public Guid AmbienteId { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
    public string Rol { get; set; } = string.Empty;
    public string? IpPublica { get; set; }
    public string? IpPrivada { get; set; }
    public string? Hostname { get; set; }
    public string? Tecnologia { get; set; }
    public string? Especificaciones { get; set; }
    public bool Activo { get; set; } = true;
}
