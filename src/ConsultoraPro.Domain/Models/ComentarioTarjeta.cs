namespace ConsultoraPro.Domain.Models;

public class ComentarioTarjeta
{
    public Guid Id { get; set; }
    public Guid TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;
    public Guid AutorId { get; set; }
    public ApplicationUser Autor { get; set; } = null!;
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? EditadoEn { get; set; }
}
