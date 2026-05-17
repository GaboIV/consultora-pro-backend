using ConsultoraPro.Application.DTOs.AmbienteCloudResources;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.AmbienteCloudResources;

public class UpdateAmbienteCloudResourceValidator : AbstractValidator<UpdateAmbienteCloudResourceDto>
{
    public UpdateAmbienteCloudResourceValidator()
    {
        RuleFor(x => x.TipoRecurso).NotEmpty().MaximumLength(80);
        RuleFor(x => x.NombreRecurso).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DeepLink).MaximumLength(500);
        RuleFor(x => x.Plataforma).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Ubicacion).MaximumLength(120);
        RuleFor(x => x.Nota).MaximumLength(500);
    }
}
