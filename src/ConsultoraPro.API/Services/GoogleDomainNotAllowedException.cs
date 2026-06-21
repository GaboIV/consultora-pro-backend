namespace ConsultoraPro.API.Services;

/// <summary>
/// El email autenticado por Google pertenece a un dominio que no está en ALLOWED_DOMAINS.
/// El AuthController la captura para redirigir al login con un mensaje de error en vez de
/// devolver un 500.
/// </summary>
public class GoogleDomainNotAllowedException : Exception
{
    public GoogleDomainNotAllowedException(string message) : base(message) { }
}
