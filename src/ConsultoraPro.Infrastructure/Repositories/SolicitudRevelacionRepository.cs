using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class SolicitudRevelacionRepository : ISolicitudRevelacionRepository
{
    private readonly AppDbContext _context;

    public SolicitudRevelacionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SolicitudRevelacionCredencial?> GetByIdAsync(Guid id)
    {
        return await _context.SolicitudesRevelacionCredencial
            .Include(s => s.Credencial).ThenInclude(c => c.Proyecto)
            .Include(s => s.Solicitante)
            .Include(s => s.Aprobador)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SolicitudRevelacionCredencial?> GetPendienteAsync(Guid credencialId, Guid solicitanteId)
    {
        return await _context.SolicitudesRevelacionCredencial
            .Where(s => s.CredencialId == credencialId
                        && s.SolicitanteId == solicitanteId
                        && s.Estado == EstadoSolicitudRevelacion.Pendiente)
            .OrderByDescending(s => s.FechaSolicitud)
            .FirstOrDefaultAsync();
    }

    public async Task<SolicitudRevelacionCredencial?> GetVigenteAsync(Guid credencialId, Guid usuarioId, DateTime now)
    {
        return await _context.SolicitudesRevelacionCredencial
            .Where(s => s.CredencialId == credencialId
                        && s.SolicitanteId == usuarioId
                        && s.Estado == EstadoSolicitudRevelacion.Aprobada
                        && s.VigenteHasta != null
                        && s.VigenteHasta > now)
            .OrderByDescending(s => s.VigenteHasta)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SolicitudRevelacionCredencial>> ListAsync(EstadoSolicitudRevelacion? estado)
    {
        var query = _context.SolicitudesRevelacionCredencial
            .AsNoTracking()
            .Include(s => s.Credencial).ThenInclude(c => c.Proyecto)
            .Include(s => s.Solicitante)
            .Include(s => s.Aprobador)
            .AsQueryable();

        if (estado.HasValue)
            query = query.Where(s => s.Estado == estado.Value);

        return await query
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();
    }

    public async Task<IEnumerable<SolicitudRevelacionCredencial>> ListBySolicitanteAsync(Guid solicitanteId)
    {
        return await _context.SolicitudesRevelacionCredencial
            .AsNoTracking()
            .Include(s => s.Credencial).ThenInclude(c => c.Proyecto)
            .Include(s => s.Aprobador)
            .Where(s => s.SolicitanteId == solicitanteId)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();
    }

    public async Task CreateAsync(SolicitudRevelacionCredencial solicitud)
    {
        _context.SolicitudesRevelacionCredencial.Add(solicitud);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SolicitudRevelacionCredencial solicitud)
    {
        _context.SolicitudesRevelacionCredencial.Update(solicitud);
        await _context.SaveChangesAsync();
    }
}
