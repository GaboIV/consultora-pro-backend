namespace ConsultoraPro.Domain.Models;

public class TipoSolucion
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
