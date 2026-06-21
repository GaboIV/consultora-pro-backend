using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Enums;
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

    public async Task<IEnumerable<Proyecto>> GetAllAsync(Guid? memberUserId = null)
    {
        var query = _context.Proyectos
            .Include(p => p.Cliente)
            .Include(p => p.TipoSolucion)
            .Include(p => p.Repositorios)
            .Include(p => p.Screenshots)
                .ThenInclude(s => s.SubidoPor)
            .Include(p => p.ProyectoMiembros)
                .ThenInclude(pm => pm.Usuario)
            .AsQueryable();

        if (memberUserId.HasValue)
        {
            query = query.Where(p => p.ProyectoMiembros.Any(pm => pm.UsuarioId == memberUserId.Value));
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Proyecto?> GetByIdAsync(Guid id)
    {
        return await _context.Proyectos
            .Include(p => p.Cliente)
            .Include(p => p.TipoSolucion)
            .Include(p => p.Repositorios)
            .Include(p => p.Screenshots)
                .ThenInclude(s => s.SubidoPor)
            .Include(p => p.ProyectoMiembros)
                .ThenInclude(pm => pm.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Proyecto>> GetByClienteIdAsync(Guid clienteId)
    {
        return await _context.Proyectos
            .Include(p => p.Cliente)
            .Include(p => p.TipoSolucion)
            .Include(p => p.Repositorios)
            .Include(p => p.Screenshots)
                .ThenInclude(s => s.SubidoPor)
            .Include(p => p.ProyectoMiembros)
                .ThenInclude(pm => pm.Usuario)
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
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Proyecto proyecto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var proyectoId = proyecto.Id;

        var credenciales = await _context.Credenciales
            .Where(c => c.ProyectoId == proyectoId)
            .ToListAsync();
        if (credenciales.Count != 0)
            _context.Credenciales.RemoveRange(credenciales);

        var ambientes = await _context.Ambientes
            .Where(a => a.ProyectoId == proyectoId)
            .ToListAsync();
        if (ambientes.Count != 0)
            _context.Ambientes.RemoveRange(ambientes);

        var repositorios = await _context.Repositorios
            .Where(r => r.ProyectoId == proyectoId)
            .ToListAsync();
        if (repositorios.Count != 0)
            _context.Repositorios.RemoveRange(repositorios);

        var miembros = await _context.ProyectoMiembros
            .Where(pm => pm.ProyectoId == proyectoId)
            .ToListAsync();
        if (miembros.Count != 0)
            _context.ProyectoMiembros.RemoveRange(miembros);

        _context.Proyectos.Remove(proyecto);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<IEnumerable<Proyecto>> GetPagedAsync(int page, int pageSize, EstadoProyecto? estado = null, Guid? clienteId = null, Guid? memberUserId = null)
    {
        var query = _context.Proyectos
            .Include(p => p.Cliente)
            .Include(p => p.TipoSolucion)
            .Include(p => p.Repositorios)
            .Include(p => p.Screenshots)
                .ThenInclude(s => s.SubidoPor)
            .Include(p => p.ProyectoMiembros)
                .ThenInclude(pm => pm.Usuario)
            .AsQueryable();

        if (estado.HasValue)
            query = query.Where(p => p.Estado == estado.Value);

        if (clienteId.HasValue)
            query = query.Where(p => p.ClienteId == clienteId.Value);

        if (memberUserId.HasValue)
            query = query.Where(p => p.ProyectoMiembros.Any(pm => pm.UsuarioId == memberUserId.Value));

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(EstadoProyecto? estado = null, Guid? clienteId = null, Guid? memberUserId = null)
    {
        var query = _context.Proyectos.AsQueryable();

        if (estado.HasValue)
            query = query.Where(p => p.Estado == estado.Value);

        if (clienteId.HasValue)
            query = query.Where(p => p.ClienteId == clienteId.Value);

        if (memberUserId.HasValue)
            query = query.Where(p => p.ProyectoMiembros.Any(pm => pm.UsuarioId == memberUserId.Value));

        return await query.CountAsync();
    }
}
