using System.Text.Json;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Authorization;

namespace ConsultoraPro.API.Authorization;

public sealed class ProjectResourceHandler : AuthorizationHandler<ModuloOperationRequirement, IProyectoScoped>
{
    private readonly IProjectScope _scope;

    public ProjectResourceHandler(IProjectScope scope)
    {
        _scope = scope;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuloOperationRequirement requirement,
        IProyectoScoped resource)
    {
        var permissions = context.User.FindAll("permisos")
            .SelectMany(ReadPermissionClaim)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var tienePermiso = permissions.Contains(requirement.Clave);
        if (!tienePermiso)
            return Task.CompletedTask;

        var veTodos = _scope.VeTodos(requirement.Modulo);
        var esMiembro = _scope.EsMiembro(resource.ProyectoId);

        if (veTodos || esMiembro)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }

    private static IEnumerable<string> ReadPermissionClaim(System.Security.Claims.Claim claim)
    {
        if (string.IsNullOrWhiteSpace(claim.Value))
            return [];

        if (!claim.Value.TrimStart().StartsWith("[", StringComparison.Ordinal))
            return [claim.Value];

        try
        {
            return JsonSerializer.Deserialize<string[]>(claim.Value) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
