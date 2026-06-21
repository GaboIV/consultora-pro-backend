using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsultoraPro.Application.Configuration;

/// <summary>
/// Configuración de autenticación (sección "Auth"). Controla qué métodos de login
/// están habilitados (credenciales y/o Google) y los parámetros del OAuth de Google.
///
/// Para conservar los nombres de variables que ya usas en Outline, el binder de
/// <c>Program.cs</c> rellena <see cref="GoogleAuthOptions.ClientId"/>,
/// <see cref="GoogleAuthOptions.ClientSecret"/> y <see cref="GoogleAuthOptions.AllowedDomains"/>
/// desde las env vars planas GOOGLE_CLIENT_ID / GOOGLE_CLIENT_SECRET / ALLOWED_DOMAINS
/// cuando la sección "Auth:Google" no las trae.
/// </summary>
public class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Habilita el login por email + contraseña (formulario visible en el front).</summary>
    public bool CredentialsEnabled { get; set; } = true;

    /// <summary>
    /// Emails que conservan acceso por contraseña aun cuando <see cref="CredentialsEnabled"/>
    /// es false (cuenta admin "break-glass" de emergencia, oculta en la UI).
    /// </summary>
    public List<string> BreakGlassEmails { get; set; } = new();

    public GoogleAuthOptions Google { get; set; } = new();

    /// <summary>True si el login por contraseña debe permitirse para el email dado.</summary>
    public bool IsPasswordLoginAllowed(string? email)
    {
        if (CredentialsEnabled)
            return true;

        return !string.IsNullOrWhiteSpace(email)
            && BreakGlassEmails.Any(e => string.Equals(e, email, StringComparison.OrdinalIgnoreCase));
    }
}

public class GoogleAuthOptions
{
    /// <summary>Activa el botón "Continuar con Google" y registra el esquema OAuth.</summary>
    public bool Enabled { get; set; }

    /// <summary>OAuth Client ID (ó env GOOGLE_CLIENT_ID).</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>OAuth Client Secret (ó env GOOGLE_CLIENT_SECRET).</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Dominios permitidos separados por coma (ó env ALLOWED_DOMAINS), ej. "equaly.pe,qconsultores.pe".</summary>
    public string AllowedDomains { get; set; } = string.Empty;

    /// <summary>Base pública del frontend a donde redirigir tras emitir el JWT.</summary>
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";

    /// <summary>Dominios permitidos ya normalizados (minúsculas, sin espacios).</summary>
    public IReadOnlyList<string> AllowedDomainList =>
        (AllowedDomains ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => d.ToLowerInvariant())
            .ToList();

    /// <summary>True si el dominio del email está dentro de la lista permitida (o si la lista está vacía).</summary>
    public bool IsDomainAllowed(string? email)
    {
        var domains = AllowedDomainList;
        if (domains.Count == 0)
            return true;

        if (string.IsNullOrWhiteSpace(email))
            return false;

        var at = email.LastIndexOf('@');
        if (at < 0 || at == email.Length - 1)
            return false;

        var domain = email[(at + 1)..].ToLowerInvariant();
        return domains.Contains(domain);
    }
}
