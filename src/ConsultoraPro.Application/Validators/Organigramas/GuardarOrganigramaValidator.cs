using ConsultoraPro.Application.DTOs.Organigramas;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Organigramas;

public class GuardarOrganigramaValidator : AbstractValidator<GuardarOrganigramaDto>
{
    public const int MaxNodos = 500;

    public GuardarOrganigramaValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Descripcion).MaximumLength(500);
        RuleFor(x => x.Nodos)
            .NotNull()
            .Must(n => n.Count <= MaxNodos)
            .WithMessage($"Un organigrama admite como máximo {MaxNodos} posiciones.");

        RuleForEach(x => x.Nodos).ChildRules(nodo =>
        {
            nodo.RuleFor(n => n.Id).NotEmpty();
            nodo.RuleFor(n => n.Cargo).NotEmpty().WithMessage("Cada posición debe tener un cargo.").MaximumLength(120);
            nodo.RuleFor(n => n.Area).MaximumLength(120);
            nodo.RuleFor(n => n.NombreLibre).MaximumLength(150);
            nodo.RuleFor(n => n.Notas).MaximumLength(500);
            nodo.RuleFor(n => n.Color).MaximumLength(20).Matches("^[a-z-]*$");
        });
    }
}
