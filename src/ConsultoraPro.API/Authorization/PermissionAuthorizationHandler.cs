using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace ConsultoraPro.API.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permisos")
            .SelectMany(ReadPermissionClaim)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (permissions.Contains(requirement.Permission))
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
