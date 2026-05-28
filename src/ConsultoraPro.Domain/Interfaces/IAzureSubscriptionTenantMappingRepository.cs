using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IAzureSubscriptionTenantMappingRepository
{
    Task<IEnumerable<AzureSubscriptionTenantMapping>> GetAllAsync();
    Task<IEnumerable<AzureSubscriptionTenantMapping>> GetAllActiveAsync();
    Task<AzureSubscriptionTenantMapping?> GetByIdAsync(Guid id);
    Task<AzureSubscriptionTenantMapping?> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<AzureSubscriptionTenantMapping> CreateAsync(AzureSubscriptionTenantMapping entity);
    Task UpdateAsync(AzureSubscriptionTenantMapping entity);
    Task<bool> ExistsBySubscriptionIdAsync(Guid subscriptionId, Guid? excludeId = null);
}
