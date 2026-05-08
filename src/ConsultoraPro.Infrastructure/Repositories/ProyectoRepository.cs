using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class ProyectoRepository : IProyectoRepository
{
    private readonly AppDbContext _context;

    public ProyectoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Proyecto>> GetAllAsync()
    {
        return await _context.Proyectos
            .Include(p => p.Cliente)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Proyecto?> GetByIdAsync(Guid id)
    {
        return await _context.Proyectos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Proyecto>> GetByClienteIdAsync(Guid clienteId)
    {
        return await _context.Proyectos
            .Include(p => p.Cliente)
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Proyecto> CreateAsync(Proyecto proyecto)
    {
        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync();
        return proyecto;
    }

    public async Task UpdateAsync(Proyecto proyecto)
    {
        _context.Proyectos.Update(proyecto);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Proyecto proyecto)
    {
        _context.Proyectos.Remove(proyecto);
        await _context.SaveChangesAsync();
    }
}
