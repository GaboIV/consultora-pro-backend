namespace ConsultoraPro.Application.Interfaces;

public interface IAzurePortalUrlResolver
{
    Task<string?> ResolveAsync(string? originalUrl);
    string? ExtractSubscriptionId(string? url);
    string? ExtractResourceId(string? url);
}
