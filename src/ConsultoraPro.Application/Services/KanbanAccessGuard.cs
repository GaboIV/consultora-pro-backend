using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;

namespace ConsultoraPro.Application.Services;

public class KanbanAccessGuard : IKanbanAccessGuard
{
    private readonly ITableroRepository _tableroRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly ICurrentUserService _currentUserService;

    public KanbanAccessGuard(
        ITableroRepository tableroRepository,
        IProyectoRepository proyectoRepository,
        ICurrentUserService currentUserService)
    {
        _tableroRepository = tableroRepository;
        _proyectoRepository = proyectoRepository;
        _currentUserService = currentUserService;
    }

    public async Task<bool> HasTableroAccessAsync(Guid tableroId)
    {
        if (_currentUserService.HasFullProjectAccessFor("proyectos"))
            return true;

        var tablero = await _tableroRepository.GetByIdAsync(tableroId);

        // Tableros personales (sin proyecto) no aplican el ámbito por proyecto; el acceso se rige
        // por el permiso de Kanban del controlador, igual que antes.
        if (tablero?.ProyectoId is not { } proyectoId)
            return true;

        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId);
        return proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == _currentUserService.UserId) ?? false;
    }

    public async Task EnsureTableroAccessAsync(Guid tableroId)
    {
        if (!await HasTableroAccessAsync(tableroId))
            throw new UnauthorizedAccessException("No tienes acceso a este tablero.");
    }
}
