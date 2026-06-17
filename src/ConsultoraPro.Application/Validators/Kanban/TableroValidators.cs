using ConsultoraPro.Application.DTOs.Kanban;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Kanban;

public class CreateTableroValidator : AbstractValidator<CreateTableroDto>
{
    public CreateTableroValidator()
    {
        RuleFor(x => x.ProyectoId)
            .NotEmpty()
            .When(x => !x.EsPersonal)
            .WithMessage("El proyecto es requerido para tableros de proyecto.");
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Clave)
            .Matches("^[A-Za-z0-9]{2,6}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Clave))
            .WithMessage("La clave debe tener entre 2 y 6 caracteres alfanuméricos.");
        RuleFor(x => x.Descripcion).MaximumLength(500);
        RuleFor(x => x.ColorClass).MaximumLength(20);
    }
}

public class UpdateTableroValidator : AbstractValidator<UpdateTableroDto>
{
    public UpdateTableroValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Clave)
            .NotEmpty()
            .Matches("^[A-Za-z0-9]{2,6}$")
            .WithMessage("La clave debe tener entre 2 y 6 caracteres alfanuméricos.");
        RuleFor(x => x.Descripcion).MaximumLength(500);
        RuleFor(x => x.ColorClass).MaximumLength(20);
    }
}

public class CreateEtiquetaValidator : AbstractValidator<CreateEtiquetaDto>
{
    public CreateEtiquetaValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(80);
        RuleFor(x => x.ColorClass).MaximumLength(20);
    }
}

public class UpdateEtiquetaValidator : AbstractValidator<UpdateEtiquetaDto>
{
    public UpdateEtiquetaValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(80);
        RuleFor(x => x.ColorClass).MaximumLength(20);
    }
}
