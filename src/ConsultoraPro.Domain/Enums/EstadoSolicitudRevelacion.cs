namespace ConsultoraPro.Domain.Enums;

/// <summary>Estado de una solicitud de revelación de secretos de una credencial (nivel básico).</summary>
public enum EstadoSolicitudRevelacion
{
    Pendiente = 0,
    Aprobada = 1,
    Rechazada = 2,
    Expirada = 3
}
