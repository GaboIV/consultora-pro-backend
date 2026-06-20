namespace ConsultoraPro.Application.DTOs.Kanban;

public class ComentarioDto
{
    public Guid Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public Guid AutorId { get; set; }
    public string AutorNombre { get; set; } = string.Empty;
    public string AutorIniciales { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? EditadoEn { get; set; }
}

public class CreateComentarioDto
{
    public string Texto { get; set; } = string.Empty;
}

public class UpdateComentarioDto
{
    public string Texto { get; set; } = string.Empty;
}
