namespace ConsultoraPro.Application.Interfaces;

/// <summary>
/// Centraliza la regla de ámbito por proyecto del módulo Kanban: los roles sin acceso total
/// a "proyectos" solo pueden operar sobre tableros de proyectos a los que están asignados.
/// </summary>
public interface IKanbanAccessGuard
{
    /// <summary>Indica si el usuario actual puede acceder al tablero indicado.</summary>
    Task<bool> HasTableroAccessAsync(Guid tableroId);

    /// <summary>Lanza <see cref="UnauthorizedAccessException"/> si el usuario actual no puede acceder al tablero.</summary>
    Task EnsureTableroAccessAsync(Guid tableroId);
}
