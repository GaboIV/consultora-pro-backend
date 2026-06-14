namespace ConsultoraPro.Application.DTOs.Kanban;

public class ChecklistItemDto
{
    public Guid Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool Completado { get; set; }
    public double Orden { get; set; }
}

public class CreateChecklistItemDto
{
    public string Texto { get; set; } = string.Empty;
}

public class UpdateChecklistItemDto
{
    public string? Texto { get; set; }
    public bool? Completado { get; set; }
}
