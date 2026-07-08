using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Notificaciones;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Notificaciones;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.Application.Services;

public class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _repository;
    private readonly NotificacionesOptions _options;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        INotificacionRepository repository,
        IOptions<NotificacionesOptions> options,
        ILogger<NotificacionService> logger)
    {
        _repository = repository;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublicarAsync(PublicarNotificacionDto evento)
    {
        try
        {
            var destinatarioIds = evento.DestinatarioIds
                .Where(id => evento.IncluirActor || id != evento.ActorId)
                .Distinct()
                .ToList();
            if (destinatarioIds.Count == 0)
                return;

            var destinatarios = await _repository.GetUsuariosActivosAsync(destinatarioIds);
            if (destinatarios.Count == 0)
                return;

            var definicion = NotificacionCatalog.Get(evento.Tipo);

            var actorNombre = "El sistema";
            if (evento.ActorId is { } actorId)
            {
                var actor = (await _repository.GetUsuariosActivosAsync(new[] { actorId })).FirstOrDefault();
                if (actor is not null)
                    actorNombre = $"{actor.Nombres} {actor.Apellidos}".Trim();
            }

            var titulo = evento.Titulo.Replace("{actor}", actorNombre);
            var mensaje = evento.Mensaje.Replace("{actor}", actorNombre);

            var preferencias = (await _repository.GetPreferenciasAsync(destinatarios.Select(d => d.Id), evento.Tipo))
                .ToDictionary(p => p.UsuarioId);

            var ahora = DateTime.UtcNow;
            var programadoPara = definicion.Agrupable
                ? ahora.AddMinutes(_options.VentanaAgrupacionMinutos)
                : ahora;

            var notificaciones = new List<Notificacion>();
            var correos = new List<CorreoPendiente>();

            foreach (var usuario in destinatarios)
            {
                preferencias.TryGetValue(usuario.Id, out var pref);
                var enApp = pref?.EnApp ?? definicion.EnAppPorDefecto;
                var porCorreo = definicion.CorreoObligatorio || (pref?.PorCorreo ?? definicion.CorreoPorDefecto);

                if (enApp)
                {
                    notificaciones.Add(new Notificacion
                    {
                        Id = Guid.NewGuid(),
                        UsuarioId = usuario.Id,
                        Tipo = evento.Tipo,
                        Titulo = titulo,
                        Mensaje = mensaje,
                        Url = evento.Url,
                        ActorId = evento.ActorId,
                        FechaCreacion = ahora
                    });
                }

                if (porCorreo && !string.IsNullOrWhiteSpace(usuario.Email))
                {
                    correos.Add(new CorreoPendiente
                    {
                        Id = Guid.NewGuid(),
                        UsuarioId = usuario.Id,
                        Tipo = evento.Tipo,
                        Titulo = titulo,
                        Mensaje = mensaje,
                        Url = evento.Url,
                        DedupKey = definicion.Agrupable ? evento.DedupKey : null,
                        ProgramadoPara = programadoPara,
                        FechaCreacion = ahora
                    });
                }
            }

            if (notificaciones.Count > 0)
                await _repository.AddNotificacionesAsync(notificaciones);
            if (correos.Count > 0)
                await _repository.EnqueueCorreosAsync(correos);
        }
        catch (Exception ex)
        {
            // La notificación es un efecto secundario: nunca debe tumbar la operación de negocio.
            _logger.LogError(ex, "Error publicando notificación {Tipo}", evento.Tipo);
        }
    }

    public async Task<PagedResultDto<NotificacionDto>> GetMisNotificacionesAsync(
        Guid usuarioId, int page, int pageSize, bool soloNoLeidas)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _repository.GetByUsuarioAsync(usuarioId, page, pageSize, soloNoLeidas);
        return new PagedResultDto<NotificacionDto>
        {
            Data = items.Select(ToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ResumenNotificacionesDto> GetResumenAsync(Guid usuarioId)
        => new() { NoLeidas = await _repository.CountNoLeidasAsync(usuarioId) };

    public Task<bool> MarcarLeidaAsync(Guid usuarioId, Guid notificacionId)
        => _repository.MarcarLeidaAsync(usuarioId, notificacionId);

    public Task<int> MarcarTodasLeidasAsync(Guid usuarioId)
        => _repository.MarcarTodasLeidasAsync(usuarioId);

    public async Task<IReadOnlyList<PreferenciaNotificacionDto>> GetPreferenciasAsync(Guid usuarioId)
    {
        var guardadas = (await _repository.GetPreferenciasAsync(usuarioId)).ToDictionary(p => p.Tipo);

        return NotificacionCatalog.All.Select(def =>
        {
            guardadas.TryGetValue(def.Tipo, out var pref);
            return new PreferenciaNotificacionDto
            {
                Tipo = def.Tipo,
                Grupo = def.Grupo,
                Nombre = def.Nombre,
                Descripcion = def.Descripcion,
                Agrupable = def.Agrupable,
                CorreoObligatorio = def.CorreoObligatorio,
                EnApp = pref?.EnApp ?? def.EnAppPorDefecto,
                PorCorreo = def.CorreoObligatorio || (pref?.PorCorreo ?? def.CorreoPorDefecto)
            };
        }).ToList();
    }

    public async Task ActualizarPreferenciasAsync(Guid usuarioId, UpdatePreferenciasNotificacionDto dto)
    {
        var preferencias = dto.Preferencias
            .Where(p => NotificacionCatalog.All.Any(d => d.Tipo == p.Tipo))
            .Select(p =>
            {
                var def = NotificacionCatalog.Get(p.Tipo);
                return new PreferenciaNotificacion
                {
                    UsuarioId = usuarioId,
                    Tipo = p.Tipo,
                    EnApp = p.EnApp,
                    // Los tipos de seguridad no permiten apagar el correo.
                    PorCorreo = def.CorreoObligatorio || p.PorCorreo
                };
            });

        await _repository.UpsertPreferenciasAsync(usuarioId, preferencias);
    }

    private static NotificacionDto ToDto(Notificacion n) => new()
    {
        Id = n.Id,
        Tipo = n.Tipo,
        Titulo = n.Titulo,
        Mensaje = n.Mensaje,
        Url = n.Url,
        ActorNombre = n.Actor is null ? null : $"{n.Actor.Nombres} {n.Actor.Apellidos}".Trim(),
        ActorIniciales = n.Actor?.Iniciales,
        Leida = n.Leida,
        FechaCreacion = n.FechaCreacion
    };
}
