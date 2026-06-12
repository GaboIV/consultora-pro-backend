using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Data.Seed;

public static class SecuritySeeder
{
    public static async Task SeedPermisosAsync(AppDbContext context)
    {
        foreach (var definition in PermissionCatalog.All)
        {
            var permiso = await context.Permisos.FindAsync(definition.Id);
            if (permiso is null)
            {
                context.Permisos.Add(new Permiso
                {
                    Id = definition.Id,
                    Clave = definition.Clave,
                    Nombre = definition.Nombre,
                    Modulo = definition.Modulo,
                    Descripcion = definition.Descripcion
                });
                continue;
            }

            permiso.Clave = definition.Clave;
            permiso.Nombre = definition.Nombre;
            permiso.Modulo = definition.Modulo;
            permiso.Descripcion = definition.Descripcion;
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var roleName in PermissionCatalog.RolePermissions.Keys)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                var result = await roleManager.CreateAsync(new ApplicationRole(roleName)
                {
                    Descripcion = PermissionCatalog.RoleDescriptions[roleName],
                    EsActivo = true
                });
                ThrowIfFailed(result, $"No se pudo crear el rol {roleName}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(role.Descripcion))
                role.Descripcion = PermissionCatalog.RoleDescriptions[roleName];
            role.EsActivo = true;
            var updateResult = await roleManager.UpdateAsync(role);
            ThrowIfFailed(updateResult, $"No se pudo actualizar el rol {roleName}");
        }
    }

    public static async Task SeedRolPermisosAsync(AppDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        var permisos = await context.Permisos.ToListAsync();

        foreach (var (roleName, grantedKeys) in PermissionCatalog.RolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
                continue;

            var existing = await context.RolPermisos
                .Where(rp => rp.RolId == role.Id)
                .ToDictionaryAsync(rp => rp.PermisoId);

            foreach (var permiso in permisos)
            {
                var granted = grantedKeys.Contains(permiso.Clave);
                if (!existing.ContainsKey(permiso.Id))
                {
                    context.RolPermisos.Add(new RolPermiso
                    {
                        RolId = role.Id,
                        PermisoId = permiso.Id,
                        Concedido = granted
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedDefaultUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        const string email = "gcaraballo@equaly.pe";
        const string roleName = PermissionCatalog.Arquitecto;

        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
            throw new InvalidOperationException($"El rol {roleName} debe existir antes de crear el usuario por defecto.");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Nombres = "Gabriel",
                Apellidos = "Caraballo",
                Telefono = string.Empty,
                Iniciales = "GC",
                Puesto = "Arquitecto de Soluciones Web",
                Activo = true,
                EmailConfirmed = true,
                FechaAlta = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(user, "adminGabo2208");
            ThrowIfFailed(createResult, "No se pudo crear el usuario por defecto.");
        }
        else
        {
            user.Nombres = "Gabriel";
            user.Apellidos = "Caraballo";
            user.Iniciales = "GC";
            user.Puesto = "Arquitecto de Soluciones Web";
            user.Activo = true;
            user.EmailConfirmed = true;
            var updateResult = await userManager.UpdateAsync(user);
            ThrowIfFailed(updateResult, "No se pudo actualizar el usuario por defecto.");
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                ThrowIfFailed(removeResult, "No se pudieron limpiar los roles del usuario por defecto.");
            }

            var addResult = await userManager.AddToRoleAsync(user, roleName);
            ThrowIfFailed(addResult, "No se pudo asignar el rol Arquitecto al usuario por defecto.");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (result.Succeeded)
            return;

        throw new InvalidOperationException($"{message} {string.Join("; ", result.Errors.Select(e => e.Description))}");
    }
}
