namespace ConsultoraPro.Domain.Models;

public class Tablero
{
    public Guid Id { get; set; }
    public Guid? ProyectoId { get; set; }           // null → tablero personal
    public Proyecto? Proyecto { get; set; }
    public Guid? CreadoPorId { get; set; }           // creador (Owner implícito)
    public ApplicationUser? CreadoPor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string ColorClass { get; set; } = "blue";
    public int Orden { get; set; }
    public int SecuenciaActual { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ColumnaKanban> Columnas { get; set; } = new List<ColumnaKanban>();
    public ICollection<TableroMiembro> Miembros { get; set; } = new List<TableroMiembro>();
    public ICollection<EtiquetaKanban> Etiquetas { get; set; } = new List<EtiquetaKanban>();
}
