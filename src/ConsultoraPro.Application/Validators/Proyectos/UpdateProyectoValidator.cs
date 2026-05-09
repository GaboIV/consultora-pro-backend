using ConsultoraPro.Application.DTOs.Proyectos;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Proyectos;

public class UpdateProyectoValidator : AbstractValidator<UpdateProyectoDto>
{
    public UpdateProyectoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.TipoSolucionId).NotEmpty();
        RuleFor(x => x.Desarrolladores).NotEmpty().WithMessage("Debe asignar al menos un desarrollador");
        RuleForEach(x => x.Desarrolladores).ChildRules(dev =>
        {
            dev.RuleFor(d => d.MemberId).NotEmpty();
        });
    }
}
