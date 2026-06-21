using Microsoft.AspNetCore.Identity;

namespace ConsultoraPro.Domain.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcceso { get; set; }

    /// <summary>Origen de la cuenta: "local" (email+password) o "google" (OAuth).</summary>
    public string AuthProvider { get; set; } = "local";

    /// <summary>URL de la foto de perfil (proporcionada por Google), opcional.</summary>
    public string? AvatarUrl { get; set; }

    public ICollection<ProyectoMiembro> ProyectosMiembro { get; set; } = new List<ProyectoMiembro>();
}
