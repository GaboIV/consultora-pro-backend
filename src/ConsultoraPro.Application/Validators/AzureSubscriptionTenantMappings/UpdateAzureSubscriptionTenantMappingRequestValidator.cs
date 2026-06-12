using ConsultoraPro.Application.DTOs.AzureSubscriptionTenantMappings;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.AzureSubscriptionTenantMappings;

public class UpdateAzureSubscriptionTenantMappingRequestValidator : AbstractValidator<UpdateAzureSubscriptionTenantMappingRequest>
{
    public UpdateAzureSubscriptionTenantMappingRequestValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty().WithMessage("SubscriptionId es requerido.");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId es requerido.");
        RuleFor(x => x.Alias).MaximumLength(120);
        RuleFor(x => x.Environment).MaximumLength(60);
    }
}
