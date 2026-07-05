namespace ConsultoraPro.Application.Interfaces;

public sealed record EmailMessage(
    string To,
    string ToName,
    string Subject,
    string HtmlBody,
    string TextBody);

/// <summary>Envío de un correo ya renderizado. Implementaciones: Console, Smtp, Brevo, Resend.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
