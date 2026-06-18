using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Kanban;

public class ResponsableDto
{
    public Guid UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
}

public class TarjetaDto
{
    public Guid Id { get; set; }
    public Guid ColumnaId { get; set; }
    public Guid TableroId { get; set; }
    public int Numero { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public double Orden { get; set; }
    public PrioridadTarjeta Prioridad { get; set; }
    public DateTime? FechaLimite { get; set; }
    public bool Completada { get; set; }
    public List<ResponsableDto> Responsables { get; set; } = new();
    public List<EtiquetaDto> Etiquetas { get; set; } = new();
    public int ChecklistCompletados { get; set; }
    public int ChecklistTotal { get; set; }
    public int TotalComentarios { get; set; }
    public int TotalAdjuntos { get; set; }
    public string? Descripcion { get; set; }
    public string? PortadaAdjuntoUrl { get; set; }
}

public class TarjetaDetalleDto : TarjetaDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreadaPorId { get; set; }
    public string? CreadaPorNombre { get; set; }
    public List<ChecklistItemDto> Checklist { get; set; } = new();
    public List<ComentarioDto> Comentarios { get; set; } = new();
    public List<AdjuntoDto> Adjuntos { get; set; } = new();
}

public class CreateTarjetaDto
{
    public Guid ColumnaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public PrioridadTarjeta Prioridad { get; set; } = PrioridadTarjeta.Media;
    public DateTime? FechaLimite { get; set; }
    public DateTime? FechaInicio { get; set; }
    public List<Guid> ResponsableIds { get; set; } = new();
    public List<Guid> EtiquetaIds { get; set; } = new();
}

public class UpdateTarjetaDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public PrioridadTarjeta Prioridad { get; set; } = PrioridadTarjeta.Media;
    public DateTime? FechaLimite { get; set; }
    public DateTime? FechaInicio { get; set; }
    public bool Completada { get; set; }
}

public class MoverTarjetaDto
{
    public Guid ColumnaDestinoId { get; set; }
    public Guid? AntesDeTarjetaId { get; set; }
    public Guid? DespuesDeTarjetaId { get; set; }
}

public class AsignarResponsablesDto
{
    public List<Guid> UsuarioIds { get; set; } = new();
}

public class AsignarEtiquetasDto
{
    public List<Guid> EtiquetaIds { get; set; } = new();
}
