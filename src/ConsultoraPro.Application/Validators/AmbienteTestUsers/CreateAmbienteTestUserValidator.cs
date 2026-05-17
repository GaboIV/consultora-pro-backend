using ConsultoraPro.Application.DTOs.AmbienteTestUsers;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.AmbienteTestUsers;

public class CreateAmbienteTestUserValidator : AbstractValidator<CreateAmbienteTestUserDto>
{
    public CreateAmbienteTestUserValidator()
    {
        RuleFor(x => x.AmbienteId).NotEmpty();
        RuleFor(x => x.RolAplicacion).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Correo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notas).MaximumLength(500);
    }
}
