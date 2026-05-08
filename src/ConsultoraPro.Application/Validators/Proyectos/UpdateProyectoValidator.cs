using ConsultoraPro.Application.DTOs.Proyectos;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Proyectos;

public class UpdateProyectoValidator : AbstractValidator<UpdateProyectoDto>
{
    public UpdateProyectoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.Progreso).InclusiveBetween(0, 100);
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio);
        RuleFor(x => x.TechLead).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TechLeadIniciales).MaximumLength(2);
        RuleFor(x => x.TotalMiembros).GreaterThanOrEqualTo(0);
    }
}
