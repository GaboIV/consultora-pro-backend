using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class TipoSolucionRepository : ITipoSolucionRepository
{
    private readonly AppDbContext _context;

    public TipoSolucionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TipoSolucion>> GetAllAsync()
    {
        return await _context.TiposSolucion.OrderBy(t => t.Nombre).ToListAsync();
    }

    public async Task<TipoSolucion?> GetByIdAsync(Guid id)
    {
        return await _context.TiposSolucion.FindAsync(id);
    }

    public async Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null)
    {
        var normalized = nombre.Trim().ToLower();
        return await _context.TiposSolucion
            .AnyAsync(t => t.Nombre.ToLower() == normalized && (excludeId == null || t.Id != excludeId));
    }

    public async Task<int> CountProyectosAsync(Guid id)
    {
        return await _context.Proyectos.CountAsync(p => p.TipoSolucionId == id);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetProyectoCountsAsync()
    {
        var counts = await _context.Proyectos
            .GroupBy(p => p.TipoSolucionId)
            .Select(g => new { TipoSolucionId = g.Key, Total = g.Count() })
            .ToListAsync();

        return counts.ToDictionary(c => c.TipoSolucionId, c => c.Total);
    }

    public async Task<TipoSolucion> CreateAsync(TipoSolucion tipoSolucion)
    {
        _context.TiposSolucion.Add(tipoSolucion);
        await _context.SaveChangesAsync();
        return tipoSolucion;
    }

    public async Task UpdateAsync(TipoSolucion tipoSolucion)
    {
        _context.TiposSolucion.Update(tipoSolucion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(TipoSolucion tipoSolucion)
    {
        _context.TiposSolucion.Remove(tipoSolucion);
        await _context.SaveChangesAsync();
    }
}
