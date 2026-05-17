using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class AmbienteCloudResourceRepository : IAmbienteCloudResourceRepository
{
    private readonly AppDbContext _context;

    public AmbienteCloudResourceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AmbienteCloudResource>> GetByAmbienteAsync(Guid ambienteId)
    {
        return await _context.AmbienteCloudResources
            .AsNoTracking()
            .Where(r => r.AmbienteId == ambienteId)
            .OrderBy(r => r.TipoRecurso)
            .ThenBy(r => r.NombreRecurso)
            .ToListAsync();
    }

    public async Task<AmbienteCloudResource?> GetByIdAsync(Guid id)
    {
        return await _context.AmbienteCloudResources
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<AmbienteCloudResource> CreateAsync(AmbienteCloudResource entity)
    {
        _context.AmbienteCloudResources.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(AmbienteCloudResource entity)
    {
        _context.AmbienteCloudResources.Update(entity);
        await _context.SaveChangesAsync();
    }
}
