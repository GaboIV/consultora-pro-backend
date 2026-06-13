using ConsultoraPro.Application.DTOs.AmbienteTestUsers;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.AmbienteTestUsers;

public class UpdateAmbienteTestUserValidator : AbstractValidator<UpdateAmbienteTestUserDto>
{
    public UpdateAmbienteTestUserValidator()
    {
        RuleFor(x => x.RolAplicacion).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Correo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).MaximumLength(200);
        RuleFor(x => x.Notas).MaximumLength(500);
    }
}
