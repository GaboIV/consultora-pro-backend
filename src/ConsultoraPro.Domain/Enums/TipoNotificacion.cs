namespace ConsultoraPro.Domain.Enums;

public enum TipoNotificacion
{
    // Cuenta y seguridad
    UsuarioBienvenida,
    PasswordCambiadaPorAdmin,
    PasswordCambiada,
    RolCambiado,

    // Proyectos
    ProyectoAsignado,
    ProyectoDesasignado,

    // Kanban
    TableroCompartido,
    TarjetaAsignada,
    TarjetaDesasignada,
    TarjetaMovida,
    TarjetaCompletada,
    TarjetaComentario,

    // Credenciales
    CredencialSolicitud,
    CredencialSolicitudResuelta
}
