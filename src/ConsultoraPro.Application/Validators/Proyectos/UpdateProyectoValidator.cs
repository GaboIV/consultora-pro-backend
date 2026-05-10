using ConsultoraPro.Application.DTOs.Proyectos;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Proyectos;

public class UpdateProyectoValidator : AbstractValidator<UpdateProyectoDto>
{
    public UpdateProyectoValidator()
    {
        RuleFor(p => p.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(p => p.ClienteId).NotEmpty();
        RuleFor(p => p.TipoSolucionId).NotEmpty();
        RuleFor(p => p.Etapa).IsInEnum();
        RuleFor(p => p.Estado).IsInEnum();
        
        RuleFor(p => p.Miembros)
            .Must(miembros => miembros == null || miembros.Select(m => m.UsuarioId).Distinct().Count() == miembros.Count)
            .WithMessage("No se pueden asignar duplicados de un mismo usuario al proyecto.");

        RuleForEach(p => p.Miembros).ChildRules(miembro =>
        {
            miembro.RuleFor(m => m.UsuarioId).NotEmpty();
            miembro.RuleFor(m => m.Rol).IsInEnum();
        });
    }
}
