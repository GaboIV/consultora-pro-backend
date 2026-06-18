namespace ConsultoraPro.Domain.Models;

public class Checklist
{
    public Guid Id { get; set; }
    public Guid TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
    public double Orden { get; set; }

    public ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
}
