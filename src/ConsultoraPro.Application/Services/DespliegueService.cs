using ConsultoraPro.Application.DTOs.Despliegues;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class DespliegueService : IDespliegueService
{
    private readonly IDespliegueRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;

    public DespliegueService(IDespliegueRepository repository, IProyectoRepository proyectoRepository)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
    }

    public async Task<PagedResultDto<DespliegueListDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        var items = await _repository.GetAllAsync(page, pageSize);
        var total = await _repository.GetTotalCountAsync();

        return new PagedResultDto<DespliegueListDto>
        {
            Data = items.Select(ToListDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<DespliegueListDto>> GetByProjectAsync(Guid proyectoId, int page = 1, int pageSize = 50)
    {
        var items = await _repository.GetByProjectAsync(proyectoId, page, pageSize);
        var total = await _repository.GetTotalCountAsync(proyectoId);

        return new PagedResultDto<DespliegueListDto>
        {
            Data = items.Select(ToListDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<DespliegueListDto>> GetRecentAsync(int count)
    {
        var items = await _repository.GetRecentAsync(count);
        return items.Select(ToListDto);
    }

    public async Task<DespliegueDto?> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null || !item.Activo ? null : ToDetailDto(item);
    }

    public async Task<DespliegueDto> CreateAsync(CreateDespliegueDto dto, Guid ejecutadoPorId)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(dto.ProyectoId);
        if (proyecto is null)
            throw new KeyNotFoundException($"Proyecto con ID {dto.ProyectoId} no encontrado");

        var despliegue = new Despliegue
        {
            Id = Guid.NewGuid(),
            ProyectoId = dto.ProyectoId,
            AmbienteId = dto.AmbienteId,
            Version = dto.Version.Trim(),
            EjecutadoPorId = ejecutadoPorId,
            FechaHora = DateTime.UtcNow,
            Estado = Domain.Enums.EstadoDespliegue.EnCurso,
            DuracionSegundos = dto.DuracionSegundos,
            Notas = dto.Notas.Trim(),
            Activo = true
        };

        var created = await _repository.CreateAsync(despliegue);
        var reloaded = await _repository.GetByIdAsync(created.Id);
        return ToDetailDto(reloaded ?? created);
    }

    public async Task UpdateEstadoAsync(Guid id, UpdateDespliegueEstadoDto dto)
    {
        var despliegue = await GetActiveEntityAsync(id);
        despliegue.Estado = dto.Estado;
        if (dto.DuracionSegundos.HasValue)
            despliegue.DuracionSegundos = dto.DuracionSegundos.Value;
        await _repository.UpdateAsync(despliegue);
    }

    private async Task<Despliegue> GetActiveEntityAsync(Guid id)
    {
        var despliegue = await _repository.GetByIdAsync(id);
        if (despliegue is null || !despliegue.Activo)
            throw new KeyNotFoundException($"Despliegue con ID {id} no encontrado");
        return despliegue;
    }

    private static DespliegueListDto ToListDto(Despliegue d)
    {
        return new DespliegueListDto
        {
            Id = d.Id,
            ProyectoNombre = d.Proyecto?.Nombre ?? string.Empty,
            ClienteNombre = d.Proyecto?.Cliente?.Nombre ?? string.Empty,
            AmbienteNombre = d.Ambiente?.Nombre ?? string.Empty,
            Version = d.Version,
            EjecutadoPorNombre = d.EjecutadoPor is null
                ? string.Empty
                : $"{d.EjecutadoPor.Nombres} {d.EjecutadoPor.Apellidos}".Trim(),
            FechaHora = d.FechaHora,
            Estado = d.Estado,
            DuracionSegundos = d.DuracionSegundos
        };
    }

    private static DespliegueDto ToDetailDto(Despliegue d)
    {
        return new DespliegueDto
        {
            Id = d.Id,
            ProyectoId = d.ProyectoId,
            ProyectoNombre = d.Proyecto?.Nombre ?? string.Empty,
            ClienteNombre = d.Proyecto?.Cliente?.Nombre ?? string.Empty,
            AmbienteId = d.AmbienteId,
            AmbienteNombre = d.Ambiente?.Nombre ?? string.Empty,
            Version = d.Version,
            EjecutadoPorId = d.EjecutadoPorId,
            EjecutadoPorNombre = d.EjecutadoPor is null
                ? string.Empty
                : $"{d.EjecutadoPor.Nombres} {d.EjecutadoPor.Apellidos}".Trim(),
            FechaHora = d.FechaHora,
            Estado = d.Estado,
            DuracionSegundos = d.DuracionSegundos,
            Notas = d.Notas
        };
    }
}
