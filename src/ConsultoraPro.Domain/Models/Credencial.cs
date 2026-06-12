using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Credencial
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoCredencial Tipo { get; set; }
    public string Servidor { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int? Puerto { get; set; }
    public string? Usuario { get; set; }
    public string? Url { get; set; }
    public string? Notas { get; set; }
    /// <summary>JSON con campos específicos no sensibles según el tipo (motor BD, dominio RDP, gateway VPN, etc.).</summary>
    public string? CamposExtra { get; set; }
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public Guid? AmbienteId { get; set; }
    public Ambiente? Ambiente { get; set; }
    public string ValorCifrado { get; set; } = string.Empty;
    /// <summary>JSON cifrado con secretos adicionales (passphrase, pre-shared key, clave de bastión, etc.).</summary>
    public string? SecretosExtraCifrado { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public Guid CreadoPor { get; set; }
    public ApplicationUser Creador { get; set; } = null!;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
    public ICollection<AuditoriaCredencial> Auditorias { get; set; } = new List<AuditoriaCredencial>();
}
