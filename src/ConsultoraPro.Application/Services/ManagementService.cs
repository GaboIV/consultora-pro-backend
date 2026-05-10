using AutoMapper;
using ConsultoraPro.Application.DTOs.Management;
using ConsultoraPro.Application.Interfaces;
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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public ManagementService(
        IClienteRepository clienteRepository,
        IProyectoRepository proyectoRepository,
        ITipoSolucionRepository tipoSolucionRepository,
        ICredencialRepository credencialRepository,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _clienteRepository = clienteRepository;
        _proyectoRepository = proyectoRepository;
        _tipoSolucionRepository = tipoSolucionRepository;
        _credencialRepository = credencialRepository;
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
                    new() { Label = "Proyectos totales", Value = totalProyectos.ToString(), Detail = $"Distribuidos en {activos} clientes", Tone = "purple" },
                    new() { Label = "Progreso promedio", Value = projects.Any() ? $"{(int)projects.Average(p => p.Progress)}%" : "0%", Detail = "General de todos los proyectos", Tone = "amber" }
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
                Credentials = credencialesPorVencer.Select(ToCredentialAlertDto).ToList()
            },
            Team = new TeamOverviewDto()
        };

        return snapshot;
    }

    private static CredentialDto ToCredentialAlertDto(Credencial credencial)
    {
        var days = (int)Math.Ceiling((credencial.FechaVencimiento.Date - DateTime.UtcNow.Date).TotalDays);
        return new CredentialDto
        {
            Service = credencial.Nombre,
            Environment = credencial.Proyecto?.Nombre ?? "Proyecto no disponible",
            EnvironmentTone = "blue",
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
}
