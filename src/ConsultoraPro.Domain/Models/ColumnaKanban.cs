namespace ConsultoraPro.Domain.Models;

public class ColumnaKanban
{
    public Guid Id { get; set; }
    public Guid TableroId { get; set; }
    public Tablero Tablero { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;     // "Por hacer"
    public double Orden { get; set; }                       // rango fraccional
    public int? LimiteWip { get; set; }                     // WIP limit opcional
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Tarjeta> Tarjetas { get; set; } = new List<Tarjeta>();
}
