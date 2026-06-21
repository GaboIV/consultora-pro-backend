using System.Linq.Expressions;

namespace ConsultoraPro.Application.Interfaces;

public interface IProjectScope
{
    bool VeTodos(string modulo);

    Task<IReadOnlySet<Guid>> ProyectosAsignadosAsync();

    bool EsMiembro(Guid proyectoId);

    IQueryable<T> FiltrarPorAcceso<T>(IQueryable<T> query,
        Expression<Func<T, Guid>> proyectoIdSelector, string modulo);
}
