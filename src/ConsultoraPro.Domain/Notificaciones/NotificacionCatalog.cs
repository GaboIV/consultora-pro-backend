using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Notificaciones;

/// <summary>
/// Metadatos de cada tipo de notificación: defaults de canal, agrupación anti-spam y
/// si el correo es obligatorio (seguridad). Es la fuente de verdad tanto para la
/// emisión como para la pantalla de preferencias del usuario.
/// </summary>
public sealed record NotificacionDefinition(
    TipoNotificacion Tipo,
    string Grupo,
    string Nombre,
    string Descripcion,
    bool EnAppPorDefecto,
    bool CorreoPorDefecto,
    // True: el correo se agrupa en una ventana (resumen) en lugar de enviarse al instante.
    bool Agrupable,
    // True: notificación de seguridad; el usuario no puede desactivar el correo.
    bool CorreoObligatorio);

public static class NotificacionCatalog
{
    public const string GrupoCuenta = "Cuenta y seguridad";
    public const string GrupoProyectos = "Proyectos";
    public const string GrupoKanban = "Tableros";
    public const string GrupoCredenciales = "Credenciales";

    public static readonly IReadOnlyList<NotificacionDefinition> All = new List<NotificacionDefinition>
    {
        new(TipoNotificacion.UsuarioBienvenida, GrupoCuenta, "Cuenta creada",
            "Correo de bienvenida cuando se crea tu cuenta.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: true),
        new(TipoNotificacion.PasswordCambiadaPorAdmin, GrupoCuenta, "Contraseña cambiada por un administrador",
            "Aviso de seguridad cuando un administrador restablece tu contraseña.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: true),
        new(TipoNotificacion.PasswordCambiada, GrupoCuenta, "Contraseña cambiada",
            "Aviso de seguridad cuando tu contraseña cambia desde tu perfil.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: true),
        new(TipoNotificacion.RolCambiado, GrupoCuenta, "Cambio de rol",
            "Cuando un administrador cambia tu rol en el sistema.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: false),

        new(TipoNotificacion.ProyectoAsignado, GrupoProyectos, "Asignación a proyecto",
            "Cuando te dan acceso a uno o más proyectos.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: false),
        new(TipoNotificacion.ProyectoDesasignado, GrupoProyectos, "Retiro de proyecto",
            "Cuando te retiran el acceso a un proyecto.",
            EnAppPorDefecto: true, CorreoPorDefecto: false, Agrupable: false, CorreoObligatorio: false),

        new(TipoNotificacion.TableroCompartido, GrupoKanban, "Tablero compartido",
            "Cuando te agregan como miembro de un tablero.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: false),
        new(TipoNotificacion.TarjetaAsignada, GrupoKanban, "Tarjeta asignada",
            "Cuando te asignan como responsable de una tarjeta.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: false),
        new(TipoNotificacion.TarjetaDesasignada, GrupoKanban, "Tarjeta desasignada",
            "Cuando te quitan de los responsables de una tarjeta.",
            EnAppPorDefecto: true, CorreoPorDefecto: false, Agrupable: false, CorreoObligatorio: false),
        new(TipoNotificacion.TarjetaMovida, GrupoKanban, "Movimiento de tarjetas",
            "Cuando cambian de columna tarjetas donde eres responsable. Los correos se agrupan en un resumen.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: true, CorreoObligatorio: false),
        new(TipoNotificacion.TarjetaCompletada, GrupoKanban, "Tarjeta completada o reabierta",
            "Cuando se completan o reabren tarjetas donde eres responsable. Los correos se agrupan en un resumen.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: true, CorreoObligatorio: false),
        new(TipoNotificacion.TarjetaComentario, GrupoKanban, "Comentarios en tarjetas",
            "Cuando comentan tarjetas donde participas. Los correos se agrupan en un resumen.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: true, CorreoObligatorio: false),

        new(TipoNotificacion.CredencialSolicitud, GrupoCredenciales, "Solicitud de revelación",
            "Para aprobadores: cuando alguien solicita revelar una credencial.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: false),
        new(TipoNotificacion.CredencialSolicitudResuelta, GrupoCredenciales, "Solicitud resuelta",
            "Cuando aprueban o rechazan tu solicitud de revelación.",
            EnAppPorDefecto: true, CorreoPorDefecto: true, Agrupable: false, CorreoObligatorio: false)
    };

    private static readonly IReadOnlyDictionary<TipoNotificacion, NotificacionDefinition> ByTipo =
        All.ToDictionary(d => d.Tipo);

    public static NotificacionDefinition Get(TipoNotificacion tipo) => ByTipo[tipo];
}
