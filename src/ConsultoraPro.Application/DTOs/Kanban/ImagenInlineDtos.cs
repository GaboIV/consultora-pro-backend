namespace ConsultoraPro.Application.DTOs.Kanban;

public class ImagenInlineDto
{
    /// <summary>Placeholder estable ("cpfile://{key}") que el editor debe persistir en la descripción.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>URL firmada (SAS) para previsualización inmediata en el editor.</summary>
    public string Url { get; set; } = string.Empty;
}
