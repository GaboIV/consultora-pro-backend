using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Security;
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
public class RolesController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public RolesController(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = "roles.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RolListDto>>>> GetAll()
    {
        var roles = await _roleManager.Roles
            .OrderBy(role => role.Name)
            .ToListAsync();

        var data = new List<RolListDto>();
        foreach (var role in roles)
            data.Add(await MapRoleAsync(role, includeOnlyGranted: true));

        return Ok(new ApiResponse<IEnumerable<RolListDto>> { Success = true, Data = data });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "roles.ver")]
    public async Task<ActionResult<ApiResponse<RolDetalleDto>>> GetById(Guid id)
    {
        var role = await FindRoleByIdAsync(id);
        if (role is null)
            return NotFound(new ApiResponse<RolDetalleDto> { Success = false, Message = "Rol no encontrado" });

        var list = await MapRoleAsync(role, includeOnlyGranted: false);
        var permisosIds = await _context.RolPermisos
            .AsNoTracking()
            .Where(rp => rp.RolId == role.Id && rp.Concedido)
            .Select(rp => rp.PermisoId)
            .ToListAsync();

        return Ok(new ApiResponse<RolDetalleDto>
        {
            Success = true,
            Data = new RolDetalleDto
            {
                Id = list.Id,
                Nombre = list.Nombre,
                Descripcion = list.Descripcion,
                EsActivo = list.EsActivo,
                UsuariosCount = list.UsuariosCount,
                Permisos = list.Permisos,
                PermisosIds = permisosIds
            }
        });
    }

    [HttpPost]
    [Authorize(Policy = "roles.crear")]
    public async Task<ActionResult<ApiResponse<RolListDto>>> Create([FromBody] CreateRolDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ApiResponse<object> { Success = false, Message = "El nombre del rol es obligatorio" });

        if (await _roleManager.RoleExistsAsync(dto.Nombre))
            return Conflict(new ApiResponse<object> { Success = false, Message = "Ya existe un rol con ese nombre" });

        var role = new ApplicationRole(dto.Nombre.Trim())
        {
            Descripcion = dto.Descripcion.Trim(),
            EsActivo = true
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(ToErrorResponse(result, "No se pudo crear el rol"));

        var data = await MapRoleAsync(role, includeOnlyGranted: true);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, new ApiResponse<RolListDto>
        {
            Success = true,
            Data = data,
            Message = "Rol creado exitosamente"
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "roles.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateRolDto dto)
    {
        var role = await FindRoleByIdAsync(id);
        if (role is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Rol no encontrado" });

        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new ApiResponse<object> { Success = false, Message = "El nombre del rol es obligatorio" });

        if (string.Equals(role.Name, PermissionCatalog.Arquitecto, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(dto.Nombre.Trim(), PermissionCatalog.Arquitecto, StringComparison.OrdinalIgnoreCase))
            return Conflict(new ApiResponse<object> { Success = false, Message = "No se puede renombrar el rol Arquitecto" });

        var existing = await _roleManager.FindByNameAsync(dto.Nombre.Trim());
        if (existing is not null && existing.Id != id)
            return Conflict(new ApiResponse<object> { Success = false, Message = "Ya existe un rol con ese nombre" });

        role.Name = dto.Nombre.Trim();
        role.Descripcion = dto.Descripcion.Trim();
        role.EsActivo = dto.EsActivo;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return BadRequest(ToErrorResponse(result, "No se pudo actualizar el rol"));

        return Ok(new ApiResponse<object> { Success = true, Message = "Rol actualizado exitosamente" });
    }

    [HttpPut("{id:guid}/permisos")]
    [Authorize(Policy = "roles.editar")]
    public async Task<ActionResult<ApiResponse<object>>> UpdatePermisos(Guid id, [FromBody] UpdateRolPermisosDto dto)
    {
        var role = await FindRoleByIdAsync(id);
        if (role is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Rol no encontrado" });

        var catalogIds = await _context.Permisos.Select(p => p.Id).ToListAsync();
        var invalidIds = dto.PermisosIds.Except(catalogIds).ToList();
        if (invalidIds.Count > 0)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "La lista contiene permisos inválidos" });

        var selected = dto.PermisosIds.ToHashSet();
        var existing = await _context.RolPermisos
            .Where(rp => rp.RolId == role.Id)
            .ToDictionaryAsync(rp => rp.PermisoId);

        foreach (var permisoId in catalogIds)
        {
            if (existing.TryGetValue(permisoId, out var rolPermiso))
            {
                rolPermiso.Concedido = selected.Contains(permisoId);
            }
            else
            {
                _context.RolPermisos.Add(new RolPermiso
                {
                    RolId = role.Id,
                    PermisoId = permisoId,
                    Concedido = selected.Contains(permisoId)
                });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object> { Success = true, Message = "Permisos actualizados exitosamente" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "roles.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var role = await FindRoleByIdAsync(id);
        if (role is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Rol no encontrado" });

        if (string.Equals(role.Name, PermissionCatalog.Arquitecto, StringComparison.OrdinalIgnoreCase))
            return Conflict(new ApiResponse<object> { Success = false, Message = "No se puede eliminar el rol Arquitecto" });

        var assignedUsers = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (assignedUsers.Count > 0)
            return Conflict(new ApiResponse<object> { Success = false, Message = "No se puede eliminar un rol con usuarios asignados" });

        var rolePermissions = await _context.RolPermisos.Where(rp => rp.RolId == id).ToListAsync();
        _context.RolPermisos.RemoveRange(rolePermissions);

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(ToErrorResponse(result, "No se pudo eliminar el rol"));

        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object> { Success = true, Message = "Rol eliminado exitosamente" });
    }

    private async Task<ApplicationRole?> FindRoleByIdAsync(Guid id)
    {
        return await _roleManager.Roles.FirstOrDefaultAsync(role => role.Id == id);
    }

    private async Task<RolListDto> MapRoleAsync(ApplicationRole role, bool includeOnlyGranted)
    {
        var query = _context.Permisos
            .AsNoTracking()
            .GroupJoin(
                _context.RolPermisos.AsNoTracking().Where(rp => rp.RolId == role.Id),
                permiso => permiso.Id,
                rolPermiso => rolPermiso.PermisoId,
                (permiso, rolPermisos) => new
                {
                    Permiso = permiso,
                    RolPermiso = rolPermisos.FirstOrDefault()
                });

        var permisos = await query
            .Where(row => !includeOnlyGranted || (row.RolPermiso != null && row.RolPermiso.Concedido))
            .Select(row => new PermisoDto
            {
                Id = row.Permiso.Id,
                Clave = row.Permiso.Clave,
                Nombre = row.Permiso.Nombre,
                Modulo = row.Permiso.Modulo,
                Descripcion = row.Permiso.Descripcion,
                Concedido = row.RolPermiso != null && row.RolPermiso.Concedido
            })
            .OrderBy(permiso => permiso.Modulo)
            .ThenBy(permiso => permiso.Clave)
            .ToListAsync();

        var users = string.IsNullOrWhiteSpace(role.Name)
            ? Array.Empty<ApplicationUser>()
            : await _userManager.GetUsersInRoleAsync(role.Name);

        return new RolListDto
        {
            Id = role.Id,
            Nombre = role.Name ?? string.Empty,
            Descripcion = role.Descripcion,
            EsActivo = role.EsActivo,
            UsuariosCount = users.Count,
            Permisos = permisos
                .GroupBy(permiso => permiso.Modulo)
                .Select(group => new PermisoModuloDto
                {
                    Modulo = group.Key,
                    Permisos = group.ToList()
                })
                .ToList()
        };
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
