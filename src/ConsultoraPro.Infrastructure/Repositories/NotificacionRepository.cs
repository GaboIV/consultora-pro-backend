using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly AppDbContext _context;

    public NotificacionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddNotificacionesAsync(IEnumerable<Notificacion> notificaciones)
    {
        _context.Notificaciones.AddRange(notificaciones);
        await _context.SaveChangesAsync();
    }

    public async Task EnqueueCorreosAsync(IEnumerable<CorreoPendiente> correos)
    {
        foreach (var correo in correos)
        {
            CorreoPendiente? existente = null;
            if (!string.IsNullOrEmpty(correo.DedupKey))
            {
                existente = await _context.CorreosPendientes.FirstOrDefaultAsync(c =>
                    c.UsuarioId == correo.UsuarioId
                    && c.DedupKey == correo.DedupKey
                    && c.Estado == EstadoCorreo.Pendiente);
            }

            if (existente is not null)
            {
                // Mismo evento repetido dentro de la ventana: se actualiza el contenido pero se
                // conserva ProgramadoPara para que el resumen salga como máximo N min después
                // del primer evento (la ventana no se extiende indefinidamente).
                existente.Titulo = correo.Titulo;
                existente.Mensaje = correo.Mensaje;
                existente.Url = correo.Url;
            }
            else
            {
                _context.CorreosPendientes.Add(correo);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetUsuariosActivosAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        return await _context.Users
            .AsNoTracking()
            .Where(u => idList.Contains(u.Id) && u.Activo)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Guid>> GetUsuarioIdsConPermisoAsync(string permisoClave)
    {
        var rolIds = await _context.RolPermisos
            .AsNoTracking()
            .Where(rp => rp.Concedido && rp.Permiso.Clave == permisoClave)
            .Select(rp => rp.RolId)
            .ToListAsync();

        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => rolIds.Contains(ur.RoleId))
            .Join(_context.Users.Where(u => u.Activo), ur => ur.UserId, u => u.Id, (ur, u) => u.Id)
            .Distinct()
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Notificacion> Items, int Total)> GetByUsuarioAsync(
        Guid usuarioId, int page, int pageSize, bool soloNoLeidas)
    {
        var query = _context.Notificaciones
            .AsNoTracking()
            .Include(n => n.Actor)
            .Where(n => n.UsuarioId == usuarioId);

        if (soloNoLeidas)
            query = query.Where(n => !n.Leida);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<int> CountNoLeidasAsync(Guid usuarioId)
        => _context.Notificaciones.CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);

    public async Task<bool> MarcarLeidaAsync(Guid usuarioId, Guid notificacionId)
    {
        var notificacion = await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UsuarioId == usuarioId);
        if (notificacion is null)
            return false;

        if (!notificacion.Leida)
        {
            notificacion.Leida = true;
            notificacion.FechaLectura = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return true;
    }

    public async Task<int> MarcarTodasLeidasAsync(Guid usuarioId)
    {
        var ahora = DateTime.UtcNow;
        return await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && !n.Leida)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Leida, true)
                .SetProperty(n => n.FechaLectura, ahora));
    }

    public async Task<IReadOnlyList<PreferenciaNotificacion>> GetPreferenciasAsync(Guid usuarioId)
        => await _context.PreferenciasNotificacion
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

    public async Task<IReadOnlyList<PreferenciaNotificacion>> GetPreferenciasAsync(
        IEnumerable<Guid> usuarioIds, TipoNotificacion tipo)
    {
        var ids = usuarioIds.Distinct().ToList();
        return await _context.PreferenciasNotificacion
            .AsNoTracking()
            .Where(p => ids.Contains(p.UsuarioId) && p.Tipo == tipo)
            .ToListAsync();
    }

    public async Task UpsertPreferenciasAsync(Guid usuarioId, IEnumerable<PreferenciaNotificacion> preferencias)
    {
        var actuales = await _context.PreferenciasNotificacion
            .Where(p => p.UsuarioId == usuarioId)
            .ToDictionaryAsync(p => p.Tipo);

        foreach (var preferencia in preferencias)
        {
            if (actuales.TryGetValue(preferencia.Tipo, out var actual))
            {
                actual.EnApp = preferencia.EnApp;
                actual.PorCorreo = preferencia.PorCorreo;
            }
            else
            {
                _context.PreferenciasNotificacion.Add(new PreferenciaNotificacion
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuarioId,
                    Tipo = preferencia.Tipo,
                    EnApp = preferencia.EnApp,
                    PorCorreo = preferencia.PorCorreo
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<CorreoPendiente>> GetCorreosParaEnviarAsync(DateTime ahora, int maxIntentos, int lote)
        => await _context.CorreosPendientes
            .Include(c => c.Usuario)
            .Where(c => c.Estado == EstadoCorreo.Pendiente
                && c.ProgramadoPara <= ahora
                && c.Intentos < maxIntentos)
            .OrderBy(c => c.ProgramadoPara)
            .Take(lote)
            .ToListAsync();

    public async Task UpdateCorreosAsync(IEnumerable<CorreoPendiente> correos)
    {
        // Las filas llegan rastreadas desde GetCorreosParaEnviarAsync; basta con guardar.
        await _context.SaveChangesAsync();
    }
}
