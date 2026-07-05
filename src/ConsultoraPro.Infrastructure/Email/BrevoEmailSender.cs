using System.Net.Http.Json;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.Infrastructure.Email;

/// <summary>Envío transaccional vía API de Brevo (https://developers.brevo.com/reference/sendtransacemail).</summary>
public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailOptions _options;

    public BrevoEmailSender(HttpClient httpClient, IOptions<EmailOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post, $"{_options.Brevo.BaseUrl.TrimEnd('/')}/v3/smtp/email");
        request.Headers.Add("api-key", _options.Brevo.ApiKey);
        request.Content = JsonContent.Create(new
        {
            sender = new { email = _options.From, name = _options.FromName },
            to = new[] { new { email = message.To, name = message.ToName } },
            subject = message.Subject,
            htmlContent = message.HtmlBody,
            textContent = message.TextBody
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detalle = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Brevo respondió {(int)response.StatusCode}: {detalle}");
        }
    }
}
