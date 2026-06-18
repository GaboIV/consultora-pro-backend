namespace ConsultoraPro.Application.DTOs.Kanban;

public class ChecklistDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public double Orden { get; set; }
    public List<ChecklistItemDto> Items { get; set; } = new();
}

public class CreateChecklistDto
{
    public string Nombre { get; set; } = string.Empty;
}

public class UpdateChecklistDto
{
    public string Nombre { get; set; } = string.Empty;
}

public class ChecklistItemDto
{
    public Guid Id { get; set; }
    public Guid ChecklistId { get; set; }
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
