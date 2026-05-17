namespace ConsultoraPro.Application.DTOs.AmbienteComponentes;

public class AmbienteComponenteDto
{
    public Guid Id { get; set; }
    public Guid AmbienteId { get; set; }
    public string Rol { get; set; } = string.Empty;
    public string? IpPublica { get; set; }
    public string? IpPrivada { get; set; }
    public string? Hostname { get; set; }
    public string? Tecnologia { get; set; }
    public string? Especificaciones { get; set; }
}

public class CreateAmbienteComponenteDto
{
    public Guid AmbienteId { get; set; }
    public string Rol { get; set; } = string.Empty;
    public string? IpPublica { get; set; }
    public string? IpPrivada { get; set; }
    public string? Hostname { get; set; }
    public string? Tecnologia { get; set; }
    public string? Especificaciones { get; set; }
}

public class UpdateAmbienteComponenteDto
{
    public string Rol { get; set; } = string.Empty;
    public string? IpPublica { get; set; }
    public string? IpPrivada { get; set; }
    public string? Hostname { get; set; }
    public string? Tecnologia { get; set; }
    public string? Especificaciones { get; set; }
}
