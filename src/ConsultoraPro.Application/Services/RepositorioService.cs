using ConsultoraPro.Application.DTOs.Repositorios;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class RepositorioService : IRepositorioService
{
    private readonly IRepositorioRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;

    public RepositorioService(IRepositorioRepository repository, IProyectoRepository proyectoRepository)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
    }

    public async Task<IEnumerable<RepositorioDto>> GetAllAsync(Guid? proyectoId = null)
    {
        var repositorios = await _repository.GetAllAsync(proyectoId);
        return repositorios.Select(ToDto);
    }

    public async Task<RepositorioDto?> GetByIdAsync(Guid id)
    {
        var repositorio = await _repository.GetByIdAsync(id);
        return repositorio is null || !repositorio.Activo ? null : ToDto(repositorio);
    }

    public async Task<RepositorioDto> CreateAsync(CreateRepositorioDto dto)
    {
        await EnsureProjectExistsAsync(dto.ProyectoId);

        var repositorio = new Repositorio
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            ProyectoId = dto.ProyectoId,
            Proveedor = dto.Proveedor,
            RamaPrincipal = dto.RamaPrincipal.Trim(),
            Url = dto.Url.Trim(),
            EstadoPipeline = dto.EstadoPipeline,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(repositorio);
        var reloaded = await _repository.GetByIdAsync(created.Id);
        return ToDto(reloaded ?? created);
    }

    public async Task UpdateAsync(Guid id, UpdateRepositorioDto dto)
    {
        var repositorio = await GetActiveEntityAsync(id);
        await EnsureProjectExistsAsync(dto.ProyectoId);

        repositorio.Nombre = dto.Nombre.Trim();
        repositorio.ProyectoId = dto.ProyectoId;
        repositorio.Proveedor = dto.Proveedor;
        repositorio.RamaPrincipal = dto.RamaPrincipal.Trim();
        repositorio.Url = dto.Url.Trim();
        repositorio.EstadoPipeline = dto.EstadoPipeline;

        await _repository.UpdateAsync(repositorio);
    }

    public async Task DeleteAsync(Guid id)
    {
        var repositorio = await GetActiveEntityAsync(id);
        repositorio.Activo = false;
        await _repository.UpdateAsync(repositorio);
    }

    private async Task<Repositorio> GetActiveEntityAsync(Guid id)
    {
        var repositorio = await _repository.GetByIdAsync(id);
        if (repositorio is null || !repositorio.Activo)
            throw new KeyNotFoundException($"Repositorio con ID {id} no encontrado");

        return repositorio;
    }

    private async Task EnsureProjectExistsAsync(Guid proyectoId)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId);
        if (proyecto is null)
            throw new KeyNotFoundException($"Proyecto con ID {proyectoId} no encontrado");
    }

    private static RepositorioDto ToDto(Repositorio repositorio)
    {
        return new RepositorioDto
        {
            Id = repositorio.Id,
            Nombre = repositorio.Nombre,
            ProyectoId = repositorio.ProyectoId,
            ProyectoNombre = repositorio.Proyecto?.Nombre ?? string.Empty,
            ClienteNombre = repositorio.Proyecto?.Cliente?.Nombre ?? string.Empty,
            Proveedor = repositorio.Proveedor,
            RamaPrincipal = repositorio.RamaPrincipal,
            Url = repositorio.Url,
            EstadoPipeline = repositorio.EstadoPipeline,
            Activo = repositorio.Activo,
            FechaCreacion = repositorio.FechaCreacion
        };
    }
}
