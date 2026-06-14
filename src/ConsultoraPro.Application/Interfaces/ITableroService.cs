using ConsultoraPro.Application.DTOs.Kanban;

namespace ConsultoraPro.Application.Interfaces;

public interface ITableroService
{
    Task<IEnumerable<TableroDto>> GetByProyectoAsync(Guid proyectoId);
    Task<TableroDetalleDto?> GetDetalleAsync(Guid id);
    Task<TableroDto> CreateAsync(CreateTableroDto dto);
    Task UpdateAsync(Guid id, UpdateTableroDto dto);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<TableroMiembroDto>> UpdateMiembrosAsync(Guid id, UpdateMiembrosDto dto);

    Task<IEnumerable<EtiquetaDto>> GetEtiquetasAsync(Guid tableroId);
    Task<EtiquetaDto> CreateEtiquetaAsync(Guid tableroId, CreateEtiquetaDto dto);
    Task<EtiquetaDto> UpdateEtiquetaAsync(Guid tableroId, Guid etiquetaId, UpdateEtiquetaDto dto);
    Task DeleteEtiquetaAsync(Guid tableroId, Guid etiquetaId);
}
