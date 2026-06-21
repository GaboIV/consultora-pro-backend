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

    /// <summary>
    /// Si es true, los usuarios con este rol ven todos los proyectos sin necesidad de asignación
    /// explícita. Si es false, solo acceden a los proyectos donde son miembros (ver <see cref="ProyectoMiembro"/>).
    /// </summary>
    public bool AccesoTotalProyectos { get; set; }

    /// <summary>
    /// Rol creado por el seeder del sistema. Sus flags estructurales (acceso total, nombre) están
    /// bloqueados para edición desde la API/UI.
    /// </summary>
    public bool EsSistema { get; set; }

    /// <summary>Versión de los permisos del rol. Se incrementa al cambiar permisos del rol para invalidar JWTs.</summary>
    public int PermVersion { get; set; }

    public ICollection<RolPermiso> Permisos { get; set; } = new List<RolPermiso>();
}
