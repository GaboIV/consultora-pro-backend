using System;
using System.Linq;
using System.Security.Claims;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Security;
using Microsoft.AspNetCore.Http;

namespace ConsultoraPro.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("userId")?.Value ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var guid) ? guid : null;
        }
    }

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;

    public bool HasFullProjectAccess
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("accesoTotalProyectos")?.Value;
            if (!string.IsNullOrEmpty(claim))
                return string.Equals(claim, "true", StringComparison.OrdinalIgnoreCase);

            // Fallback para tokens emitidos antes de incorporar el claim: se infiere por nombre de rol.
            return Role is not null && PermissionCatalog.FullProjectAccessRoles.Contains(Role);
        }
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public bool HasPermission(string permission)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return false;

        var permissionsClaim = user.FindAll("permisos")
            .SelectMany(claim =>
            {
                if (string.IsNullOrWhiteSpace(claim.Value))
                    return [];
                if (!claim.Value.TrimStart().StartsWith("[", StringComparison.Ordinal))
                    return [claim.Value];
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<string[]>(claim.Value) ?? [];
                }
                catch
                {
                    return [];
                }
            })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return permissionsClaim.Contains(permission);
    }
}
