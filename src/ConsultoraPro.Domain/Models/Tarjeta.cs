using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class Tarjeta
{
    public Guid Id { get; set; }
    public Guid ColumnaId { get; set; }
    public ColumnaKanban Columna { get; set; } = null!;
    public Guid TableroId { get; set; }                     // desnormalizado: tablero "dueño" del código
    public Tablero Tablero { get; set; } = null!;
    public int Numero { get; set; }                         // 1, 2, 3...
    public string Codigo { get; set; } = string.Empty;      // "REP-TAR-001" (inmutable, único global)
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }                // markdown
    public double Orden { get; set; }                       // rango fraccional dentro de la columna
    public PrioridadTarjeta Prioridad { get; set; } = PrioridadTarjeta.Media;
    public DateTime? FechaLimite { get; set; }
    public DateTime? FechaInicio { get; set; }
    public bool Completada { get; set; }
    public Guid? CreadaPorId { get; set; }
    public ApplicationUser? CreadaPor { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TarjetaResponsable> Responsables { get; set; } = new List<TarjetaResponsable>();
    public ICollection<TarjetaEtiqueta> Etiquetas { get; set; } = new List<TarjetaEtiqueta>();
    public ICollection<Checklist> Checklists { get; set; } = new List<Checklist>();
    public ICollection<ComentarioTarjeta> Comentarios { get; set; } = new List<ComentarioTarjeta>();
    public ICollection<AdjuntoTarjeta> Adjuntos { get; set; } = new List<AdjuntoTarjeta>();
    public ICollection<ActividadTarjeta> Actividades { get; set; } = new List<ActividadTarjeta>();
}
