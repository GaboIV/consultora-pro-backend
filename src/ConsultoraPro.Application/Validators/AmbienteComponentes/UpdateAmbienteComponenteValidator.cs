using ConsultoraPro.Application.DTOs.AmbienteComponentes;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.AmbienteComponentes;

public class UpdateAmbienteComponenteValidator : AbstractValidator<UpdateAmbienteComponenteDto>
{
    public UpdateAmbienteComponenteValidator()
    {
        RuleFor(x => x.Rol).NotEmpty().MaximumLength(80);
        RuleFor(x => x.IpPublica).MaximumLength(45);
        RuleFor(x => x.IpPrivada).MaximumLength(45);
        RuleFor(x => x.Hostname).MaximumLength(220);
        RuleFor(x => x.Tecnologia).MaximumLength(120);
        RuleFor(x => x.Especificaciones).MaximumLength(200);
    }
}
