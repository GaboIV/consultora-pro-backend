namespace ConsultoraPro.Domain.Models;

public class Cliente
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Industria { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public string ColorClass { get; set; } = "blue";
    public int TotalProyectos { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}
