namespace ConsultoraPro.Application.Configuration;

public class NotificacionesOptions
{
    public const string SectionName = "Notificaciones";

    /// <summary>Ventana en minutos para agrupar en un resumen los correos de tipos agrupables.</summary>
    public int VentanaAgrupacionMinutos { get; set; } = 10;

    /// <summary>Frecuencia con la que el procesador revisa el outbox.</summary>
    public int IntervaloProcesadorSegundos { get; set; } = 30;

    /// <summary>Reintentos máximos antes de marcar un correo como Error.</summary>
    public int MaxIntentos { get; set; } = 5;

    /// <summary>Máximo de filas del outbox tomadas por ciclo.</summary>
    public int CorreosPorLote { get; set; } = 50;
}
