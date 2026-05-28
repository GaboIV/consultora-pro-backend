using ConsultoraPro.Application.DTOs.AzureSubscriptionTenantMappings;

namespace ConsultoraPro.Application.Interfaces;

public interface IAzureSubscriptionTenantMappingService
{
    Task<IEnumerable<AzureSubscriptionTenantMappingDto>> GetAllAsync();
    Task<AzureSubscriptionTenantMappingDto?> GetByIdAsync(Guid id);
    Task<AzureSubscriptionTenantMappingDto> CreateAsync(CreateAzureSubscriptionTenantMappingRequest request);
    Task UpdateAsync(Guid id, UpdateAzureSubscriptionTenantMappingRequest request);
    Task DeleteAsync(Guid id);
}
