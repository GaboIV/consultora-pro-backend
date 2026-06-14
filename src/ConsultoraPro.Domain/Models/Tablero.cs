namespace ConsultoraPro.Domain.Models;

public class Tablero
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;     // "Tareas"
    public string Clave { get; set; } = string.Empty;      // "TAR" (único dentro del proyecto)
    public string? Descripcion { get; set; }
    public string ColorClass { get; set; } = "blue";
    public int Orden { get; set; }                          // orden del tablero dentro del proyecto
    public int SecuenciaActual { get; set; }                // contador para el código de tarjeta
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ColumnaKanban> Columnas { get; set; } = new List<ColumnaKanban>();
    public ICollection<TableroMiembro> Miembros { get; set; } = new List<TableroMiembro>();
    public ICollection<EtiquetaKanban> Etiquetas { get; set; } = new List<EtiquetaKanban>();
}
