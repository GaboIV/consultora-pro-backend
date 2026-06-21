using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;

namespace ConsultoraPro.API.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RecordAsync(Guid actorId, string accion, string entidad, string? entidadId = null,
        string? antes = null, string? despues = null, string? ip = null)
    {
        ip ??= _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        _context.AuditoriasSeguridad.Add(new AuditoriaSeguridad
        {
            Id = Guid.NewGuid(),
            ActorId = actorId,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Antes = antes,
            Despues = despues,
            Ip = ip,
            Fecha = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}
