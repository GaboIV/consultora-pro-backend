namespace ConsultoraPro.Application.DTOs.Kanban;

public class AdjuntoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long TamanoBytes { get; set; }
    public Guid? SubidoPorId { get; set; }
    public string? SubidoPorNombre { get; set; }
    public DateTime FechaSubida { get; set; }
}
