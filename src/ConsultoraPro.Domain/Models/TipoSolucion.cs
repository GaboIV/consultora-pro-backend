namespace ConsultoraPro.Domain.Models;

public class TipoSolucion
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el tipo se ofrece al seleccionar el tipo de solución de un proyecto.
    /// Un tipo inactivo se conserva (no rompe proyectos históricos) pero deja de aparecer
    /// en el selector de "Nuevo proyecto".
    /// </summary>
    public bool Activo { get; set; } = true;

    public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
