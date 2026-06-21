using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

/// <summary>
/// Solicitud de un usuario con nivel "básico" para revelar los secretos de una credencial.
/// Un aprobador con <c>credenciales.solicitud.aprobar</c> la resuelve; si la aprueba, concede una
/// revelación temporal vigente hasta <see cref="VigenteHasta"/> (no un permiso permanente).
/// </summary>
public class SolicitudRevelacionCredencial
{
    public Guid Id { get; set; }

    public Guid CredencialId { get; set; }
    public Credencial Credencial { get; set; } = null!;

    public Guid SolicitanteId { get; set; }
    public ApplicationUser Solicitante { get; set; } = null!;

    public Guid? AprobadorId { get; set; }
    public ApplicationUser? Aprobador { get; set; }

    public EstadoSolicitudRevelacion Estado { get; set; } = EstadoSolicitudRevelacion.Pendiente;

    /// <summary>Justificación opcional escrita por el solicitante.</summary>
    public string? Motivo { get; set; }

    /// <summary>Nota opcional del aprobador al resolver (motivo del rechazo, condiciones, etc.).</summary>
    public string? NotaResolucion { get; set; }

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucion { get; set; }

    /// <summary>Ventana durante la cual la revelación aprobada es válida. Null si no está aprobada.</summary>
    public DateTime? VigenteHasta { get; set; }

    /// <summary>True si la solicitud está aprobada y su ventana de vigencia aún no expiró.</summary>
    public bool EsVigente(DateTime now) =>
        Estado == EstadoSolicitudRevelacion.Aprobada && VigenteHasta.HasValue && VigenteHasta.Value > now;
}
