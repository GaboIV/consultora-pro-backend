using System;

namespace ConsultoraPro.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }

    /// <summary>True si el rol del usuario actual tiene acceso total a todos los proyectos (flag global del rol).</summary>
    bool HasFullProjectAccess { get; }

    /// <summary>
    /// True si el usuario ve TODOS los recursos del módulo indicado (no solo los de proyectos asignados),
    /// ya sea por el flag global del rol o por el permiso <c>&lt;modulo&gt;.ver.todos</c>.
    /// </summary>
    bool HasFullProjectAccessFor(string modulo);

    bool IsInRole(string role);
    bool HasPermission(string permission);
}
