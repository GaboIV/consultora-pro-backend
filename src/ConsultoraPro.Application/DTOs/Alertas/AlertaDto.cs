using System;

namespace ConsultoraPro.Application.DTOs.Alertas;

public class AlertaDto
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // CredencialVencimiento, ProyectoVencimiento, AmbienteAlerta
    public string Mensaje { get; set; } = string.Empty;
    public string Tone { get; set; } = "info"; // info, warn, red, amber
    public bool EsCritica { get; set; }
    public string ReferenciaId { get; set; } = string.Empty;
    public DateTime? FechaReferencia { get; set; }
}
