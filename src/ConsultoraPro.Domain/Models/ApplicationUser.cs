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
    public ICollection<ProyectoMiembro> ProyectosMiembro { get; set; } = new List<ProyectoMiembro>();
}
