using ConsultoraPro.Application.DTOs.AmbienteCloudResources;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.AmbienteCloudResources;

public class CreateAmbienteCloudResourceValidator : AbstractValidator<CreateAmbienteCloudResourceDto>
{
    public CreateAmbienteCloudResourceValidator()
    {
        RuleFor(x => x.AmbienteId).NotEmpty();
        RuleFor(x => x.TipoRecurso).NotEmpty().MaximumLength(80);
        RuleFor(x => x.NombreRecurso).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DeepLink).MaximumLength(500);
    }
}
