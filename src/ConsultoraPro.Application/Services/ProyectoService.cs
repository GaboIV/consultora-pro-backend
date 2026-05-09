using AutoMapper;
using ConsultoraPro.Application.DTOs.Proyectos;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class ProyectoService : IProyectoService
{
    private readonly IProyectoRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ITipoSolucionRepository _tipoSolucionRepository;
    private readonly IMapper _mapper;

    public ProyectoService(IProyectoRepository repository, IClienteRepository clienteRepository, ITipoSolucionRepository tipoSolucionRepository, IMapper mapper)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _tipoSolucionRepository = tipoSolucionRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProyectoDto>> GetAllAsync()
    {
        var proyectos = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProyectoDto>>(proyectos);
    }

    public async Task<ProyectoDto?> GetByIdAsync(Guid id)
    {
        var proyecto = await _repository.GetByIdAsync(id);
        return proyecto == null ? null : _mapper.Map<ProyectoDto>(proyecto);
    }

    public async Task<IEnumerable<ProyectoDto>> GetByClienteIdAsync(Guid clienteId)
    {
        var proyectos = await _repository.GetByClienteIdAsync(clienteId);
        return _mapper.Map<IEnumerable<ProyectoDto>>(proyectos);
    }

    public async Task<ProyectoDto> CreateAsync(CreateProyectoDto dto)
    {
        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {dto.ClienteId} no encontrado");

        var tipoSolucion = await _tipoSolucionRepository.GetByIdAsync(dto.TipoSolucionId);
        if (tipoSolucion == null)
            throw new KeyNotFoundException($"Tipo de solución con ID {dto.TipoSolucionId} no encontrado");

        var proyecto = _mapper.Map<Proyecto>(dto);
        proyecto.Id = Guid.NewGuid();
        proyecto.Progreso = 0;
        proyecto.FechaInicio = DateTime.UtcNow;
        proyecto.FechaFin = DateTime.UtcNow.AddMonths(3);
        proyecto.TotalMiembros = 0;
        proyecto.CreatedAt = DateTime.UtcNow;
        proyecto.UpdatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(proyecto);

        cliente.TotalProyectos = (await _repository.GetByClienteIdAsync(cliente.Id)).Count();
        await _clienteRepository.UpdateAsync(cliente);

        return _mapper.Map<ProyectoDto>(created);
    }

    public async Task UpdateAsync(Guid id, UpdateProyectoDto dto)
    {
        var proyecto = await _repository.GetByIdAsync(id);
        if (proyecto == null)
            throw new KeyNotFoundException($"Proyecto con ID {id} no encontrado");

        var tipoSolucion = await _tipoSolucionRepository.GetByIdAsync(dto.TipoSolucionId);
        if (tipoSolucion == null)
            throw new KeyNotFoundException($"Tipo de solución con ID {dto.TipoSolucionId} no encontrado");

        _mapper.Map(dto, proyecto);
        proyecto.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(proyecto);
    }

    public async Task DeleteAsync(Guid id)
    {
        var proyecto = await _repository.GetByIdAsync(id);
        if (proyecto == null)
            throw new KeyNotFoundException($"Proyecto con ID {id} no encontrado");
        await _repository.DeleteAsync(proyecto);
    }
}
