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
}
