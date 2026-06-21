namespace ConsultoraPro.Domain.Models;

public class AuditoriaSeguridad
{
    public Guid Id { get; set; }
    public Guid ActorId { get; set; }
    public ApplicationUser Actor { get; set; } = null!;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public string? Antes { get; set; }
    public string? Despues { get; set; }
    public string Ip { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
