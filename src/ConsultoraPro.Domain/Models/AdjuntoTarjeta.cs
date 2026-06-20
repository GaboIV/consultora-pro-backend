namespace ConsultoraPro.Domain.Models;

public class AdjuntoTarjeta
{
    public Guid Id { get; set; }
    public Guid TarjetaId { get; set; }
    public Tarjeta Tarjeta { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Key relativa en el almacenamiento (ej. "adjuntos/{guid}.pdf"). No es una URL absoluta.</summary>
    public string StorageKey { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long TamanoBytes { get; set; }
    public Guid? SubidoPorId { get; set; }
    public ApplicationUser? SubidoPor { get; set; }
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
}
