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
            var accesoTotal = PermissionCatalog.FullProjectAccessRoles.Contains(roleName);
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                var result = await roleManager.CreateAsync(new ApplicationRole(roleName)
                {
                    Descripcion = PermissionCatalog.RoleDescriptions[roleName],
                    EsActivo = true,
                    AccesoTotalProyectos = accesoTotal,
                    EsSistema = true
                });
                ThrowIfFailed(result, $"No se pudo crear el rol {roleName}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(role.Descripcion))
                role.Descripcion = PermissionCatalog.RoleDescriptions[roleName];
            role.EsActivo = true;
            // Los flags estructurales de los roles de sistema se imponen siempre desde el seeder.
            role.AccesoTotalProyectos = accesoTotal;
            role.EsSistema = true;
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

    /// <summary>
    /// Migra las concesiones de claves legacy a las nuevas claves granulares para roles ya existentes
    /// (especialmente los personalizados, que el seeder de defaults no toca). Es idempotente y solo
    /// AGREGA concesiones que falten: nunca revoca, de modo que respeta los ajustes manuales de un admin.
    /// </summary>
    public static async Task MigrateLegacyGrantsAsync(AppDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        var permisos = await context.Permisos.AsNoTracking().ToListAsync();
        var idByClave = permisos.ToDictionary(p => p.Clave, p => p.Id, StringComparer.OrdinalIgnoreCase);
        var claveById = permisos.ToDictionary(p => p.Id, p => p.Clave);

        var roles = await roleManager.Roles.ToListAsync();
        var added = false;

        foreach (var role in roles)
        {
            var rolePermisos = await context.RolPermisos
                .Where(rp => rp.RolId == role.Id)
                .ToListAsync();

            var existingPermisoIds = rolePermisos.Select(rp => rp.PermisoId).ToHashSet();
            var granted = rolePermisos
                .Where(rp => rp.Concedido && claveById.ContainsKey(rp.PermisoId))
                .Select(rp => claveById[rp.PermisoId])
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var clave in DeriveNewGrants(granted, role.AccesoTotalProyectos))
            {
                if (!idByClave.TryGetValue(clave, out var permisoId))
                    continue;
                if (existingPermisoIds.Contains(permisoId))
                    continue; // ya tiene una fila (concedida o no): no se pisa la decisión existente.

                context.RolPermisos.Add(new RolPermiso
                {
                    RolId = role.Id,
                    PermisoId = permisoId,
                    Concedido = true
                });
                existingPermisoIds.Add(permisoId);
                added = true;
            }
        }

        if (added)
            await context.SaveChangesAsync();
    }

    // Traduce un conjunto de claves legacy concedidas a las nuevas claves equivalentes, preservando
    // el comportamiento previo (el ámbito "ver todos" se infiere del flag global del rol).
    private static IEnumerable<string> DeriveNewGrants(ISet<string> granted, bool accesoTotal)
    {
        var add = new List<string>();

        foreach (var modulo in PermissionCatalog.ScopedModules)
        {
            if (accesoTotal && granted.Contains($"{modulo}.ver"))
                add.Add($"{modulo}.ver.todos");
        }

        if (granted.Contains("credenciales.crear") || granted.Contains("credenciales.editar"))
            add.Add("credenciales.nivel.full");
        else if (granted.Contains("credenciales.revelar"))
            add.Add("credenciales.nivel.ver-todo");
        else if (granted.Contains("credenciales.ver"))
            add.Add("credenciales.nivel.basico");

        if (granted.Contains("roles.ver")) add.Add("usuarios.ver");
        if (granted.Contains("roles.crear")) add.Add("usuarios.editar");
        if (granted.Contains("roles.editar")) { add.Add("usuarios.editar"); add.Add("usuarios.cambiar-password"); }
        if (granted.Contains("roles.eliminar")) add.Add("usuarios.eliminar");
        if (granted.Contains("equipo.asignar-proyectos")) add.Add("usuarios.asignar-proyectos");

        if (granted.Contains("proyectos.editar")) { add.Add("screenshots.ver"); add.Add("screenshots.editar"); }
        else if (granted.Contains("proyectos.ver")) add.Add("screenshots.ver");

        return add;
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
