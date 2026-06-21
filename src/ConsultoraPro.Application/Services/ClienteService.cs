using AutoMapper;
using ConsultoraPro.Application.DTOs.Clientes;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ClienteService(IClienteRepository repository, IMapper mapper, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResultDto<ClienteDto>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        Guid? memberUserId = null;
        if (_currentUserService.IsInRole("Soporte"))
        {
            memberUserId = _currentUserService.UserId;
        }

        var items = await _repository.GetPagedAsync(page, pageSize, search, memberUserId);
        var total = await _repository.GetTotalCountAsync(search, memberUserId);

        return new PagedResultDto<ClienteDto>
        {
            Data = _mapper.Map<List<ClienteDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ClienteDto?> GetByIdAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente == null) return null;

        if (_currentUserService.IsInRole("Soporte"))
        {
            var userId = _currentUserService.UserId;
            var isMemberOfAnyProject = cliente.Proyectos.Any(p => p.ProyectoMiembros.Any(pm => pm.UsuarioId == userId));
            if (!isMemberOfAnyProject) return null;
        }

        return _mapper.Map<ClienteDto>(cliente);
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
    {
        var cliente = _mapper.Map<Cliente>(dto);
        cliente.Id = Guid.NewGuid();
        cliente.FechaAlta = DateTime.UtcNow;
        var created = await _repository.CreateAsync(cliente);
        return _mapper.Map<ClienteDto>(created);
    }

    public async Task UpdateAsync(Guid id, UpdateClienteDto dto)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {id} no encontrado");
        _mapper.Map(dto, cliente);
        await _repository.UpdateAsync(cliente);
    }

    public async Task DeleteAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {id} no encontrado");
        cliente.Activo = false;
        await _repository.UpdateAsync(cliente);
    }
}
