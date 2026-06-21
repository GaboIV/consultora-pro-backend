using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsultoraPro.Application.DTOs.Alertas;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class AlertaService : IAlertaService
{
    private readonly ICredencialRepository _credencialRepository;
    private readonly ISolicitudRevelacionRepository _solicitudRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IAmbienteRepository _ambienteRepository;
    private readonly ICurrentUserService _currentUserService;

    public AlertaService(
        ICredencialRepository credencialRepository,
        ISolicitudRevelacionRepository solicitudRepository,
        IProyectoRepository proyectoRepository,
        IAmbienteRepository ambienteRepository,
        ICurrentUserService currentUserService)
    {
        _credencialRepository = credencialRepository;
        _solicitudRepository = solicitudRepository;
        _proyectoRepository = proyectoRepository;
        _ambienteRepository = ambienteRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<AlertaDto>> GetAlertasActivasAsync()
    {
        var alerts = new List<AlertaDto>();
        var now = DateTime.UtcNow;

        // 1. Credenciales que vencen en menos de 7 días
        var credencialesPorVencer = await _credencialRepository.GetExpiringWithinAsync(7);
        foreach (var credencial in credencialesPorVencer)
        {
            var dias = (int)Math.Ceiling((credencial.FechaVencimiento.Date - now.Date).TotalDays);
            var esCritica = dias < 3;
            var tone = esCritica ? "red" : "amber";
            var environmentName = credencial.Ambiente?.Nombre ?? credencial.Proyecto?.Nombre ?? "General";

            string msg = dias switch
            {
                < 0 => $"La credencial '{credencial.Nombre}' ({environmentName}) está vencida hace {Math.Abs(dias)} día(s).",
                0 => $"La credencial '{credencial.Nombre}' ({environmentName}) vence hoy.",
                1 => $"La credencial '{credencial.Nombre}' ({environmentName}) vence mañana.",
                _ => $"La credencial '{credencial.Nombre}' ({environmentName}) vence en {dias} días."
            };

            alerts.Add(new AlertaDto
            {
                Id = $"credencial-vence-{credencial.Id}",
                Tipo = "CredencialVencimiento",
                Mensaje = msg,
                Tone = tone,
                EsCritica = esCritica,
                ReferenciaId = credencial.Id.ToString(),
                FechaReferencia = credencial.FechaVencimiento
            });
        }

        // 2. Proyectos cuya fecha fin es en menos de 14 días y no están completados
        var proyectos = await _proyectoRepository.GetAllAsync();
        var proyectosPorVencer = proyectos
            .Where(p => p.Estado != EstadoProyecto.Completado && p.FechaFin <= now.AddDays(14));

        foreach (var proyecto in proyectosPorVencer)
        {
            var dias = (int)Math.Ceiling((proyecto.FechaFin.Date - now.Date).TotalDays);
            var esCritica = dias < 0; // retrasado es crítico
            var tone = esCritica ? "red" : "amber";

            string msg = dias switch
            {
                < 0 => $"El proyecto '{proyecto.Nombre}' está retrasado por {Math.Abs(dias)} día(s).",
                0 => $"El proyecto '{proyecto.Nombre}' vence hoy.",
                1 => $"El proyecto '{proyecto.Nombre}' vence mañana.",
                _ => $"El proyecto '{proyecto.Nombre}' vence en {dias} días."
            };

            alerts.Add(new AlertaDto
            {
                Id = $"proyecto-vence-{proyecto.Id}",
                Tipo = "ProyectoVencimiento",
                Mensaje = msg,
                Tone = tone,
                EsCritica = esCritica,
                ReferenciaId = proyecto.Id.ToString(),
                FechaReferencia = proyecto.FechaFin
            });
        }

        // 3. Ambientes en estado Alerta
        var ambientes = await _ambienteRepository.GetAllAsync();
        var ambientesEnAlerta = ambientes.Where(a => a.Estado == EstadoAmbiente.Alerta);

        foreach (var ambiente in ambientesEnAlerta)
        {
            var projectName = ambiente.Proyecto?.Nombre ?? "General";
            alerts.Add(new AlertaDto
            {
                Id = $"ambiente-alerta-{ambiente.Id}",
                Tipo = "AmbienteAlerta",
                Mensaje = $"El ambiente '{ambiente.Nombre}' del proyecto '{projectName}' requiere atención operativa.",
                Tone = "red",
                EsCritica = true,
                ReferenciaId = ambiente.Id.ToString(),
                FechaReferencia = null // No reference date
            });
        }

        // 4. Solicitudes de revelación de credenciales (específicas del usuario actual).
        var userId = _currentUserService.UserId;
        if (userId.HasValue)
        {
            // 4a. Pendientes por resolver, visibles solo para quien puede aprobar.
            if (_currentUserService.HasPermission("credenciales.solicitud.aprobar"))
            {
                var pendientes = await _solicitudRepository.ListAsync(EstadoSolicitudRevelacion.Pendiente);
                foreach (var solicitud in pendientes)
                {
                    alerts.Add(new AlertaDto
                    {
                        Id = $"solicitud-revelacion-{solicitud.Id}",
                        Tipo = "SolicitudRevelacion",
                        Mensaje = $"{FullName(solicitud.Solicitante)} solicita revelar la credencial '{solicitud.Credencial?.Nombre}'.",
                        Tone = "amber",
                        EsCritica = false,
                        ReferenciaId = solicitud.Id.ToString(),
                        FechaReferencia = solicitud.FechaSolicitud
                    });
                }
            }

            // 4b. Resoluciones de mis propias solicitudes (aprobación vigente o rechazo reciente).
            var mias = await _solicitudRepository.ListBySolicitanteAsync(userId.Value);
            foreach (var solicitud in mias)
            {
                if (solicitud.EsVigente(now))
                {
                    alerts.Add(new AlertaDto
                    {
                        Id = $"solicitud-aprobada-{solicitud.Id}",
                        Tipo = "SolicitudRevelacionAprobada",
                        Mensaje = $"Tu solicitud para '{solicitud.Credencial?.Nombre}' fue aprobada. Puedes revelarla hasta las {solicitud.VigenteHasta:HH:mm} UTC.",
                        Tone = "info",
                        EsCritica = false,
                        ReferenciaId = solicitud.CredencialId.ToString(),
                        FechaReferencia = solicitud.VigenteHasta
                    });
                }
                else if (solicitud.Estado == EstadoSolicitudRevelacion.Rechazada
                         && solicitud.FechaResolucion >= now.AddDays(-1))
                {
                    alerts.Add(new AlertaDto
                    {
                        Id = $"solicitud-rechazada-{solicitud.Id}",
                        Tipo = "SolicitudRevelacionRechazada",
                        Mensaje = $"Tu solicitud para '{solicitud.Credencial?.Nombre}' fue rechazada.",
                        Tone = "warn",
                        EsCritica = false,
                        ReferenciaId = solicitud.CredencialId.ToString(),
                        FechaReferencia = solicitud.FechaResolucion
                    });
                }
            }
        }

        return alerts.OrderByDescending(a => a.EsCritica).ThenBy(a => a.FechaReferencia).ToList();
    }

    private static string FullName(ApplicationUser? user) =>
        user is null ? string.Empty : $"{user.Nombres} {user.Apellidos}".Trim();
}
