namespace ConsultoraPro.Application.Interfaces;

public interface IAuditService
{
    Task RecordAsync(Guid actorId, string accion, string entidad, string? entidadId = null,
        string? antes = null, string? despues = null, string? ip = null);
}
