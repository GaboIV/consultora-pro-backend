using AutoMapper;
using ConsultoraPro.Application.DTOs.Management;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;

namespace ConsultoraPro.Application.Services;

public class ManagementService : IManagementService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IMapper _mapper;

    public ManagementService(
        IClienteRepository clienteRepository,
        IProyectoRepository proyectoRepository,
        IMapper mapper)
    {
        _clienteRepository = clienteRepository;
        _proyectoRepository = proyectoRepository;
        _mapper = mapper;
    }

    public async Task<ManagementSnapshotDto> GetSnapshotAsync()
    {
        var clientes = await _clienteRepository.GetAllAsync();
        var proyectos = await _proyectoRepository.GetAllAsync();

        var clients = _mapper.Map<List<ManagementClientDto>>(clientes);
        var projects = _mapper.Map<List<ManagementProjectDto>>(proyectos);

        var now = DateTime.UtcNow;
        var totalProyectos = projects.Count;
        var activos = clients.Count;
        var completados = projects.Count(p => p.StatusTone == "green");

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
                Alerts = new List<AlertMessageDto>
                {
                    new() { Tone = "info", Text = $"Última actualización: {now:dd/MM/yyyy HH:mm} UTC" }
                },
                SpotlightProjects = projects.OrderByDescending(p => p.Progress).Take(3).ToList(),
                Gantt = new List<GanttItemDto>(),
                Milestones = new List<AlertMessageDto>
                {
                    new() { Tone = "info", Text = $"{totalProyectos} proyectos registrados en el sistema" }
                }
            },
            Clients = clients,
            Projects = projects,
            Infrastructure = new InfrastructureOverviewDto(),
            Team = new TeamOverviewDto()
        };

        return snapshot;
    }
}
