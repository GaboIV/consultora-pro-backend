using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Notificaciones;

/// <summary>Fila de la pantalla de preferencias: metadatos del catálogo + valores efectivos del usuario.</summary>
public class PreferenciaNotificacionDto
{
    public TipoNotificacion Tipo { get; set; }
    public string Grupo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Agrupable { get; set; }
    /// <summary>True: el usuario no puede desactivar el correo (notificación de seguridad).</summary>
    public bool CorreoObligatorio { get; set; }
    public bool EnApp { get; set; }
    public bool PorCorreo { get; set; }
}

public class UpdatePreferenciaNotificacionDto
{
    public TipoNotificacion Tipo { get; set; }
    public bool EnApp { get; set; }
    public bool PorCorreo { get; set; }
}

public class UpdatePreferenciasNotificacionDto
{
    public List<UpdatePreferenciaNotificacionDto> Preferencias { get; set; } = new();
}
