using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class AmbienteTestUserRepository : IAmbienteTestUserRepository
{
    private readonly AppDbContext _context;

    public AmbienteTestUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AmbienteTestUser>> GetByAmbienteAsync(Guid ambienteId)
    {
        return await _context.AmbienteTestUsers
            .AsNoTracking()
            .Where(t => t.AmbienteId == ambienteId)
            .OrderBy(t => t.RolAplicacion)
            .ThenBy(t => t.Correo)
            .ToListAsync();
    }

    public async Task<AmbienteTestUser?> GetByIdAsync(Guid id)
    {
        return await _context.AmbienteTestUsers
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<AmbienteTestUser> CreateAsync(AmbienteTestUser entity)
    {
        _context.AmbienteTestUsers.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(AmbienteTestUser entity)
    {
        _context.AmbienteTestUsers.Update(entity);
        await _context.SaveChangesAsync();
    }
}
