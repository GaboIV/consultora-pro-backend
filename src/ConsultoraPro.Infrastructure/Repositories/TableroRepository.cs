using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class TableroRepository : ITableroRepository
{
    private readonly AppDbContext _context;

    public TableroRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tablero>> GetByProyectoAsync(Guid proyectoId)
    {
        return await _context.Tableros
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Columnas.Where(c => c.Activo))
                .ThenInclude(c => c.Tarjetas.Where(ta => ta.Activo))
            .Include(t => t.Miembros)
            .Where(t => t.ProyectoId == proyectoId && t.Activo)
            .OrderBy(t => t.Orden)
            .ThenBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tablero>> GetByUsuarioAsync(Guid usuarioId)
    {
        return await _context.Tableros
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Proyecto)
            .Include(t => t.Columnas.Where(c => c.Activo))
                .ThenInclude(c => c.Tarjetas.Where(ta => ta.Activo))
            .Include(t => t.Miembros)
            .Where(t => t.Activo && t.Miembros.Any(m => m.UsuarioId == usuarioId))
            .OrderBy(t => t.ProyectoId == null ? 1 : 0)
            .ThenBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<Tablero?> GetByIdAsync(Guid id)
    {
        return await _context.Tableros
            .Include(t => t.Proyecto)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tablero?> GetDetalleAsync(Guid id)
    {
        return await _context.Tableros
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Proyecto)
            .Include(t => t.Columnas.Where(c => c.Activo))
                .ThenInclude(c => c.Tarjetas.Where(ta => ta.Activo))
                    .ThenInclude(ta => ta.Responsables)
                        .ThenInclude(r => r.Usuario)
            .Include(t => t.Columnas.Where(c => c.Activo))
                .ThenInclude(c => c.Tarjetas.Where(ta => ta.Activo))
                    .ThenInclude(ta => ta.Etiquetas)
                        .ThenInclude(te => te.Etiqueta)
            .Include(t => t.Columnas.Where(c => c.Activo))
                .ThenInclude(c => c.Tarjetas.Where(ta => ta.Activo))
                    .ThenInclude(ta => ta.Checklists)
                        .ThenInclude(cl => cl.Items)
            .Include(t => t.Columnas.Where(c => c.Activo))
                .ThenInclude(c => c.Tarjetas.Where(ta => ta.Activo))
                    .ThenInclude(ta => ta.Comentarios)
            .Include(t => t.Columnas.Where(c => c.Activo))
                .ThenInclude(c => c.Tarjetas.Where(ta => ta.Activo))
                    .ThenInclude(ta => ta.Adjuntos)
            .Include(t => t.Etiquetas.Where(e => e.Activo))
            .Include(t => t.Miembros)
                .ThenInclude(m => m.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tablero?> GetWithMiembrosAsync(Guid id)
    {
        return await _context.Tableros
            .Include(t => t.Miembros)
                .ThenInclude(m => m.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tablero?> GetWithEtiquetasAsync(Guid id)
    {
        return await _context.Tableros
            .Include(t => t.Etiquetas)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tablero> CreateAsync(Tablero tablero)
    {
        _context.Tableros.Add(tablero);
        await _context.SaveChangesAsync();
        return tablero;
    }

    public async Task UpdateAsync(Tablero tablero)
    {
        // La entidad llega rastreada desde los Get* del propio repositorio; basta con
        // persistir los cambios detectados (incluidas altas/bajas en colecciones hijas).
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ClaveExistsAsync(Guid? proyectoId, Guid? creadoPorId, string clave, Guid? excludeId = null)
    {
        // Para tableros de proyecto: unicidad dentro del proyecto.
        // Para tableros personales: unicidad dentro del espacio del creador.
        return await _context.Tableros.AnyAsync(t =>
            t.Activo &&
            t.Clave == clave &&
            (excludeId == null || t.Id != excludeId.Value) &&
            (proyectoId.HasValue
                ? t.ProyectoId == proyectoId
                : t.ProyectoId == null && t.CreadoPorId == creadoPorId));
    }

    public async Task<int> GetMaxOrdenAsync(Guid? proyectoId, Guid? creadoPorId)
    {
        IQueryable<Tablero> query = proyectoId.HasValue
            ? _context.Tableros.Where(t => t.ProyectoId == proyectoId && t.Activo)
            : _context.Tableros.Where(t => t.ProyectoId == null && t.CreadoPorId == creadoPorId && t.Activo);
        return await query.AnyAsync()
            ? await query.MaxAsync(t => t.Orden)
            : 0;
    }

    public async Task<EtiquetaKanban?> GetEtiquetaAsync(Guid etiquetaId)
    {
        return await _context.EtiquetasKanban.FirstOrDefaultAsync(e => e.Id == etiquetaId);
    }

    public async Task AddEtiquetaAndSaveAsync(EtiquetaKanban etiqueta)
    {
        _context.EtiquetasKanban.Add(etiqueta);
        await _context.SaveChangesAsync();
    }
}
