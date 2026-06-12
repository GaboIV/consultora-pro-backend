using ConsultoraPro.Application.DTOs.AzureSubscriptionTenantMappings;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class AzureSubscriptionTenantMappingService : IAzureSubscriptionTenantMappingService
{
    private readonly IAzureSubscriptionTenantMappingRepository _repository;

    public AzureSubscriptionTenantMappingService(IAzureSubscriptionTenantMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AzureSubscriptionTenantMappingDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(ToDto);
    }

    public async Task<AzureSubscriptionTenantMappingDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<AzureSubscriptionTenantMappingDto> CreateAsync(CreateAzureSubscriptionTenantMappingRequest request)
    {
        var exists = await _repository.ExistsBySubscriptionIdAsync(request.SubscriptionId);
        if (exists)
            throw new InvalidOperationException($"Ya existe un mapping para la suscripción {request.SubscriptionId}.");

        var entity = new AzureSubscriptionTenantMapping
        {
            Id = Guid.NewGuid(),
            SubscriptionId = request.SubscriptionId,
            TenantId = request.TenantId,
            Alias = request.Alias?.Trim(),
            Environment = request.Environment?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        entity = await _repository.CreateAsync(entity);
        return ToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateAzureSubscriptionTenantMappingRequest request)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Mapping con ID {id} no encontrado.");

        var duplicate = await _repository.ExistsBySubscriptionIdAsync(request.SubscriptionId, id);
        if (duplicate)
            throw new InvalidOperationException($"Ya existe otro mapping para la suscripción {request.SubscriptionId}.");

        entity.SubscriptionId = request.SubscriptionId;
        entity.TenantId = request.TenantId;
        entity.Alias = request.Alias?.Trim();
        entity.Environment = request.Environment?.Trim();
        entity.IsActive = request.IsActive;

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Mapping con ID {id} no encontrado.");

        entity.IsActive = false;
        await _repository.UpdateAsync(entity);
    }

    private static AzureSubscriptionTenantMappingDto ToDto(AzureSubscriptionTenantMapping entity)
    {
        return new AzureSubscriptionTenantMappingDto
        {
            Id = entity.Id,
            SubscriptionId = entity.SubscriptionId,
            TenantId = entity.TenantId,
            Alias = entity.Alias,
            Environment = entity.Environment,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }
}
