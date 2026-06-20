using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class ActividadTarjeta
{
    public Guid Id { get; set; }
    public Guid TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;
    public Guid? UsuarioId { get; set; }
    public ApplicationUser? Usuario { get; set; }
    public TipoActividadTarjeta Tipo { get; set; }
    public string? Detalle { get; set; }                    // texto legible o JSON
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
