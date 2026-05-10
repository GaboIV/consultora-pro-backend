using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class CredencialRepository : ICredencialRepository
{
    private readonly AppDbContext _context;

    public CredencialRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Credencial>> GetAllAsync(Guid? proyectoId = null)
    {
        var query = _context.Credenciales
            .AsNoTracking()
            .Include(c => c.Proyecto)
            .Include(c => c.Ambiente)
            .Where(c => c.Activo);

        if (proyectoId.HasValue)
            query = query.Where(c => c.ProyectoId == proyectoId.Value);

        return await query
            .OrderBy(c => c.FechaVencimiento)
            .ThenBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Credencial?> GetByIdAsync(Guid id)
    {
        return await _context.Credenciales
            .Include(c => c.Proyecto)
            .Include(c => c.Ambiente)
            .Include(c => c.Creador)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Credencial> CreateAsync(Credencial credencial)
    {
        _context.Credenciales.Add(credencial);
        await _context.SaveChangesAsync();
        return credencial;
    }

    public async Task UpdateAsync(Credencial credencial)
    {
        _context.Credenciales.Update(credencial);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Credencial>> GetExpiringWithinAsync(int days)
    {
        var today = DateTime.UtcNow.Date;
        var limit = today.AddDays(days);

        return await _context.Credenciales
            .AsNoTracking()
            .Include(c => c.Proyecto)
            .Include(c => c.Ambiente)
            .Where(c => c.Activo && c.FechaVencimiento.Date <= limit)
            .OrderBy(c => c.FechaVencimiento)
            .ToListAsync();
    }

    public async Task AddAuditAsync(AuditoriaCredencial auditoria)
    {
        _context.AuditoriasCredenciales.Add(auditoria);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditoriaCredencial>> GetAuditAsync(Guid credencialId)
    {
        return await _context.AuditoriasCredenciales
            .AsNoTracking()
            .Include(a => a.Usuario)
            .Where(a => a.CredencialId == credencialId)
            .OrderByDescending(a => a.FechaRevelacion)
            .ToListAsync();
    }
}
