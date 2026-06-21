using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.API.Services;

public class ProjectScopeService : IProjectScope
{
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _context;

    public ProjectScopeService(ICurrentUserService currentUser, AppDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public bool VeTodos(string modulo)
    {
        return _currentUser.HasFullProjectAccessFor(modulo);
    }

    public async Task<IReadOnlySet<Guid>> ProyectosAsignadosAsync()
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return new HashSet<Guid>();

        var ids = await _context.ProyectoMiembros
            .AsNoTracking()
            .Where(pm => pm.UsuarioId == userId.Value)
            .Select(pm => pm.ProyectoId)
            .ToListAsync();

        return new HashSet<Guid>(ids);
    }

    public bool EsMiembro(Guid proyectoId)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return false;

        return _context.ProyectoMiembros
            .AsNoTracking()
            .Any(pm => pm.UsuarioId == userId.Value && pm.ProyectoId == proyectoId);
    }

    public IQueryable<T> FiltrarPorAcceso<T>(IQueryable<T> query,
        System.Linq.Expressions.Expression<Func<T, Guid>> proyectoIdSelector, string modulo)
    {
        if (VeTodos(modulo))
            return query;

        var userId = _currentUser.UserId;
        if (userId is null)
            return Enumerable.Empty<T>().AsQueryable();

        var proyectoIds = _context.ProyectoMiembros
            .AsNoTracking()
            .Where(pm => pm.UsuarioId == userId.Value)
            .Select(pm => pm.ProyectoId)
            .ToHashSet();

        var selector = proyectoIdSelector.Compile();
        return query.Where(e => proyectoIds.Contains(selector(e))).AsQueryable();
    }
}
