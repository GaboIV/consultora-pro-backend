namespace ConsultoraPro.Application.DTOs.AmbienteCloudResources;

public class AmbienteCloudResourceDto
{
    public Guid Id { get; set; }
    public Guid AmbienteId { get; set; }
    public string TipoRecurso { get; set; } = string.Empty;
    public string NombreRecurso { get; set; } = string.Empty;
    public string? DeepLink { get; set; }
}

public class CreateAmbienteCloudResourceDto
{
    public Guid AmbienteId { get; set; }
    public string TipoRecurso { get; set; } = string.Empty;
    public string NombreRecurso { get; set; } = string.Empty;
    public string? DeepLink { get; set; }
}

public class UpdateAmbienteCloudResourceDto
{
    public string TipoRecurso { get; set; } = string.Empty;
    public string NombreRecurso { get; set; } = string.Empty;
    public string? DeepLink { get; set; }
}
