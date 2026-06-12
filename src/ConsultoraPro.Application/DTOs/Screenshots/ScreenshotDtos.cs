using System;

namespace ConsultoraPro.Application.DTOs.Screenshots;

public class ScreenshotDto
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public Guid SubidoPorId { get; set; }
    public string SubidoPorNombre { get; set; } = string.Empty;
    public DateTime FechaSubida { get; set; }
}
