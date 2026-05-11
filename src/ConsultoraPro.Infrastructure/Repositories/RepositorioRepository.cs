using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class RepositorioRepository : IRepositorioRepository
{
    private readonly AppDbContext _context;

    public RepositorioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Repositorio>> GetAllAsync(Guid? proyectoId = null)
    {
        var query = _context.Repositorios
            .AsNoTracking()
            .Include(r => r.Proyecto)
                .ThenInclude(p => p.Cliente)
            .Where(r => r.Activo);

        if (proyectoId.HasValue)
            query = query.Where(r => r.ProyectoId == proyectoId.Value);

        return await query
            .OrderBy(r => r.Proyecto.Cliente.Nombre)
            .ThenBy(r => r.Proyecto.Nombre)
            .ThenBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<Repositorio?> GetByIdAsync(Guid id)
    {
        return await _context.Repositorios
            .Include(r => r.Proyecto)
                .ThenInclude(p => p.Cliente)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Repositorio> CreateAsync(Repositorio repositorio)
    {
        _context.Repositorios.Add(repositorio);
        await _context.SaveChangesAsync();
        return repositorio;
    }

    public async Task UpdateAsync(Repositorio repositorio)
    {
        _context.Repositorios.Update(repositorio);
        await _context.SaveChangesAsync();
    }
}
