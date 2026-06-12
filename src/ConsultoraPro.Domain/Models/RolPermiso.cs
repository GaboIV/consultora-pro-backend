namespace ConsultoraPro.Domain.Models;

public class RolPermiso
{
    public Guid RolId { get; set; }
    public ApplicationRole Rol { get; set; } = null!;
    public int PermisoId { get; set; }
    public Permiso Permiso { get; set; } = null!;
    public bool Concedido { get; set; } = true;
}
