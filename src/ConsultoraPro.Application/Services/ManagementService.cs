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
    private readonly IRepositorioRepository _repositorioRepository;
    private readonly IAlertaService _alertaService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ManagementService(
        IClienteRepository clienteRepository,
        IProyectoRepository proyectoRepository,
        ITipoSolucionRepository tipoSolucionRepository,
        ICredencialRepository credencialRepository,
        IAmbienteRepository ambienteRepository,
        IDespliegueRepository despliegueRepository,
        IRepositorioRepository repositorioRepository,
        IAlertaService alertaService,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _clienteRepository = clienteRepository;
        _proyectoRepository = proyectoRepository;
        _tipoSolucionRepository = tipoSolucionRepository;
        _credencialRepository = credencialRepository;
        _ambienteRepository = ambienteRepository;
        _despliegueRepository = despliegueRepository;
        _repositorioRepository = repositorioRepository;
        _alertaService = alertaService;
        _userManager = userManager;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ManagementSnapshotDto> GetSnapshotAsync(string? period = null)
    {
        var now = DateTime.UtcNow;
        var selectedDate = now;
        if (!string.IsNullOrEmpty(period) && DateTime.TryParseExact(period, "yyyy-MM", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedDate))
        {
            selectedDate = new DateTime(parsedDate.Year, parsedDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        // Los roles sin acceso total a proyectos (p. ej. Soporte, Dev) solo ven los datos de los
        // proyectos donde están asignados como miembros. Se usa HasFullProjectAccess (claim del JWT)
        // en lugar de IsInRole para mantener consistencia con ProyectoService.
        Guid? memberUserId = null;
        var restringidoAProyectos = !_currentUserService.HasFullProjectAccessFor("proyectos");
        if (restringidoAProyectos)
        {
            memberUserId = _currentUserService.UserId;
        }

        var clientes = await _clienteRepository.GetAllAsync(memberUserId);
        var proyectos = await _proyectoRepository.GetAllAsync(memberUserId);
        var tiposSolucion = await _tipoSolucionRepository.GetAllAsync();
        var users = await _userManager.Users.ToListAsync();
        
        var credencialesPorVencer = !restringidoAProyectos
            ? (await _credencialRepository.GetExpiringWithinAsync(7)).ToList()
            : new List<Credencial>();

        var ambientes = (await _ambienteRepository.GetAllAsync(null, memberUserId)).ToList();

        var repositorios = !restringidoAProyectos
            ? (await _repositorioRepository.GetAllAsync()).ToList()
            : new List<Repositorio>();

        var despliegues = !restringidoAProyectos
            ? (await _despliegueRepository.GetRecentAsync(5, selectedDate)).ToList()
            : (await _despliegueRepository.GetRecentAsync(5, selectedDate)).Where(d => proyectos.Any(p => p.Id == d.ProyectoId)).ToList();

        var (totalDesplieguesMes, exitososDesplieguesMes) = !restringidoAProyectos
            ? await _despliegueRepository.GetMonthlyStatsAsync(selectedDate)
            : (0, 0);

        var clients = _mapper.Map<List<ManagementClientDto>>(clientes);
        var projects = _mapper.Map<List<ManagementProjectDto>>(proyectos);
        var userDtos = _mapper.Map<List<UsuarioSnapshotDto>>(users);
        var tiposSolucionDtos = tiposSolucion.Select(t => new TipoSolucionDto
        {
            Id = t.Id.ToString(),
            Nombre = t.Nombre
        }).ToList();

        var totalProyectos = projects.Count;
        var activos = clientes.Count(c => c.Activo);
        var completados = projects.Count(p => p.StatusValue == "Completado");
        var proyectosEnCurso = projects.Count(p => p.StatusValue != "Completado");
        var ambientesOnline = ambientes.Count(a => a.Estado == EstadoAmbiente.Online);
        var ambientesAlerta = ambientes.Count(a => a.Estado == EstadoAmbiente.Alerta);
        var ambientesOffline = ambientes.Count(a => a.Estado == EstadoAmbiente.Offline);
        var ambientesConfigurando = ambientes.Count(a => a.Estado == EstadoAmbiente.Configurando);

        var activeAlerts = await _alertaService.GetAlertasActivasAsync();
        var alerts = new List<AlertMessageDto>
        {
            new() { Tone = "info", Text = $"Última actualización: {now:dd/MM/yyyy HH:mm} UTC" }
        };

        foreach (var alert in activeAlerts)
        {
            alerts.Add(new AlertMessageDto
            {
                Tone = alert.EsCritica || alert.Tone == "red" || alert.Tone == "amber" ? "warn" : "info",
                Text = alert.Mensaje
            });
        }

        var spotlightProyectos = proyectos
            .Where(p => p.Estado != EstadoProyecto.Completado)
            .OrderBy(p => p.FechaFin)
            .Take(5)
            .ToList();

        if (spotlightProyectos.Count < 3)
        {
            var extra = proyectos
                .Where(p => p.Estado == EstadoProyecto.Completado)
                .OrderByDescending(p => p.FechaFin)
                .Take(5 - spotlightProyectos.Count);
            spotlightProyectos.AddRange(extra);
        }

        var spotlightDtos = _mapper.Map<List<ManagementProjectDto>>(spotlightProyectos);

        var snapshot = new ManagementSnapshotDto
        {
            GeneratedAt = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            PeriodLabel = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(
                selectedDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"))
            ),
            Executive = new ExecutiveOverviewDto
            {
                Metrics = new List<MetricDto>
                {
                    new() { Label = "Clientes activos", Value = activos.ToString(), Detail = $"Gestionando {totalProyectos} proyectos", Tone = "blue" },
                    new() { Label = "Proyectos en curso", Value = proyectosEnCurso.ToString(), Detail = completados > 0 ? $"{completados} completados" : "0 completados", Tone = "green" },
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
                SpotlightProjects = spotlightDtos,
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
                Repositories = repositorios.Select(ToRepositoryHealthDto).ToList(),
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
            StateTone = MapEnvironmentStateTone(ambiente.Estado)
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
        TipoAmbiente.Desarrollo => "Desarrollo",
        TipoAmbiente.Calidad => "Calidad",
        TipoAmbiente.Produccion => "Producción",
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
        TipoAmbiente.Desarrollo => "blue",
        TipoAmbiente.Calidad => "purple",
        TipoAmbiente.Produccion => "red",
        _ => "gray"
    };

    private static RepositoryHealthDto ToRepositoryHealthDto(Repositorio r)
    {
        return new RepositoryHealthDto
        {
            Name = r.Nombre,
            Provider = MapProviderLabel(r.Proveedor),
            Branch = r.RamaPrincipal,
            Stack = MapStackFromRepository(r),
            Status = r.EstadoPipeline.ToString(),
            Tone = MapPipelineStatusTone(r.EstadoPipeline)
        };
    }

    private static string MapProviderLabel(ProveedorRepositorio proveedor) => proveedor switch
    {
        ProveedorRepositorio.GitHub => "GitHub",
        ProveedorRepositorio.GitLab => "GitLab",
        ProveedorRepositorio.AzureDevOps => "Azure DevOps",
        ProveedorRepositorio.Bitbucket => "Bitbucket",
        _ => proveedor.ToString()
    };

    private static string MapPipelineStatusTone(EstadoPipeline estado) => estado switch
    {
        EstadoPipeline.Passing => "green",
        EstadoPipeline.Failed => "red",
        EstadoPipeline.EnEjecucion => "amber",
        _ => "gray"
    };

    private static string MapStackFromRepository(Repositorio r)
    {
        if (r.Nombre.Contains("-worker"))
            return ".NET 8 (Worker)";
        if (r.Nombre.Contains("backend") || r.Nombre.Contains("api"))
            return ".NET 8";
        if (r.Nombre.Contains("frontend") || r.Nombre.Contains("web"))
            return "Angular 21";

        if (r.Proyecto?.TipoSolucion?.Nombre == "Host2Host")
            return ".NET 8";

        return ".NET 8 · Angular";
    }
}
