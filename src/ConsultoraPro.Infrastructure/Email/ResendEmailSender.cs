using System.Net.Http.Headers;
using System.Net.Http.Json;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.Infrastructure.Email;

/// <summary>Envío transaccional vía API de Resend (https://resend.com/docs/api-reference/emails/send-email).</summary>
public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailOptions _options;

    public ResendEmailSender(HttpClient httpClient, IOptions<EmailOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post, $"{_options.Resend.BaseUrl.TrimEnd('/')}/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Resend.ApiKey);
        request.Content = JsonContent.Create(new
        {
            from = $"{_options.FromName} <{_options.From}>",
            to = new[] { message.To },
            subject = message.Subject,
            html = message.HtmlBody,
            text = message.TextBody
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detalle = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Resend respondió {(int)response.StatusCode}: {detalle}");
        }
    }
}
