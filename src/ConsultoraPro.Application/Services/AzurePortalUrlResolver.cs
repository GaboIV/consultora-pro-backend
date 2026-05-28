using System.Text.RegularExpressions;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ConsultoraPro.Application.Services;

public partial class AzurePortalUrlResolver : IAzurePortalUrlResolver
{
    private static readonly Regex SubscriptionPattern = SubscriptionRegex();
    private static readonly Regex ResourceIdPattern = ResourceIdRegex();

    private readonly IAzureSubscriptionTenantMappingRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AzurePortalUrlResolver> _logger;

    private const string CacheKey = "AzureSubscriptionMappings";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public AzurePortalUrlResolver(
        IAzureSubscriptionTenantMappingRepository repository,
        IMemoryCache cache,
        ILogger<AzurePortalUrlResolver> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> ResolveAsync(string? originalUrl)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
            return originalUrl;

        var subscriptionId = ExtractSubscriptionId(originalUrl);
        if (subscriptionId is null)
        {
            _logger.LogDebug("No se pudo extraer subscriptionId de URL: {Url}", originalUrl);
            return originalUrl;
        }

        var resourceId = ExtractResourceId(originalUrl);
        if (resourceId is null)
        {
            _logger.LogDebug("No se pudo extraer resourceId de URL: {Url}", originalUrl);
            return originalUrl;
        }

        var subGuid = Guid.Parse(subscriptionId);
        var mappings = await GetMappingsAsync();
        if (!mappings.TryGetValue(subGuid, out var tenantId))
        {
            _logger.LogDebug("No se encontró mapping para subscription {Sub}", subscriptionId);
            return originalUrl;
        }

        var resolved = $"https://portal.azure.com/#@{tenantId}/resource{resourceId}";
        _logger.LogInformation("URL resuelta: {Original} -> {Resolved}", originalUrl, resolved);
        return resolved;
    }

    public string? ExtractSubscriptionId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var match = SubscriptionPattern.Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    public string? ExtractResourceId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var match = ResourceIdPattern.Match(url);
        return match.Success ? match.Value : null;
    }

    private async Task<IReadOnlyDictionary<Guid, Guid>> GetMappingsAsync()
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyDictionary<Guid, Guid>? cached) && cached is not null)
            return cached;

        var mappings = await _repository.GetAllActiveAsync();
        var dict = mappings.ToDictionary(m => m.SubscriptionId, m => m.TenantId);

        _cache.Set(CacheKey, dict, CacheDuration);
        _logger.LogDebug("Cargados {Count} mappings en caché", dict.Count);

        return dict;
    }

    [GeneratedRegex(@"subscriptions/([0-9a-fA-F\-]{36})", RegexOptions.IgnoreCase)]
    private static partial Regex SubscriptionRegex();

    [GeneratedRegex(@"/subscriptions/[0-9a-fA-F\-]{36}(/.+)", RegexOptions.IgnoreCase)]
    private static partial Regex ResourceIdRegex();
}
