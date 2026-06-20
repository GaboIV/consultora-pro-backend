using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class ColumnaKanbanRepository : IColumnaKanbanRepository
{
    private readonly AppDbContext _context;

    public ColumnaKanbanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ColumnaKanban?> GetByIdAsync(Guid id)
    {
        return await _context.ColumnasKanban
            .Include(c => c.Tablero)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<ColumnaKanban>> GetActiveByTableroAsync(Guid tableroId)
    {
        return await _context.ColumnasKanban
            .AsNoTracking()
            .Where(c => c.TableroId == tableroId && c.Activo)
            .OrderBy(c => c.Orden)
            .ToListAsync();
    }

    public async Task<ColumnaKanban> CreateAsync(ColumnaKanban columna)
    {
        _context.ColumnasKanban.Add(columna);
        await _context.SaveChangesAsync();
        return columna;
    }

    public async Task UpdateAsync(ColumnaKanban columna)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountTarjetasActivasAsync(Guid columnaId)
    {
        return await _context.Tarjetas.CountAsync(t => t.ColumnaId == columnaId && t.Activo);
    }

    public async Task<double> GetMaxOrdenAsync(Guid tableroId)
    {
        var columnas = _context.ColumnasKanban.Where(c => c.TableroId == tableroId && c.Activo);
        return await columnas.AnyAsync()
            ? await columnas.MaxAsync(c => c.Orden)
            : 0d;
    }
}
