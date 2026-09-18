namespace ConsultoraPro.Domain.Models;

/// <summary>
/// Carpeta del repositorio documental de un proyecto. Las carpetas de sistema provienen de la
/// estructura corporativa (<see cref="EsSistema"/>) y no se pueden renombrar ni eliminar; bajo
/// ellas el equipo puede crear subcarpetas propias.
/// </summary>
public class CarpetaDocumento : IProyectoScoped
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public CarpetaDocumento? Parent { get; set; }
    public ICollection<CarpetaDocumento> Subcarpetas { get; set; } = new List<CarpetaDocumento>();
    /// <summary>Prefijo numérico corporativo (ej. "05.2"). Vacío en subcarpetas creadas por el equipo.</summary>
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool EsSistema { get; set; }
    public Guid? CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
