using ConsultoraPro.Application.DTOs.Proyectos;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Proyectos;

public class CreateProyectoValidator : AbstractValidator<CreateProyectoDto>
{
    public CreateProyectoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.TipoSolucionId).NotEmpty();
        RuleFor(x => x.Desarrolladores)
            .NotEmpty().WithMessage("Debe asignar al menos un desarrollador")
            .Must(desarrolladores => desarrolladores == null || desarrolladores.Select(d => d.MemberId).Distinct().Count() == desarrolladores.Count)
            .WithMessage("Un miembro no puede estar asignado más de una vez al mismo proyecto");
        RuleForEach(x => x.Desarrolladores).ChildRules(dev =>
        {
            dev.RuleFor(d => d.MemberId).NotEmpty();
        });
    }
}
