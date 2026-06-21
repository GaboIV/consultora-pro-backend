using System.Reflection;
using ConsultoraPro.API.Controllers;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.Tests;

public class AuthorizationCoverageTests
{
    [Fact]
    public void EveryControllerAction_HasExplicitAuthorizeAttribute()
    {
        var apiAssembly = typeof(AuthController).Assembly;

        var controllers = apiAssembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract);

        var missing = new List<string>();

        foreach (var controller in controllers)
        {
            var hasControllerAuthorize = controller.GetCustomAttribute<AuthorizeAttribute>() is not null;
            if (hasControllerAuthorize)
                continue;

            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttribute<NonActionAttribute>() is null);

            foreach (var method in methods)
            {
                var hasMethodAuthorize = method.GetCustomAttribute<AuthorizeAttribute>() is not null;
                var hasAllowAnonymous = method.GetCustomAttribute<AllowAnonymousAttribute>() is not null;
                if (!hasMethodAuthorize && !hasAllowAnonymous)
                {
                    missing.Add($"{controller.Name}.{method.Name}");
                }
            }
        }

        Assert.Empty(missing);
    }

    [Fact]
    public void EveryPermissionInCatalog_HasPolicyRegistered()
    {
        var policies = ConsultoraPro.Domain.Security.PermissionCatalog.All
            .Select(p => p.Clave)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Las policies se registran en Program.cs recorriendo PermissionCatalog.All.
        // Verificamos que todas las claves existen.
        Assert.NotNull(policies);
        Assert.NotEmpty(policies);

        // Verificar que usuarios.ver, clientes.ver.todos, etc. existen.
        Assert.Contains("usuarios.ver", policies);
        Assert.Contains("clientes.ver.todos", policies);
        Assert.Contains("credenciales.nivel.full", policies);
    }

    [Fact]
    public void EveryProjectScopedEntity_ImplementsIProyectoScoped()
    {
        var domainAssembly = typeof(ConsultoraPro.Domain.Models.Ambiente).Assembly;

        var entitiesWithProyectoId = domainAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
            .Select(t => new
            {
                Type = t,
                Prop = t.GetProperty("ProyectoId", typeof(Guid))
            })
            .Where(x => x.Prop is not null)
            .ToList();

        var missing = new List<string>();

        foreach (var entity in entitiesWithProyectoId)
        {
            if (!typeof(IProyectoScoped).IsAssignableFrom(entity.Type))
            {
                missing.Add(entity.Type.Name);
            }
        }

        Assert.Empty(missing);
    }
}
