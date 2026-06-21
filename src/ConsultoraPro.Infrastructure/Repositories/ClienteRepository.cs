using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> GetAllAsync(Guid? memberUserId = null)
    {
        var query = _context.Clientes
            .Where(c => c.Activo);

        if (memberUserId.HasValue)
        {
            query = query.Where(c => c.Proyectos.Any(p => p.ProyectoMiembros.Any(pm => pm.UsuarioId == memberUserId.Value)));
        }

        return await query
            .OrderByDescending(c => c.FechaAlta)
            .Include(c => c.Proyectos)
            .ToListAsync();
    }

    public async Task<Cliente?> GetByIdAsync(Guid id)
    {
        return await _context.Clientes
            .Include(c => c.Proyectos)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente> CreateAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Cliente>> GetPagedAsync(int page, int pageSize, string? search = null, Guid? memberUserId = null)
    {
        var query = _context.Clientes
            .Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c => c.Nombre.ToLower().Contains(searchLower));
        }

        if (memberUserId.HasValue)
        {
            query = query.Where(c => c.Proyectos.Any(p => p.ProyectoMiembros.Any(pm => pm.UsuarioId == memberUserId.Value)));
        }

        return await query
            .OrderByDescending(c => c.FechaAlta)
            .Include(c => c.Proyectos)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? search = null, Guid? memberUserId = null)
    {
        var query = _context.Clientes
            .Where(c => c.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c => c.Nombre.ToLower().Contains(searchLower));
        }

        if (memberUserId.HasValue)
        {
            query = query.Where(c => c.Proyectos.Any(p => p.ProyectoMiembros.Any(pm => pm.UsuarioId == memberUserId.Value)));
        }

        return await query.CountAsync();
    }
}
