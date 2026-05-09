namespace ConsultoraPro.Domain.Models;

public class Permiso
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<RolPermiso> Roles { get; set; } = new List<RolPermiso>();
}
