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
    [Authorize(Policy = "roles.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioListDto>>>> GetAll()
    {
        var users = await _userManager.Users
            .OrderByDescending(u => u.Activo)
            .ThenBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .ToListAsync();

        var data = new List<UsuarioListDto>();
        foreach (var user in users)
            data.Add(await MapListDtoAsync(user));

        return Ok(new ApiResponse<IEnumerable<UsuarioListDto>> { Success = true, Data = data });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "roles.ver")]
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
    [Authorize(Policy = "roles.crear")]
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
            ? dto.Correo.Split('@')[0] 
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
    [Authorize(Policy = "roles.editar")]
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
    [Authorize(Policy = "roles.editar")]
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
    [Authorize(Policy = "roles.editar")]
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

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "roles.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Usuario no encontrado" });

        if (user.Activo && await IsArchitectAsync(user) && !await HasAnotherActiveArchitectAsync(user.Id))
            return Conflict(new ApiResponse<object> { Success = false, Message = "No se puede eliminar el último usuario activo con rol Arquitecto" });

        user.Activo = false;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(ToErrorResponse(result, "No se pudo eliminar el usuario"));

        return Ok(new ApiResponse<object> { Success = true, Message = "Usuario eliminado exitosamente" });
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
