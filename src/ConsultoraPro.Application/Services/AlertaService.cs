using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsultoraPro.Application.DTOs.Alertas;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;

namespace ConsultoraPro.Application.Services;

public class AlertaService : IAlertaService
{
    private readonly ICredencialRepository _credencialRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IAmbienteRepository _ambienteRepository;

    public AlertaService(
        ICredencialRepository credencialRepository,
        IProyectoRepository proyectoRepository,
        IAmbienteRepository ambienteRepository)
    {
        _credencialRepository = credencialRepository;
        _proyectoRepository = proyectoRepository;
        _ambienteRepository = ambienteRepository;
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

        return alerts.OrderByDescending(a => a.EsCritica).ThenBy(a => a.FechaReferencia).ToList();
    }
}
