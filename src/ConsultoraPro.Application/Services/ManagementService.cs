using AutoMapper;
using ConsultoraPro.Application.DTOs.Management;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Application.Services;

public class ManagementService : IManagementService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly ITipoSolucionRepository _tipoSolucionRepository;
    private readonly ICredencialRepository _credencialRepository;
    private readonly IAmbienteRepository _ambienteRepository;
    private readonly IDespliegueRepository _despliegueRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public ManagementService(
        IClienteRepository clienteRepository,
        IProyectoRepository proyectoRepository,
        ITipoSolucionRepository tipoSolucionRepository,
        ICredencialRepository credencialRepository,
        IAmbienteRepository ambienteRepository,
        IDespliegueRepository despliegueRepository,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _clienteRepository = clienteRepository;
        _proyectoRepository = proyectoRepository;
        _tipoSolucionRepository = tipoSolucionRepository;
        _credencialRepository = credencialRepository;
        _ambienteRepository = ambienteRepository;
        _despliegueRepository = despliegueRepository;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<ManagementSnapshotDto> GetSnapshotAsync()
    {
        var clientes = await _clienteRepository.GetAllAsync();
        var proyectos = await _proyectoRepository.GetAllAsync();
        var tiposSolucion = await _tipoSolucionRepository.GetAllAsync();
        var users = await _userManager.Users.ToListAsync();
        var credencialesPorVencer = (await _credencialRepository.GetExpiringWithinAsync(7)).ToList();
        var ambientes = (await _ambienteRepository.GetAllAsync()).ToList();
        var despliegues = (await _despliegueRepository.GetRecentAsync(5)).ToList();
        var (totalDesplieguesMes, exitososDesplieguesMes) = await _despliegueRepository.GetMonthlyStatsAsync();

        var clients = _mapper.Map<List<ManagementClientDto>>(clientes);
        var projects = _mapper.Map<List<ManagementProjectDto>>(proyectos);
        var userDtos = _mapper.Map<List<UsuarioSnapshotDto>>(users);
        var tiposSolucionDtos = tiposSolucion.Select(t => new TipoSolucionDto
        {
            Id = t.Id.ToString(),
            Nombre = t.Nombre
        }).ToList();

        var now = DateTime.UtcNow;
        var totalProyectos = projects.Count;
        var activos = clients.Count;
        var completados = projects.Count(p => p.StatusTone == "green");
        var ambientesOnline = ambientes.Count(a => a.Estado == EstadoAmbiente.Online);
        var ambientesAlerta = ambientes.Count(a => a.Estado == EstadoAmbiente.Alerta);
        var ambientesOffline = ambientes.Count(a => a.Estado == EstadoAmbiente.Offline);
        var ambientesConfigurando = ambientes.Count(a => a.Estado == EstadoAmbiente.Configurando);

        var alerts = new List<AlertMessageDto>
        {
            new() { Tone = "info", Text = $"Última actualización: {now:dd/MM/yyyy HH:mm} UTC" }
        };

        if (credencialesPorVencer.Count > 0)
        {
            alerts.Add(new AlertMessageDto
            {
                Tone = "warn",
                Text = $"{credencialesPorVencer.Count} credencial(es) vencen en menos de 7 días"
            });
        }

        if (ambientesAlerta > 0)
        {
            alerts.Add(new AlertMessageDto
            {
                Tone = "warn",
                Text = $"{ambientesAlerta} ambiente(s) requieren atención operativa"
            });
        }

        var snapshot = new ManagementSnapshotDto
        {
            GeneratedAt = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            PeriodLabel = $"{now:MMMM yyyy}",
            Executive = new ExecutiveOverviewDto
            {
                Metrics = new List<MetricDto>
                {
                    new() { Label = "Clientes activos", Value = activos.ToString(), Detail = $"Gestionando {totalProyectos} proyectos", Tone = "blue" },
                    new() { Label = "Proyectos en curso", Value = projects.Count(p => p.StatusTone is "amber" or "blue").ToString(), Detail = completados > 0 ? $"{completados} completados" : "0 completados", Tone = "green" },
                    new() { Label = "Ambientes activos", Value = ambientes.Count.ToString(), Detail = $"{ambientesOnline} online · {ambientesAlerta} alerta · {ambientesOffline} offline", Tone = ambientesAlerta > 0 ? "amber" : "teal", DetailTone = ambientesAlerta > 0 ? "warn" : "up" },
                    new() { Label = "Progreso promedio", Value = projects.Any() ? $"{(int)projects.Average(p => p.Progress)}%" : "0%", Detail = "General de todos los proyectos", Tone = "amber" },
                    new()
                    {
                        Label = "Despliegues del mes",
                        Value = totalDesplieguesMes.ToString(),
                        Detail = totalDesplieguesMes > 0 ? $"{(exitososDesplieguesMes * 100 / totalDesplieguesMes)}% tasa de éxito" : "Sin despliegues este mes",
                        Tone = totalDesplieguesMes > 0 && exitososDesplieguesMes * 100 / totalDesplieguesMes >= 90 ? "green" : totalDesplieguesMes > 0 ? "amber" : "gray"
                    }
                },
                Alerts = alerts,
                SpotlightProjects = projects.OrderByDescending(p => p.Progress).Take(3).ToList(),
                Gantt = new List<GanttItemDto>(),
                Milestones = new List<AlertMessageDto>
                {
                    new() { Tone = "info", Text = $"{totalProyectos} proyectos registrados en el sistema" }
                }
            },
            Clients = clients,
            Projects = projects,
            TiposSolucion = tiposSolucionDtos,
            Usuarios = userDtos,
            Infrastructure = new InfrastructureOverviewDto
            {
                EnvironmentSummary = new EnvironmentSummaryDto
                {
                    Total = ambientes.Count,
                    Online = ambientesOnline,
                    Alertas = ambientesAlerta,
                    Offline = ambientesOffline,
                    Configurando = ambientesConfigurando
                },
                EnvironmentGroups = ambientes
                    .GroupBy(a => new
                    {
                        a.ProyectoId,
                        ProjectName = $"{a.Proyecto?.Cliente?.Nombre ?? "Cliente"} · {a.Proyecto?.Nombre ?? "Proyecto"}"
                    })
                    .Select(group => new EnvironmentGroupDto
                    {
                        ProjectId = group.Key.ProyectoId.ToString(),
                        ProjectName = group.Key.ProjectName,
                        Items = group.Select(ToEnvironmentItemDto).ToList()
                    })
                    .ToList(),
                Credentials = credencialesPorVencer.Select(ToCredentialAlertDto).ToList(),
                Deployments = despliegues.Select(ToDeploymentDto).ToList()
            },
            Team = new TeamOverviewDto()
        };

        return snapshot;
    }

    private static EnvironmentItemDto ToEnvironmentItemDto(Ambiente ambiente)
    {
        return new EnvironmentItemDto
        {
            Id = ambiente.Id.ToString(),
            ProjectId = ambiente.ProyectoId.ToString(),
            Name = ambiente.Nombre,
            Type = MapEnvironmentTypeLabel(ambiente.Tipo),
            Url = ambiente.Url,
            Stack = ambiente.Tecnologia,
            State = MapEnvironmentStateLabel(ambiente.Estado),
            StateTone = MapEnvironmentStateTone(ambiente.Estado),
            Availability = $"{ambiente.UptimePorcentaje:0.##}%"
        };
    }

    private static CredentialDto ToCredentialAlertDto(Credencial credencial)
    {
        var days = (int)Math.Ceiling((credencial.FechaVencimiento.Date - DateTime.UtcNow.Date).TotalDays);
        return new CredentialDto
        {
            Service = credencial.Nombre,
            Environment = credencial.Ambiente?.Nombre ?? credencial.Proyecto?.Nombre ?? "Proyecto no disponible",
            EnvironmentTone = credencial.Ambiente is null ? "blue" : MapEnvironmentTypeTone(credencial.Ambiente.Tipo),
            Kind = credencial.Tipo.ToString(),
            ExpiresIn = days switch
            {
                < 0 => $"Vencida hace {Math.Abs(days)} día(s)",
                0 => "Vence hoy",
                1 => "Vence mañana",
                _ => $"Vence en {days} días"
            },
            Tone = days < 7 ? "red" : "amber"
        };
    }

    private static string MapEnvironmentTypeLabel(TipoAmbiente tipo) => tipo switch
    {
        TipoAmbiente.Produccion => "Producción",
        TipoAmbiente.Staging => "Staging",
        TipoAmbiente.Desarrollo => "Desarrollo",
        TipoAmbiente.QA => "QA",
        _ => tipo.ToString()
    };

    private static string MapEnvironmentStateLabel(EstadoAmbiente estado) => estado switch
    {
        EstadoAmbiente.Online => "Online",
        EstadoAmbiente.Offline => "Offline",
        EstadoAmbiente.Alerta => "Alerta",
        EstadoAmbiente.Configurando => "Configurando",
        _ => estado.ToString()
    };

    private static string MapEnvironmentStateTone(EstadoAmbiente estado) => estado switch
    {
        EstadoAmbiente.Online => "green",
        EstadoAmbiente.Alerta => "amber",
        EstadoAmbiente.Offline => "red",
        EstadoAmbiente.Configurando => "blue",
        _ => "gray"
    };

    private static DeploymentDto ToDeploymentDto(Despliegue d)
    {
        var actorName = d.EjecutadoPor is null ? string.Empty : $"{d.EjecutadoPor.Nombres} {d.EjecutadoPor.Apellidos}".Trim();
        var duration = d.DuracionSegundos switch
        {
            < 60 => $"{d.DuracionSegundos}s",
            < 3600 => $"{d.DuracionSegundos / 60}m {d.DuracionSegundos % 60}s",
            _ => $"{d.DuracionSegundos / 3600}h {(d.DuracionSegundos % 3600) / 60}m"
        };

        return new DeploymentDto
        {
            ProjectName = d.Proyecto?.Nombre ?? string.Empty,
            Target = d.Ambiente?.Nombre ?? string.Empty,
            When = d.FechaHora.ToString("dd/MM/yyyy HH:mm"),
            Actor = actorName,
            Duration = duration,
            Version = d.Version,
            Status = MapDeploymentStatusLabel(d.Estado),
            Tone = MapDeploymentStatusTone(d.Estado)
        };
    }

    private static string MapDeploymentStatusLabel(EstadoDespliegue estado) => estado switch
    {
        EstadoDespliegue.Exitoso => "Exitoso",
        EstadoDespliegue.Fallido => "Fallido",
        EstadoDespliegue.EnCurso => "En curso",
        EstadoDespliegue.Cancelado => "Cancelado",
        _ => estado.ToString()
    };

    private static string MapDeploymentStatusTone(EstadoDespliegue estado) => estado switch
    {
        EstadoDespliegue.Exitoso => "green",
        EstadoDespliegue.Fallido => "red",
        EstadoDespliegue.EnCurso => "amber",
        EstadoDespliegue.Cancelado => "gray",
        _ => "gray"
    };

    private static string MapEnvironmentTypeTone(TipoAmbiente tipo) => tipo switch
    {
        TipoAmbiente.Produccion => "red",
        TipoAmbiente.Staging => "amber",
        TipoAmbiente.Desarrollo => "blue",
        TipoAmbiente.QA => "purple",
        _ => "gray"
    };
}
