using AutoMapper;
using ConsultoraPro.Application.DTOs.Proyectos;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Application.Services;

public class ProyectoService : IProyectoService
{
    private readonly IProyectoRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ITipoSolucionRepository _tipoSolucionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ProyectoService(
        IProyectoRepository repository, 
        IClienteRepository clienteRepository, 
        ITipoSolucionRepository tipoSolucionRepository, 
        UserManager<ApplicationUser> userManager, 
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _tipoSolucionRepository = tipoSolucionRepository;
        _userManager = userManager;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResultDto<ProyectoDto>> GetAllAsync(int page = 1, int pageSize = 20, EstadoProyecto? estado = null, Guid? clienteId = null)
    {
        Guid? memberUserId = null;
        if (!_currentUserService.HasFullProjectAccessFor("proyectos"))
        {
            memberUserId = _currentUserService.UserId;
        }

        var items = await _repository.GetPagedAsync(page, pageSize, estado, clienteId, memberUserId);
        var total = await _repository.GetTotalCountAsync(estado, clienteId, memberUserId);

        return new PagedResultDto<ProyectoDto>
        {
            Data = _mapper.Map<List<ProyectoDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProyectoDto?> GetByIdAsync(Guid id)
    {
        var proyecto = await _repository.GetByIdAsync(id);
        if (proyecto == null) return null;

        if (!_currentUserService.HasFullProjectAccessFor("proyectos"))
        {
            var isMember = proyecto.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId);
            if (!isMember) return null;
        }

        return _mapper.Map<ProyectoDto>(proyecto);
    }

    public async Task<IEnumerable<ProyectoDto>> GetByClienteIdAsync(Guid clienteId)
    {
        var proyectos = await _repository.GetByClienteIdAsync(clienteId);

        if (!_currentUserService.HasFullProjectAccessFor("proyectos"))
        {
            proyectos = proyectos.Where(p => p.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId));
        }

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
        proyecto.Progreso = dto.Progreso;
        proyecto.FechaInicio = DateTime.SpecifyKind(dto.FechaInicio, DateTimeKind.Utc);
        proyecto.FechaFin = DateTime.SpecifyKind(dto.FechaFin, DateTimeKind.Utc);
        proyecto.TotalMiembros = dto.Miembros.Count;
        proyecto.CreatedAt = DateTime.UtcNow;
        proyecto.UpdatedAt = DateTime.UtcNow;

        foreach (var mDto in dto.Miembros)
        {
            var user = await _userManager.FindByIdAsync(mDto.UsuarioId.ToString());
            if (user == null)
                throw new KeyNotFoundException($"Usuario con ID {mDto.UsuarioId} no encontrado");

            proyecto.ProyectoMiembros.Add(new ProyectoMiembro
            {
                Id = Guid.NewGuid(),
                UsuarioId = user.Id,
                Rol = mDto.Rol,
                ProyectoId = proyecto.Id
            });
        }

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

        var previousClienteId = proyecto.ClienteId;
        var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente con ID {dto.ClienteId} no encontrado");

        var tipoSolucion = await _tipoSolucionRepository.GetByIdAsync(dto.TipoSolucionId);
        if (tipoSolucion == null)
            throw new KeyNotFoundException($"Tipo de solución con ID {dto.TipoSolucionId} no encontrado");

        _mapper.Map(dto, proyecto);
        proyecto.FechaInicio = DateTime.SpecifyKind(dto.FechaInicio, DateTimeKind.Utc);
        proyecto.FechaFin = DateTime.SpecifyKind(dto.FechaFin, DateTimeKind.Utc);
        proyecto.TotalMiembros = dto.Miembros.Count;
        proyecto.UpdatedAt = DateTime.UtcNow;

        // Build lookup of existing members for diff
        var existingByUser = proyecto.ProyectoMiembros
            .ToDictionary(m => m.UsuarioId);

        // Remove members not present in the new list
        var toRemove = proyecto.ProyectoMiembros
            .Where(m => !dto.Miembros.Any(nm => nm.UsuarioId == m.UsuarioId))
            .ToList();
        foreach (var m in toRemove)
            proyecto.ProyectoMiembros.Remove(m);

        // Add or update members
        foreach (var mDto in dto.Miembros)
        {
            if (existingByUser.TryGetValue(mDto.UsuarioId, out var existingMember))
            {
                existingMember.Rol = mDto.Rol;
            }
            else
            {
                var user = await _userManager.FindByIdAsync(mDto.UsuarioId.ToString());
                if (user == null)
                    throw new KeyNotFoundException($"Usuario con ID {mDto.UsuarioId} no encontrado");

                proyecto.ProyectoMiembros.Add(new ProyectoMiembro
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = user.Id,
                    Rol = mDto.Rol,
                    ProyectoId = proyecto.Id
                });
            }
        }

        await _repository.UpdateAsync(proyecto);
        await RefreshClientProjectCountAsync(previousClienteId);
        if (previousClienteId != dto.ClienteId)
            await RefreshClientProjectCountAsync(dto.ClienteId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var proyecto = await _repository.GetByIdAsync(id);
        if (proyecto == null)
            throw new KeyNotFoundException($"Proyecto con ID {id} no encontrado");
        var clienteId = proyecto.ClienteId;
        await _repository.DeleteAsync(proyecto);
        await RefreshClientProjectCountAsync(clienteId);
    }

    private async Task RefreshClientProjectCountAsync(Guid clienteId)
    {
        var cliente = await _clienteRepository.GetByIdAsync(clienteId);
        if (cliente == null) return;

        cliente.TotalProyectos = (await _repository.GetByClienteIdAsync(clienteId)).Count();
        await _clienteRepository.UpdateAsync(cliente);
    }
}
