using Microsoft.AspNetCore.Identity;

namespace ConsultoraPro.Domain.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    public string Descripcion { get; set; } = string.Empty;
    public bool EsActivo { get; set; } = true;
    public ICollection<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();
}
