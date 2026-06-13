namespace ConsultoraPro.Application.DTOs.AmbienteCloudResources;

public class AmbienteCloudResourceDto
{
    public Guid Id { get; set; }
    public Guid AmbienteId { get; set; }
    public string TipoRecurso { get; set; } = string.Empty;
    public string NombreRecurso { get; set; } = string.Empty;
    public string? DeepLink { get; set; }
    public string Plataforma { get; set; } = "Azure";
    public string? Ubicacion { get; set; }
    public string? Nota { get; set; }
}

public class CreateAmbienteCloudResourceDto
{
    public Guid AmbienteId { get; set; }
    public string TipoRecurso { get; set; } = string.Empty;
    public string NombreRecurso { get; set; } = string.Empty;
    public string? DeepLink { get; set; }
    public string Plataforma { get; set; } = "Azure";
    public string? Ubicacion { get; set; }
    public string? Nota { get; set; }
}

public class UpdateAmbienteCloudResourceDto
{
    public string TipoRecurso { get; set; } = string.Empty;
    public string NombreRecurso { get; set; } = string.Empty;
    public string? DeepLink { get; set; }
    public string Plataforma { get; set; } = "Azure";
    public string? Ubicacion { get; set; }
    public string? Nota { get; set; }
}

public class ImportCloudResourcesCsvRequest
{
    public string Plataforma { get; set; } = "Azure";
    public string CsvContent { get; set; } = string.Empty;
}

public class ImportCloudResourcesCsvResponse
{
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
