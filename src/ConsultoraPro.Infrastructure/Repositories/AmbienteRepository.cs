using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class AmbienteRepository : IAmbienteRepository
{
    private readonly AppDbContext _context;

    public AmbienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Ambiente>> GetAllAsync(Guid? proyectoId = null)
    {
        var query = _context.Ambientes
            .AsNoTracking()
            .Include(a => a.Proyecto)
                .ThenInclude(p => p.Cliente)
            .Where(a => a.Activo);

        if (proyectoId.HasValue)
            query = query.Where(a => a.ProyectoId == proyectoId.Value);

        return await query
            .OrderBy(a => a.Proyecto.Cliente.Nombre)
            .ThenBy(a => a.Proyecto.Nombre)
            .ThenBy(a => a.Tipo)
            .ThenBy(a => a.Nombre)
            .ToListAsync();
    }

    public async Task<Ambiente?> GetByIdAsync(Guid id)
    {
        return await _context.Ambientes
            .Include(a => a.Proyecto)
                .ThenInclude(p => p.Cliente)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Ambiente> CreateAsync(Ambiente ambiente)
    {
        _context.Ambientes.Add(ambiente);
        await _context.SaveChangesAsync();
        return ambiente;
    }

    public async Task UpdateAsync(Ambiente ambiente)
    {
        _context.Ambientes.Update(ambiente);
        await _context.SaveChangesAsync();
    }
}
