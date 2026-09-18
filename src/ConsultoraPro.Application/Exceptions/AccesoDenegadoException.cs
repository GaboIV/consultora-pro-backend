namespace ConsultoraPro.Application.Exceptions;

/// <summary>
/// El usuario está autenticado y tiene el permiso del módulo, pero no alcance sobre el recurso
/// (p. ej. un proyecto del que no es miembro). El middleware la traduce a HTTP 403.
/// </summary>
public class AccesoDenegadoException : Exception
{
    public AccesoDenegadoException(string message = "No tienes acceso a este proyecto.") : base(message)
    {
    }
}
