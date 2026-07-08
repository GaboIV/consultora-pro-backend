namespace ConsultoraPro.Application.Configuration;

/// <summary>
/// Configuración del proveedor de correo saliente. Provider soporta:
/// "Console" (default, dev: imprime en logs), "Smtp" (ej. Gmail con app password),
/// "Brevo" y "Resend" (APIs HTTP para producción con dominio propio).
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Console";
    public string From { get; set; } = "no-reply@consultorapro.local";
    public string FromName { get; set; } = "ConsultoraPro";
    /// <summary>Base para construir los links de los correos (ej. https://app.miempresa.com).</summary>
    public string FrontendBaseUrl { get; set; } = "http://localhost:4200";

    public SmtpOptions Smtp { get; set; } = new();
    public BrevoOptions Brevo { get; set; } = new();
    public ResendOptions Resend { get; set; } = new();

    public class SmtpOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        /// <summary>STARTTLS en el puerto 587 (Gmail). Para SSL implícito usar puerto 465.</summary>
        public bool EnableSsl { get; set; } = true;
    }

    public class BrevoOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.brevo.com";
    }

    public class ResendOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.resend.com";
    }
}
