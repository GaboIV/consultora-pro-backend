using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Credenciales;

/// <summary>Cuerpo para crear una solicitud de revelación (nivel básico).</summary>
public class CrearSolicitudRevelacionDto
{
    public string? Motivo { get; set; }
}

/// <summary>Cuerpo para resolver (aprobar/rechazar) una solicitud.</summary>
public class ResolverSolicitudRevelacionDto
{
    public string? Nota { get; set; }
}

public class SolicitudRevelacionDto
{
    public Guid Id { get; set; }
    public Guid CredencialId { get; set; }
    public string CredencialNombre { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public Guid SolicitanteId { get; set; }
    public string SolicitanteNombre { get; set; } = string.Empty;
    public Guid? AprobadorId { get; set; }
    public string? AprobadorNombre { get; set; }
    public EstadoSolicitudRevelacion Estado { get; set; }
    public string? Motivo { get; set; }
    public string? NotaResolucion { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public DateTime? VigenteHasta { get; set; }
}
