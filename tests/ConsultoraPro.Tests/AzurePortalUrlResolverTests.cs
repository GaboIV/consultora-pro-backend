using ConsultoraPro.Application.Services;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConsultoraPro.Tests;

public class AzurePortalUrlResolverTests
{
    private readonly Mock<IAzureSubscriptionTenantMappingRepository> _repositoryMock;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<AzurePortalUrlResolver>> _loggerMock;
    private readonly AzurePortalUrlResolver _resolver;

    private static readonly Guid SubId = Guid.Parse("ac6c79c7-7a17-4702-875a-d8da0991f502");
    private static readonly Guid TenantId = Guid.Parse("00d941eb-0604-431b-9126-b3db997d2cf0");

    private const string ValidUrl = "https://portal.azure.com#resource/subscriptions/ac6c79c7-7a17-4702-875a-d8da0991f502/resourceGroups/EQUALY-KOMATSU/providers/Microsoft.Network/publicIPAddresses/EQUALY-KOMATSU-public-ip";
    private static readonly string ExpectedResolved = $"https://portal.azure.com/#@{TenantId}/resource/subscriptions/{SubId}/resourceGroups/EQUALY-KOMATSU/providers/Microsoft.Network/publicIPAddresses/EQUALY-KOMATSU-public-ip";

    public AzurePortalUrlResolverTests()
    {
        _repositoryMock = new Mock<IAzureSubscriptionTenantMappingRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<AzurePortalUrlResolver>>();

        _repositoryMock
            .Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(new List<AzureSubscriptionTenantMapping>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = SubId,
                    TenantId = TenantId,
                    Alias = "Test",
                    Environment = "Production",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            });

        _resolver = new AzurePortalUrlResolver(_repositoryMock.Object, _cache, _loggerMock.Object);
    }

    [Fact]
    public async Task ResolveAsync_WithValidUrlAndMapping_ReturnsResolvedUrl()
    {
        var result = await _resolver.ResolveAsync(ValidUrl);
        Assert.Equal(ExpectedResolved, result);
    }

    [Fact]
    public async Task ResolveAsync_WithValidUrlWithoutMapping_ReturnsOriginalUrl()
    {
        var resolver = CreateResolverWithEmptyMappings();
        var result = await resolver.ResolveAsync(ValidUrl);
        Assert.Equal(ValidUrl, result);
    }

    [Fact]
    public async Task ResolveAsync_WithNullUrl_ReturnsNull()
    {
        var result = await _resolver.ResolveAsync(null);
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_WithEmptyUrl_ReturnsEmpty()
    {
        var result = await _resolver.ResolveAsync(string.Empty);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task ResolveAsync_WithWhitespaceUrl_ReturnsWhitespace()
    {
        var result = await _resolver.ResolveAsync("   ");
        Assert.Equal("   ", result);
    }

    [Fact]
    public async Task ResolveAsync_WithInvalidUrl_ReturnsOriginalUrl()
    {
        const string invalidUrl = "https://example.com/some/page";
        var result = await _resolver.ResolveAsync(invalidUrl);
        Assert.Equal(invalidUrl, result);
    }

    [Fact]
    public async Task ResolveAsync_WithUrlHavingHashSlash_WorksCorrectly()
    {
        const string url = "https://portal.azure.com/#resource/subscriptions/ac6c79c7-7a17-4702-875a-d8da0991f502/resourceGroups/test/providers/Microsoft.Compute/virtualMachines/test-vm";
        var result = await _resolver.ResolveAsync(url);
        Assert.StartsWith($"https://portal.azure.com/#@{TenantId}/resource/subscriptions/{SubId}/resourceGroups/test", result);
    }

    [Fact]
    public async Task ResolveAsync_WithNonExistentSubscriptionId_ReturnsOriginalUrl()
    {
        const string url = "https://portal.azure.com#resource/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/test/providers/Microsoft.Network/publicIPAddresses/test-ip";
        var result = await _resolver.ResolveAsync(url);
        Assert.Equal(url, result);
    }

    [Fact]
    public void ExtractSubscriptionId_WithValidUrl_ReturnsGuid()
    {
        var result = _resolver.ExtractSubscriptionId(ValidUrl);
        Assert.Equal("ac6c79c7-7a17-4702-875a-d8da0991f502", result);
    }

    [Fact]
    public void ExtractSubscriptionId_WithInvalidUrl_ReturnsNull()
    {
        var result = _resolver.ExtractSubscriptionId("https://example.com");
        Assert.Null(result);
    }

    [Fact]
    public void ExtractResourceId_WithValidUrl_ReturnsResourcePath()
    {
        var result = _resolver.ExtractResourceId(ValidUrl);
        Assert.NotNull(result);
        Assert.StartsWith("/subscriptions/", result);
        Assert.Contains("/resourceGroups/EQUALY-KOMATSU/providers/Microsoft.Network/publicIPAddresses", result);
    }

    [Fact]
    public void ExtractResourceId_WithInvalidUrl_ReturnsNull()
    {
        var result = _resolver.ExtractResourceId("https://example.com");
        Assert.Null(result);
    }

    [Fact]
    public void ExtractSubscriptionId_WithNull_ReturnsNull()
    {
        Assert.Null(_resolver.ExtractSubscriptionId(null));
    }

    [Fact]
    public void ExtractSubscriptionId_WithEmpty_ReturnsNull()
    {
        Assert.Null(_resolver.ExtractSubscriptionId(""));
    }

    private AzurePortalUrlResolver CreateResolverWithEmptyMappings()
    {
        var repoMock = new Mock<IAzureSubscriptionTenantMappingRepository>();
        repoMock
            .Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(new List<AzureSubscriptionTenantMapping>());

        return new AzurePortalUrlResolver(repoMock.Object, new MemoryCache(new MemoryCacheOptions()), _loggerMock.Object);
    }
}
