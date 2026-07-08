using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Application.Notificaciones;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.Infrastructure.Notificaciones;

/// <summary>
/// Procesa el outbox de correos: toma las filas Pendiente ya vencidas y las envía.
/// Si un usuario acumuló varias dentro de la ventana de agrupación, se envía un único
/// correo resumen. Los errores se reintentan con backoff exponencial.
/// </summary>
public class CorreoPendienteProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificacionesOptions _notifOptions;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<CorreoPendienteProcessor> _logger;

    public CorreoPendienteProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<NotificacionesOptions> notifOptions,
        IOptions<EmailOptions> emailOptions,
        ILogger<CorreoPendienteProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _notifOptions = notifOptions.Value;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Procesador de correos iniciado (proveedor: {Provider}, intervalo: {Intervalo}s, ventana: {Ventana}min)",
            _emailOptions.Provider, _notifOptions.IntervaloProcesadorSegundos, _notifOptions.VentanaAgrupacionMinutos);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(5, _notifOptions.IntervaloProcesadorSegundos)));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarLoteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo del procesador de correos");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcesarLoteAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificacionRepository>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var correos = await repository.GetCorreosParaEnviarAsync(
            DateTime.UtcNow, _notifOptions.MaxIntentos, _notifOptions.CorreosPorLote);
        if (correos.Count == 0)
            return;

        foreach (var grupo in correos.GroupBy(c => c.UsuarioId))
        {
            var lote = grupo.ToList();
            var usuario = lote[0].Usuario;

            if (usuario is null || !usuario.Activo || string.IsNullOrWhiteSpace(usuario.Email))
            {
                foreach (var correo in lote)
                    correo.Estado = EstadoCorreo.Cancelado;
                continue;
            }

            var mensaje = lote.Count == 1
                ? EmailTemplateBuilder.BuildIndividual(lote[0], usuario, _emailOptions.FrontendBaseUrl)
                : EmailTemplateBuilder.BuildResumen(lote, usuario, _emailOptions.FrontendBaseUrl);

            try
            {
                await emailSender.SendAsync(mensaje, cancellationToken);
                var ahora = DateTime.UtcNow;
                foreach (var correo in lote)
                {
                    correo.Estado = EstadoCorreo.Enviado;
                    correo.FechaEnvio = ahora;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo enviando correo a {Email} ({Cantidad} items)", usuario.Email, lote.Count);
                foreach (var correo in lote)
                    MarcarError(correo, ex.Message);
            }
        }

        await repository.UpdateCorreosAsync(correos);
    }

    private void MarcarError(CorreoPendiente correo, string error)
    {
        correo.Intentos++;
        correo.UltimoError = error.Length > 950 ? error[..950] : error;
        if (correo.Intentos >= _notifOptions.MaxIntentos)
        {
            correo.Estado = EstadoCorreo.Error;
        }
        else
        {
            // Backoff exponencial: 2, 4, 8, 16... minutos.
            correo.ProgramadoPara = DateTime.UtcNow.AddMinutes(Math.Pow(2, correo.Intentos));
        }
    }
}
