namespace ConsultoraPro.Domain.Models;

public class AmbienteCloudResource
{
    public Guid Id { get; set; }
    public Guid AmbienteId { get; set; }
    public Ambiente Ambiente { get; set; } = null!;
    public string TipoRecurso { get; set; } = string.Empty;
    public string NombreRecurso { get; set; } = string.Empty;
    public string? DeepLink { get; set; }
    public bool Activo { get; set; } = true;
}
