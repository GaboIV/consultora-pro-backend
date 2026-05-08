using AutoMapper;
using ConsultoraPro.Application.DTOs.Clientes;
using ConsultoraPro.Application.DTOs.Proyectos;
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
            .ForMember(dest => dest.Etapa, opt => opt.MapFrom(src => src.Etapa.ToString()))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()));
        CreateMap<CreateProyectoDto, Proyecto>();
        CreateMap<UpdateProyectoDto, Proyecto>();
    }
}
