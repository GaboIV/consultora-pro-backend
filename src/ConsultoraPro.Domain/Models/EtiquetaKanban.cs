namespace ConsultoraPro.Domain.Models;

public class EtiquetaKanban
{
    public Guid Id { get; set; }
    public Guid TableroId { get; set; }
    public Tablero Tablero { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;     // "Backend", "Urgente"
    public string ColorClass { get; set; } = "blue";
    public bool Activo { get; set; } = true;

    public ICollection<TarjetaEtiqueta> Tarjetas { get; set; } = new List<TarjetaEtiqueta>();
}
