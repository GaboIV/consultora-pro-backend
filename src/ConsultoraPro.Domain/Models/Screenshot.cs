using System;

namespace ConsultoraPro.Domain.Models;

public class Screenshot
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    /// <summary>Key relativa en el almacenamiento (ej. "screenshots/{guid}.png"). No es una URL absoluta.</summary>
    public string StorageKey { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public Guid SubidoPorId { get; set; }
    public ApplicationUser SubidoPor { get; set; } = null!;
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
