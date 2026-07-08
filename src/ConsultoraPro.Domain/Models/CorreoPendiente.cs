using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

/// <summary>
/// Outbox de correos: la emisión de negocio solo inserta filas y un procesador en segundo
/// plano las envía. Los tipos agrupables se programan con una ventana (ProgramadoPara en el
/// futuro) para juntarse en un único correo resumen por usuario.
/// </summary>
public class CorreoPendiente
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public TipoNotificacion Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Url { get; set; }
    /// <summary>
    /// Clave de deduplicación: si llega otro evento con la misma clave mientras esta fila
    /// sigue Pendiente, se actualiza el contenido en lugar de insertar una fila nueva
    /// (ej. una tarjeta movida varias veces dentro de la ventana → un solo renglón).
    /// </summary>
    public string? DedupKey { get; set; }
    public EstadoCorreo Estado { get; set; } = EstadoCorreo.Pendiente;
    /// <summary>Momento a partir del cual el procesador puede enviar este correo.</summary>
    public DateTime ProgramadoPara { get; set; } = DateTime.UtcNow;
    public int Intentos { get; set; }
    public string? UltimoError { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEnvio { get; set; }
}
