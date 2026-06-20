using ConsultoraPro.Application.DTOs.Kanban;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Kanban;

public class CreateColumnaValidator : AbstractValidator<CreateColumnaDto>
{
    public CreateColumnaValidator()
    {
        RuleFor(x => x.TableroId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.LimiteWip).GreaterThan(0).When(x => x.LimiteWip.HasValue);
    }
}

public class UpdateColumnaValidator : AbstractValidator<UpdateColumnaDto>
{
    public UpdateColumnaValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.LimiteWip).GreaterThan(0).When(x => x.LimiteWip.HasValue);
    }
}
