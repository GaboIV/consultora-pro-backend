using System.Net;
using System.Net.Mail;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.Infrastructure.Email;

/// <summary>
/// Envío por SMTP. Pensado para pruebas con Gmail (smtp.gmail.com:587 + app password),
/// aunque sirve para cualquier servidor SMTP estándar.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var smtp = _options.Smtp;
        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            Credentials = new NetworkCredential(smtp.User, smtp.Password)
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(_options.From, _options.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };
        mail.To.Add(new MailAddress(message.To, message.ToName));
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            message.TextBody, null, "text/plain"));
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            message.HtmlBody, null, "text/html"));

        await client.SendMailAsync(mail, cancellationToken);
    }
}
