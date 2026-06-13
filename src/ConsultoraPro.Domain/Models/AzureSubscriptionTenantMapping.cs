namespace ConsultoraPro.Domain.Models;

public class AzureSubscriptionTenantMapping
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid TenantId { get; set; }
    public string? Alias { get; set; }
    public string? Environment { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
