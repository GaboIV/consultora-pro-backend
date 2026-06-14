namespace ConsultoraPro.Domain.Models;

public class ChecklistItem
{
    public Guid Id { get; set; }
    public Guid TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;
    public string Texto { get; set; } = string.Empty;
    public bool Completado { get; set; }
    public double Orden { get; set; }
}
