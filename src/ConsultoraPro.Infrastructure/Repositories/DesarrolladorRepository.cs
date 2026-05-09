using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class DesarrolladorRepository : IDesarrolladorRepository
{
    private readonly AppDbContext _context;

    public DesarrolladorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Desarrollador>> GetByProyectoIdAsync(Guid proyectoId)
    {
        return await _context.Desarrolladores
            .Where(d => d.ProyectoId == proyectoId)
            .OrderBy(d => d.Rol)
            .ThenBy(d => d.Nombre)
            .ToListAsync();
    }

    public async Task<Desarrollador?> GetByIdAsync(Guid id)
    {
        return await _context.Desarrolladores.FindAsync(id);
    }

    public async Task<Desarrollador> CreateAsync(Desarrollador desarrollador)
    {
        _context.Desarrolladores.Add(desarrollador);
        await _context.SaveChangesAsync();
        return desarrollador;
    }

    public async Task UpdateAsync(Desarrollador desarrollador)
    {
        _context.Desarrolladores.Update(desarrollador);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Desarrollador desarrollador)
    {
        _context.Desarrolladores.Remove(desarrollador);
        await _context.SaveChangesAsync();
    }
}
