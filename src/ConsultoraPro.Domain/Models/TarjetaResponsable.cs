namespace ConsultoraPro.Domain.Models;

public class TarjetaResponsable
{
    public Guid Id { get; set; }
    public Guid TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
}
