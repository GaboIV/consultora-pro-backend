using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class ScreenshotRepository : IScreenshotRepository
{
    private readonly AppDbContext _context;

    public ScreenshotRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Screenshot>> GetByProyectoIdAsync(Guid proyectoId)
    {
        return await _context.Screenshots
            .AsNoTracking()
            .Include(s => s.SubidoPor)
            .Where(s => s.ProyectoId == proyectoId && s.Activo)
            .OrderByDescending(s => s.FechaSubida)
            .ToListAsync();
    }

    public async Task<Screenshot?> GetByIdAsync(Guid id)
    {
        return await _context.Screenshots
            .Include(s => s.SubidoPor)
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);
    }

    public async Task<Screenshot> CreateAsync(Screenshot screenshot)
    {
        _context.Screenshots.Add(screenshot);
        await _context.SaveChangesAsync();
        return screenshot;
    }

    public async Task DeleteAsync(Screenshot screenshot)
    {
        _context.Screenshots.Remove(screenshot);
        await _context.SaveChangesAsync();
    }
}
