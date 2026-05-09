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
        RuleFor(x => x.TechLead).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TechLeadIniciales).MaximumLength(2);
    }
}
