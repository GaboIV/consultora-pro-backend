using AutoMapper;
using ConsultoraPro.Application.DTOs.Clientes;
using ConsultoraPro.Application.DTOs.Management;
using ConsultoraPro.Application.DTOs.Proyectos;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Profiles;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Cliente, ClienteDto>();
        CreateMap<CreateClienteDto, Cliente>();
        CreateMap<UpdateClienteDto, Cliente>();

        CreateMap<Proyecto, ProyectoDto>()
            .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Cliente.Nombre))
            .ForMember(dest => dest.TipoSolucionNombre, opt => opt.MapFrom(src => src.TipoSolucion.Nombre))
            .ForMember(dest => dest.Etapa, opt => opt.MapFrom(src => src.Etapa.ToString()))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()));
        CreateMap<CreateProyectoDto, Proyecto>();
        CreateMap<UpdateProyectoDto, Proyecto>();

        CreateMap<Cliente, ManagementClientDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Initials, opt => opt.MapFrom(src => src.Iniciales))
            .ForMember(dest => dest.ProjectsCount, opt => opt.MapFrom(src => src.TotalProyectos))
            .ForMember(dest => dest.Sector, opt => opt.MapFrom(src => src.Industria))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Activo ? "Activo" : "Inactivo"))
            .ForMember(dest => dest.StatusTone, opt => opt.MapFrom(src => src.Activo ? "amber" : "gray"))
            .ForMember(dest => dest.LogoTone, opt => opt.MapFrom(src => MapColorClass(src.ColorClass)));

        CreateMap<Proyecto, ManagementProjectDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Cliente.Nombre))
            .ForMember(dest => dest.Stage, opt => opt.MapFrom(src => MapStageLabel(src.Etapa)))
            .ForMember(dest => dest.StageTone, opt => opt.MapFrom(src => MapStageTone(src.Etapa)))
            .ForMember(dest => dest.Lead, opt => opt.MapFrom(src => new LeadDto
            {
                Initials = src.TechLeadIniciales,
                Name = src.TechLead,
                Tone = MapProgressTone(src.Progreso)
            }))
            .ForMember(dest => dest.ProgressTone, opt => opt.MapFrom(src => MapProgressTone(src.Progreso)))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.FechaInicio.ToString("dd MMM yyyy")))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.FechaFin.ToString("dd MMM yyyy")))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatusLabel(src.Estado)))
            .ForMember(dest => dest.StatusTone, opt => opt.MapFrom(src => MapStatusTone(src.Estado)))
            .ForMember(dest => dest.TeamSize, opt => opt.MapFrom(src => src.TotalMiembros));
    }

    private static string MapColorClass(string colorClass) => colorClass switch
    {
        "blue" or "purple" or "green" or "amber" or "red" => colorClass,
        _ => "blue"
    };

    private static string MapStageLabel(EtapaProyecto etapa) => etapa switch
    {
        EtapaProyecto.Analisis => "Análisis",
        EtapaProyecto.Diseno => "Diseño",
        EtapaProyecto.Desarrollo => "Desarrollo",
        EtapaProyecto.QA => "QA",
        EtapaProyecto.Deploy => "Deploy",
        EtapaProyecto.Soporte => "Soporte",
        _ => etapa.ToString()
    };

    private static string MapStageTone(EtapaProyecto etapa) => etapa switch
    {
        EtapaProyecto.Analisis => "purple",
        EtapaProyecto.Diseno => "purple",
        EtapaProyecto.Desarrollo => "blue",
        EtapaProyecto.QA => "amber",
        EtapaProyecto.Deploy => "teal",
        EtapaProyecto.Soporte => "gray",
        _ => "blue"
    };

    private static string MapProgressTone(int progreso) => progreso switch
    {
        >= 80 => "green",
        >= 40 => "blue",
        >= 20 => "amber",
        _ => "red"
    };

    private static string MapStatusLabel(EstadoProyecto estado) => estado switch
    {
        EstadoProyecto.Planificacion => "Planificación",
        EstadoProyecto.EnCurso => "En curso",
        EstadoProyecto.Completado => "Completado",
        EstadoProyecto.PorVencer => "Por vencer",
        _ => estado.ToString()
    };

    private static string MapStatusTone(EstadoProyecto estado) => estado switch
    {
        EstadoProyecto.Planificacion => "blue",
        EstadoProyecto.EnCurso => "amber",
        EstadoProyecto.Completado => "green",
        EstadoProyecto.PorVencer => "red",
        _ => "gray"
    };
}
