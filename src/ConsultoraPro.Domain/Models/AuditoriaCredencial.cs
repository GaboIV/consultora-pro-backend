namespace ConsultoraPro.Domain.Models;

public class AuditoriaCredencial
{
    public Guid Id { get; set; }
    public Guid CredencialId { get; set; }
    public Credencial Credencial { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public DateTime FechaRevelacion { get; set; } = DateTime.UtcNow;
    public string Ip { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
