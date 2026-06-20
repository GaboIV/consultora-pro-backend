namespace ConsultoraPro.Domain.Models;

public class ChecklistItem
{
    public Guid Id { get; set; }
    public Guid ChecklistId { get; set; }
    public Checklist Checklist { get; set; } = null!;
    public string Texto { get; set; } = string.Empty;
    public bool Completado { get; set; }
    public double Orden { get; set; }
}
