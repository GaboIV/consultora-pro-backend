using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class AzureSubscriptionTenantMappingRepository : IAzureSubscriptionTenantMappingRepository
{
    private readonly AppDbContext _context;

    public AzureSubscriptionTenantMappingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AzureSubscriptionTenantMapping>> GetAllAsync()
    {
        return await _context.AzureSubscriptionTenantMappings
            .AsNoTracking()
            .OrderBy(m => m.Alias ?? m.SubscriptionId.ToString())
            .ToListAsync();
    }

    public async Task<IEnumerable<AzureSubscriptionTenantMapping>> GetAllActiveAsync()
    {
        return await _context.AzureSubscriptionTenantMappings
            .AsNoTracking()
            .Where(m => m.IsActive)
            .ToListAsync();
    }

    public async Task<AzureSubscriptionTenantMapping?> GetByIdAsync(Guid id)
    {
        return await _context.AzureSubscriptionTenantMappings
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<AzureSubscriptionTenantMapping?> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.AzureSubscriptionTenantMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.SubscriptionId == subscriptionId && m.IsActive);
    }

    public async Task<AzureSubscriptionTenantMapping> CreateAsync(AzureSubscriptionTenantMapping entity)
    {
        _context.AzureSubscriptionTenantMappings.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(AzureSubscriptionTenantMapping entity)
    {
        _context.AzureSubscriptionTenantMappings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsBySubscriptionIdAsync(Guid subscriptionId, Guid? excludeId = null)
    {
        var query = _context.AzureSubscriptionTenantMappings
            .Where(m => m.SubscriptionId == subscriptionId);

        if (excludeId.HasValue)
            query = query.Where(m => m.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
