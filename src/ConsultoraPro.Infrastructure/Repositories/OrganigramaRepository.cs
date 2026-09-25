using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class OrganigramaRepository : IOrganigramaRepository
{
    private readonly AppDbContext _context;

    public OrganigramaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Organigrama>> GetAllAsync()
    {
        return await _context.Organigramas
            .AsNoTracking()
            .OrderBy(o => o.Nombre)
            .ToListAsync();
    }

    public async Task<IReadOnlyDictionary<Guid, (int Nodos, int Asignados)>> GetNodoStatsAsync()
    {
        var stats = await _context.OrganigramaNodos
            .GroupBy(n => n.OrganigramaId)
            .Select(g => new
            {
                OrganigramaId = g.Key,
                Nodos = g.Count(),
                Asignados = g.Count(n => n.UsuarioId != null || n.NombreLibre != "")
            })
            .ToListAsync();

        return stats.ToDictionary(s => s.OrganigramaId, s => (s.Nodos, s.Asignados));
    }

    public async Task<Organigrama?> GetByIdAsync(Guid id)
    {
        return await _context.Organigramas
            .Include(o => o.Nodos)
                .ThenInclude(n => n.Usuario)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null)
    {
        var normalized = nombre.Trim().ToLower();
        return await _context.Organigramas
            .AnyAsync(o => o.Nombre.ToLower() == normalized && (excludeId == null || o.Id != excludeId));
    }

    public async Task<IReadOnlyList<Guid>> GetNodoIdsEnOtrosOrganigramasAsync(IEnumerable<Guid> nodoIds, Guid organigramaId)
    {
        var ids = nodoIds.Distinct().ToList();
        if (ids.Count == 0) return [];

        return await _context.OrganigramaNodos
            .Where(n => ids.Contains(n.Id) && n.OrganigramaId != organigramaId)
            .Select(n => n.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetUsuariosAsync(IEnumerable<Guid> ids)
    {
        var list = ids.Distinct().ToList();
        if (list.Count == 0) return [];

        return await _context.Users
            .AsNoTracking()
            .Where(u => list.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetUsuariosDisponiblesAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.Activo)
            .ThenBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .ToListAsync();
    }

    public async Task CreateAsync(Organigrama organigrama)
    {
        _context.Organigramas.Add(organigrama);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Organigrama organigrama, IReadOnlyList<OrganigramaNodo> nodos)
    {
        // Sincronización explícita (no Update()): los ids de nodo los genera el cliente, así que
        // Update() marcaría los nodos nuevos como modificados en lugar de agregados.
        var existentes = organigrama.Nodos.ToDictionary(n => n.Id);
        var deseados = nodos.Select(n => n.Id).ToHashSet();

        foreach (var nodo in existentes.Values.Where(n => !deseados.Contains(n.Id)))
        {
            organigrama.Nodos.Remove(nodo);
            _context.OrganigramaNodos.Remove(nodo);
        }

        foreach (var nodo in nodos)
        {
            if (existentes.TryGetValue(nodo.Id, out var actual))
            {
                actual.ParentId = nodo.ParentId;
                actual.Cargo = nodo.Cargo;
                actual.Area = nodo.Area;
                actual.NombreLibre = nodo.NombreLibre;
                actual.UsuarioId = nodo.UsuarioId;
                actual.Notas = nodo.Notas;
                actual.Color = nodo.Color;
                actual.Orden = nodo.Orden;
            }
            else
            {
                nodo.OrganigramaId = organigrama.Id;
                organigrama.Nodos.Add(nodo);
                _context.OrganigramaNodos.Add(nodo);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Organigrama organigrama)
    {
        _context.Organigramas.Remove(organigrama);
        await _context.SaveChangesAsync();
    }
}
