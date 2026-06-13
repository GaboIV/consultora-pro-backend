using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class AmbienteComponenteRepository : IAmbienteComponenteRepository
{
    private readonly AppDbContext _context;

    public AmbienteComponenteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AmbienteComponente>> GetByAmbienteAsync(Guid ambienteId)
    {
        return await _context.AmbienteComponentes
            .AsNoTracking()
            .Where(c => c.AmbienteId == ambienteId)
            .OrderBy(c => c.Rol)
            .ThenBy(c => c.Hostname)
            .ToListAsync();
    }

    public async Task<AmbienteComponente?> GetByIdAsync(Guid id)
    {
        return await _context.AmbienteComponentes
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<AmbienteComponente> CreateAsync(AmbienteComponente entity)
    {
        _context.AmbienteComponentes.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(AmbienteComponente entity)
    {
        _context.AmbienteComponentes.Update(entity);
        await _context.SaveChangesAsync();
    }
}
