using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Security;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Security;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AppDbContext _context;

    public UsuariosController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = "usuarios.ver")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<UsuarioListDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? rol = null)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(rol))
        {
            query = from u in query
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where r.Name == rol
                    select u;
        }

        query = query.OrderByDescending(u => u.Activo)
                     .ThenBy(u => u.Nombres)
                     .ThenBy(u => u.Apellidos);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var list = new List<UsuarioListDto>();
        foreach (var user in items)
            list.Add(await MapListDtoAsync(user));

        var data = new PagedResultDto<UsuarioListDto>
        {
            Data = list,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };

        return Ok(new ApiResponse<PagedResultDto<UsuarioListDto>> { Success = true, Data = data });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "usuarios.ver")]
    public async Task<ActionResult<ApiResponse<UsuarioDetalleDto>>> GetById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<UsuarioDetalleDto> { Success = false, Message = "Usuario no encontrado" });

        var role = await GetSingleRoleAsync(user);
        var permisos = role is null ? Array.Empty<string>() : await GetGrantedPermissionKeysAsync(role.Id);
        var list = await MapListDtoAsync(user);

        return Ok(new ApiResponse<UsuarioDetalleDto>
        {
            Success = true,
            Data = new UsuarioDetalleDto
            {
                Id = list.Id,
                Nombres = list.Nombres,
                Apellidos = list.Apellidos,
                Correo = list.Correo,
                Telefono = list.Telefono,
                Iniciales = list.Iniciales,
                Puesto = list.Puesto,
                RolId = list.RolId,
                Rol = list.Rol,
                Activo = list.Activo,
                FechaAlta = list.FechaAlta,
                UltimoAcceso = list.UltimoAcceso,
                Permisos = permisos
            }
        });
    }

    [HttpPost]
    [Authorize(Policy = "usuarios.editar")]
    public async Task<ActionResult<ApiResponse<UsuarioListDto>>> Create([FromBody] CreateUsuarioDto dto)
    {
        var validation = ValidateUsuario(dto.Nombres, dto.Apellidos, dto.Correo);
        if (validation.Count > 0)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Datos inválidos", Errors = validation });

        if (await _userManager.FindByEmailAsync(dto.Correo) is not null)
            return Conflict(new ApiResponse<object> { Success = false, Message = "El correo ya está registrado" });

        var role = await FindRoleByIdAsync(dto.RolId);
        if (role is null)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Rol inválido" });

        var user = new ApplicationUser
        {
            UserName = dto.Correo,
            Email = dto.Correo,
            Nombres = dto.Nombres.Trim(),
            Apellidos = dto.Apellidos.Trim(),
            Telefono = dto.Telefono.Trim(),
            PhoneNumber = dto.Telefono.Trim(),
            Iniciales = BuildInitials(dto.Nombres, dto.Apellidos, dto.Iniciales),
            Puesto = role.Name ?? string.Empty,
            Activo = true,
            EmailConfirmed = true,
            FechaAlta = DateTime.UtcNow
        };

        var password = string.IsNullOrWhiteSpace(dto.Password)
            ? BuildDefaultPassword(dto.Correo)
            : dto.Password;

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return BadRequest(ToErrorResponse(createResult, "No se pudo crear el usuario"));

        var roleResult = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!roleResult.Succeeded)
            return BadRequest(ToErrorResponse(roleResult, "No se pudo asignar el rol"));

        var data = await MapListDtoAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new ApiResponse<UsuarioListDto>
        {
            Success = true,
            Data = data,
            Message = "Usuario creado exitosamente"
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "usuarios.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateUsuarioDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Usuario no encontrado" });

        var validation = ValidateUsuario(dto.Nombres, dto.Apellidos, dto.Correo);
        if (validation.Count > 0)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Datos inválidos", Errors = validation });

        var existingEmail = await _userManager.FindByEmailAsync(dto.Correo);
        if (existingEmail is not null && existingEmail.Id != id)
            return Conflict(new ApiResponse<object> { Success = false, Message = "El correo ya está registrado" });

        var role = await FindRoleByIdAsync(dto.RolId);
        if (role is null)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Rol inválido" });

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentlyArchitect = currentRoles.Contains(PermissionCatalog.Arquitecto, StringComparer.OrdinalIgnoreCase);
        var willStayArchitect = string.Equals(role.Name, PermissionCatalog.Arquitecto, StringComparison.OrdinalIgnoreCase);
        if (user.Activo && currentlyArchitect && !willStayArchitect && !await HasAnotherActiveArchitectAsync(user.Id))
            return Conflict(new ApiResponse<object> { Success = false, Message = "Debe existir al menos un usuario activo con rol Arquitecto" });

        user.UserName = dto.Correo.Trim();
        user.Email = dto.Correo.Trim();
        user.Nombres = dto.Nombres.Trim();
        user.Apellidos = dto.Apellidos.Trim();
        user.Telefono = dto.Telefono.Trim();
        user.PhoneNumber = dto.Telefono.Trim();
        user.Iniciales = BuildInitials(dto.Nombres, dto.Apellidos, dto.Iniciales);
        user.Puesto = role.Name ?? string.Empty;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return BadRequest(ToErrorResponse(updateResult, "No se pudo actualizar el usuario"));

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return BadRequest(ToErrorResponse(removeResult, "No se pudieron actualizar los roles"));
        }

        var addResult = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!addResult.Succeeded)
            return BadRequest(ToErrorResponse(addResult, "No se pudo asignar el rol"));

        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario actualizado exitosamente" });
    }

    [HttpPut("{id:guid}/password")]
    [Authorize(Policy = "usuarios.cambiar-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(Guid id, [FromBody] UpdateUsuarioPasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Usuario no encontrado" });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.Password);
        if (!result.Succeeded)
            return BadRequest(ToErrorResponse(result, "No se pudo cambiar la contraseña"));

        return Ok(new ApiResponse<object> { Success = true, Message = "Contraseña actualizada exitosamente" });
    }

    [HttpPut("{id:guid}/toggle")]
    [Authorize(Policy = "usuarios.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Toggle(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Usuario no encontrado" });

        if (user.Activo && await IsArchitectAsync(user) && !await HasAnotherActiveArchitectAsync(user.Id))
            return Conflict(new ApiResponse<object> { Success = false, Message = "Debe existir al menos un usuario activo con rol Arquitecto" });

        user.Activo = !user.Activo;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(ToErrorResponse(result, "No se pudo cambiar el estado del usuario"));

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = user.Activo ? "Usuario activado exitosamente" : "Usuario desactivado exitosamente"
        });
    }

    [HttpPut("{id:guid}/desactivar")]
    [Authorize(Policy = "usuarios.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Desactivar(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Usuario no encontrado" });

        if (user.Activo && await IsArchitectAsync(user) && !await HasAnotherActiveArchitectAsync(user.Id))
            return Conflict(new ApiResponse<object> { Success = false, Message = "Debe existir al menos un usuario activo con rol Arquitecto" });

        user.Activo = false;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(ToErrorResponse(result, "No se pudo desactivar el usuario"));

        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario desactivado exitosamente" });
    }

    [HttpPut("{id:guid}/activar")]
    [Authorize(Policy = "usuarios.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Activar(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Usuario no encontrado" });

        user.Activo = true;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(ToErrorResponse(result, "No se pudo activar el usuario"));

        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario activado exitosamente" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "usuarios.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Usuario no encontrado" });

        if (await IsArchitectAsync(user) && !await HasAnotherActiveArchitectAsync(user.Id))
            return Conflict(new ApiResponse<object> { Success = false, Message = "No se puede eliminar el último usuario con rol Arquitecto" });

        try
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(ToErrorResponse(result, "No se pudo eliminar el usuario"));

            return Ok(new ApiResponse<object> { Success = true, Message = "Usuario eliminado definitivamente" });
        }
        catch (DbUpdateException)
        {
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = "No se puede eliminar el usuario porque tiene actividades o registros asociados en el sistema. Considere desactivarlo."
            });
        }
    }

    [HttpGet("{id:guid}/proyectos")]
    [Authorize(Policy = "usuarios.asignar-proyectos")]
    public async Task<ActionResult<ApiResponse<UsuarioProyectosAccesoDto>>> GetProyectos(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<UsuarioProyectosAccesoDto> { Success = false, Message = "Usuario no encontrado" });

        var role = await GetSingleRoleAsync(user);
        var accesoTotal = role?.AccesoTotalProyectos ?? false;

        var asignados = (await _context.ProyectoMiembros
            .AsNoTracking()
            .Where(pm => pm.UsuarioId == id)
            .Select(pm => pm.ProyectoId)
            .ToListAsync())
            .ToHashSet();

        var proyectos = await _context.Proyectos
            .AsNoTracking()
            .Include(p => p.Cliente)
            .OrderBy(p => p.Nombre)
            .Select(p => new UsuarioProyectoAccesoDto
            {
                ProyectoId = p.Id,
                Nombre = p.Nombre,
                Clave = p.Clave,
                Cliente = p.Cliente.Nombre,
                Asignado = asignados.Contains(p.Id)
            })
            .ToListAsync();

        return Ok(new ApiResponse<UsuarioProyectosAccesoDto>
        {
            Success = true,
            Data = new UsuarioProyectosAccesoDto { AccesoTotal = accesoTotal, Proyectos = proyectos }
        });
    }

    [HttpPut("{id:guid}/proyectos")]
    [Authorize(Policy = "usuarios.asignar-proyectos")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProyectos(Guid id, [FromBody] UpdateUsuarioProyectosDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Usuario no encontrado" });

        var seleccionados = dto.ProyectoIds.Distinct().ToHashSet();
        if (seleccionados.Count > 0)
        {
            var existentes = await _context.Proyectos
                .Where(p => seleccionados.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();
            if (existentes.Count != seleccionados.Count)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "La lista contiene proyectos inválidos" });
        }

        var actuales = await _context.ProyectoMiembros
            .Where(pm => pm.UsuarioId == id)
            .ToListAsync();
        var actualesPorProyecto = actuales.ToDictionary(pm => pm.ProyectoId);

        var afectados = new HashSet<Guid>();

        // Quitar accesos que ya no están seleccionados.
        foreach (var miembro in actuales.Where(pm => !seleccionados.Contains(pm.ProyectoId)))
        {
            _context.ProyectoMiembros.Remove(miembro);
            afectados.Add(miembro.ProyectoId);
        }

        // Agregar accesos nuevos.
        foreach (var proyectoId in seleccionados.Where(pid => !actualesPorProyecto.ContainsKey(pid)))
        {
            _context.ProyectoMiembros.Add(new ProyectoMiembro
            {
                Id = Guid.NewGuid(),
                UsuarioId = id,
                ProyectoId = proyectoId,
                Rol = RolDesarrollador.Apoyo,
                FechaAsignacion = DateTime.UtcNow
            });
            afectados.Add(proyectoId);
        }

        if (afectados.Count > 0)
        {
            await _context.SaveChangesAsync();
            await RefreshTotalMiembrosAsync(afectados);
        }

        return Ok(new ApiResponse<object> { Success = true, Message = "Acceso a proyectos actualizado exitosamente" });
    }

    private async Task RefreshTotalMiembrosAsync(IEnumerable<Guid> proyectoIds)
    {
        var ids = proyectoIds.ToList();
        var conteos = await _context.ProyectoMiembros
            .Where(pm => ids.Contains(pm.ProyectoId))
            .GroupBy(pm => pm.ProyectoId)
            .Select(g => new { ProyectoId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.ProyectoId, x => x.Total);

        var proyectos = await _context.Proyectos.Where(p => ids.Contains(p.Id)).ToListAsync();
        foreach (var proyecto in proyectos)
            proyecto.TotalMiembros = conteos.TryGetValue(proyecto.Id, out var total) ? total : 0;

        await _context.SaveChangesAsync();
    }

    private async Task<UsuarioListDto> MapListDtoAsync(ApplicationUser user)
    {
        var role = await GetSingleRoleAsync(user);
        return new UsuarioListDto
        {
            Id = user.Id,
            Nombres = user.Nombres,
            Apellidos = user.Apellidos,
            Correo = user.Email ?? string.Empty,
            Telefono = user.Telefono,
            Iniciales = user.Iniciales,
            Puesto = role?.Name ?? user.Puesto,
            RolId = role?.Id,
            Rol = role?.Name ?? string.Empty,
            Activo = user.Activo,
            FechaAlta = user.FechaAlta,
            UltimoAcceso = user.UltimoAcceso
        };
    }

    private async Task<ApplicationRole?> GetSingleRoleAsync(ApplicationUser user)
    {
        var roleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
        return string.IsNullOrWhiteSpace(roleName) ? null : await _roleManager.FindByNameAsync(roleName);
    }

    private async Task<ApplicationRole?> FindRoleByIdAsync(Guid roleId)
    {
        return await _roleManager.Roles.FirstOrDefaultAsync(role => role.Id == roleId && role.EsActivo);
    }

    private async Task<IReadOnlyList<string>> GetGrantedPermissionKeysAsync(Guid roleId)
    {
        return await _context.RolPermisos
            .AsNoTracking()
            .Where(rp => rp.RolId == roleId && rp.Concedido)
            .OrderBy(rp => rp.Permiso.Modulo)
            .ThenBy(rp => rp.Permiso.Clave)
            .Select(rp => rp.Permiso.Clave)
            .ToListAsync();
    }

    private async Task<bool> IsArchitectAsync(ApplicationUser user)
    {
        return await _userManager.IsInRoleAsync(user, PermissionCatalog.Arquitecto);
    }

    private async Task<bool> HasAnotherActiveArchitectAsync(Guid currentUserId)
    {
        var architects = await _userManager.GetUsersInRoleAsync(PermissionCatalog.Arquitecto);
        return architects.Any(user => user.Id != currentUserId && user.Activo);
    }

    private static List<string> ValidateUsuario(string nombres, string apellidos, string correo)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(nombres)) errors.Add("Los nombres son obligatorios.");
        if (string.IsNullOrWhiteSpace(apellidos)) errors.Add("Los apellidos son obligatorios.");
        if (string.IsNullOrWhiteSpace(correo)) errors.Add("El correo es obligatorio.");
        if (!correo.Contains('@', StringComparison.Ordinal)) errors.Add("El correo no tiene un formato válido.");
        return errors;
    }

    private static string BuildDefaultPassword(string correo)
    {
        var prefix = correo.Split('@')[0];
        return prefix.Length < 8 ? prefix.PadRight(8, '0') : prefix;
    }

    private static string BuildInitials(string nombres, string apellidos, string? iniciales)
    {
        if (!string.IsNullOrWhiteSpace(iniciales))
            return iniciales.Trim().ToUpperInvariant()[..Math.Min(2, iniciales.Trim().Length)];

        var first = string.IsNullOrWhiteSpace(nombres) ? string.Empty : nombres.Trim()[0].ToString();
        var second = string.IsNullOrWhiteSpace(apellidos) ? string.Empty : apellidos.Trim()[0].ToString();
        return $"{first}{second}".ToUpperInvariant();
    }

    private static ApiResponse<object> ToErrorResponse(IdentityResult result, string message)
    {
        return new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = result.Errors.Select(error => error.Description).ToList()
        };
    }
}
