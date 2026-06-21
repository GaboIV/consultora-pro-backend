namespace ConsultoraPro.Application.Exceptions;

/// <summary>
/// Se lanza cuando un usuario de nivel básico intenta revelar una credencial sin tener una
/// aprobación de revelación vigente. El controlador la traduce a HTTP 409 con un código que el
/// frontend usa para ofrecer la creación de una solicitud (evita el redirect global de los 403).
/// </summary>
public class RevelacionRequiereSolicitudException : Exception
{
    public const string Code = "REVELACION_REQUIERE_SOLICITUD";

    public RevelacionRequiereSolicitudException()
        : base("Necesitas solicitar autorización para revelar los secretos de esta credencial.")
    {
    }
}
