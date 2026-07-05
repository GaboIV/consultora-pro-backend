using ConsultoraPro.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ConsultoraPro.Infrastructure.Email;

/// <summary>Proveedor por defecto en desarrollo: escribe el correo en el log en lugar de enviarlo.</summary>
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[Email:Console] Para: {To} | Asunto: {Subject}\n{Body}",
            message.To, message.Subject, message.TextBody);
        return Task.CompletedTask;
    }
}
