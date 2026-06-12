namespace ConsultoraPro.Application.DTOs.AzureSubscriptionTenantMappings;

public class AzureSubscriptionTenantMappingDto
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Guid TenantId { get; set; }
    public string? Alias { get; set; }
    public string? Environment { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAzureSubscriptionTenantMappingRequest
{
    public Guid SubscriptionId { get; set; }
    public Guid TenantId { get; set; }
    public string? Alias { get; set; }
    public string? Environment { get; set; }
}

public class UpdateAzureSubscriptionTenantMappingRequest
{
    public Guid SubscriptionId { get; set; }
    public Guid TenantId { get; set; }
    public string? Alias { get; set; }
    public string? Environment { get; set; }
    public bool IsActive { get; set; }
}
