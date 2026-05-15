using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class DespliegueRepository : IDespliegueRepository
{
    private readonly AppDbContext _context;

    public DespliegueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Despliegue>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        return await _context.Despliegues
            .AsNoTracking()
            .Include(d => d.Proyecto).ThenInclude(p => p.Cliente)
            .Include(d => d.Ambiente)
            .Include(d => d.EjecutadoPor)
            .Where(d => d.Activo)
            .OrderByDescending(d => d.FechaHora)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Despliegue>> GetByProjectAsync(Guid proyectoId, int page = 1, int pageSize = 50)
    {
        return await _context.Despliegues
            .AsNoTracking()
            .Include(d => d.Proyecto).ThenInclude(p => p.Cliente)
            .Include(d => d.Ambiente)
            .Include(d => d.EjecutadoPor)
            .Where(d => d.Activo && d.ProyectoId == proyectoId)
            .OrderByDescending(d => d.FechaHora)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Despliegue>> GetRecentAsync(int count)
    {
        return await _context.Despliegues
            .AsNoTracking()
            .Include(d => d.Proyecto).ThenInclude(p => p.Cliente)
            .Include(d => d.Ambiente)
            .Include(d => d.EjecutadoPor)
            .Where(d => d.Activo)
            .OrderByDescending(d => d.FechaHora)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Despliegue?> GetByIdAsync(Guid id)
    {
        return await _context.Despliegues
            .Include(d => d.Proyecto).ThenInclude(p => p.Cliente)
            .Include(d => d.Ambiente)
            .Include(d => d.EjecutadoPor)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Despliegue> CreateAsync(Despliegue despliegue)
    {
        _context.Despliegues.Add(despliegue);
        await _context.SaveChangesAsync();
        return despliegue;
    }

    public async Task UpdateAsync(Despliegue despliegue)
    {
        _context.Despliegues.Update(despliegue);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetTotalCountAsync(Guid? proyectoId = null)
    {
        var query = _context.Despliegues.AsNoTracking().Where(d => d.Activo);
        if (proyectoId.HasValue)
            query = query.Where(d => d.ProyectoId == proyectoId.Value);
        return await query.CountAsync();
    }

    public async Task<(int total, int exitosos)> GetMonthlyStatsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var query = _context.Despliegues
            .AsNoTracking()
            .Where(d => d.Activo && d.FechaHora >= startOfMonth);

        var total = await query.CountAsync();
        var exitosos = await query.Where(d => d.Estado == EstadoDespliegue.Exitoso).CountAsync();

        return (total, exitosos);
    }
}
