using System.Text.Json;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace ConsultoraPro.API.Middleware;

public class TokenFreshnessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenFreshnessMiddleware> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    public TokenFreshnessMiddleware(RequestDelegate next, ILogger<TokenFreshnessMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager, IMemoryCache cache)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claimValue = context.User.FindFirst("permVersion")?.Value;
            if (claimValue is not null && int.TryParse(claimValue, out var tokenPermVersion))
            {
                var userIdClaim = context.User.FindFirst("userId")?.Value;
                if (userIdClaim is not null && Guid.TryParse(userIdClaim, out var userId))
                {
                    var dbPermVersion = await GetCurrentPermVersionAsync(userId, userManager, roleManager, cache);
                    if (tokenPermVersion != dbPermVersion)
                    {
                        _logger.LogInformation("Token obsoleto para usuario {UserId}: token={TokenVersion} db={DbVersion}",
                            userId, tokenPermVersion, dbPermVersion);

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        var payload = new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Token obsoleto",
                            Errors = ["token-stale"]
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));
                        return;
                    }
                }
            }
        }

        await _next(context);
    }

    private static async Task<int> GetCurrentPermVersionAsync(Guid userId,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMemoryCache cache)
    {
        var cacheKey = $"permVersion_{userId}";
        if (cache.TryGetValue(cacheKey, out int cached))
            return cached;

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return 0;

        var roleName = (await userManager.GetRolesAsync(user)).FirstOrDefault();
        var rolePermVersion = 0;
        if (roleName is not null)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is not null)
                rolePermVersion = role.PermVersion;
        }

        var dbPermVersion = user.PermVersion ^ rolePermVersion;

        cache.Set(cacheKey, dbPermVersion, CacheTtl);
        return dbPermVersion;
    }
}
