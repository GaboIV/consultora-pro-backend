using ConsultoraPro.Application.DTOs.Kanban;

namespace ConsultoraPro.Application.Interfaces;

public interface ITarjetaService
{
    Task<TarjetaDetalleDto?> GetByIdAsync(Guid id);
    Task<TarjetaDetalleDto> CreateAsync(CreateTarjetaDto dto, Guid usuarioId);
    Task UpdateAsync(Guid id, UpdateTarjetaDto dto, Guid usuarioId);
    Task MoverAsync(Guid id, MoverTarjetaDto dto, Guid usuarioId);
    Task DeleteAsync(Guid id, Guid usuarioId);

    Task<IEnumerable<ResponsableDto>> AsignarResponsablesAsync(Guid id, AsignarResponsablesDto dto, Guid usuarioId);
    Task<IEnumerable<EtiquetaDto>> AsignarEtiquetasAsync(Guid id, AsignarEtiquetasDto dto, Guid usuarioId);

    Task<ChecklistDto> AddChecklistAsync(Guid tarjetaId, CreateChecklistDto dto);
    Task<ChecklistDto> UpdateChecklistAsync(Guid tarjetaId, Guid checklistId, UpdateChecklistDto dto);
    Task DeleteChecklistAsync(Guid tarjetaId, Guid checklistId);

    Task<ChecklistItemDto> AddChecklistItemAsync(Guid tarjetaId, Guid checklistId, CreateChecklistItemDto dto);
    Task<ChecklistItemDto> UpdateChecklistItemAsync(Guid tarjetaId, Guid checklistId, Guid itemId, UpdateChecklistItemDto dto);
    Task DeleteChecklistItemAsync(Guid tarjetaId, Guid checklistId, Guid itemId);

    Task<ComentarioDto> AddComentarioAsync(Guid tarjetaId, CreateComentarioDto dto, Guid usuarioId);
    Task<ComentarioDto> UpdateComentarioAsync(Guid tarjetaId, Guid comentarioId, UpdateComentarioDto dto, Guid usuarioId);
    Task DeleteComentarioAsync(Guid tarjetaId, Guid comentarioId);

    Task<AdjuntoDto> AddAdjuntoAsync(Guid tarjetaId, string nombre, string url, string? contentType, long tamanoBytes, Guid usuarioId);
    Task DeleteAdjuntoAsync(Guid tarjetaId, Guid adjuntoId);

    Task<IEnumerable<ActividadDto>> GetActividadAsync(Guid tarjetaId);
}
