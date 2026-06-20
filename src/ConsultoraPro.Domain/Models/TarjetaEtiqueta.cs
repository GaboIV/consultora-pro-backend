namespace ConsultoraPro.Domain.Models;

public class TarjetaEtiqueta
{
    public Guid TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;
    public Guid EtiquetaId { get; set; }
    public EtiquetaKanban Etiqueta { get; set; } = null!;
}
