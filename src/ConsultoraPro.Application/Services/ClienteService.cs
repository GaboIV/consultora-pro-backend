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

    public ClienteService(IClienteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ClienteDto>> GetAllAsync()
    {
        var clientes = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ClienteDto>>(clientes);
    }

    public async Task<ClienteDto?> GetByIdAsync(Guid id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        return cliente == null ? null : _mapper.Map<ClienteDto>(cliente);
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
