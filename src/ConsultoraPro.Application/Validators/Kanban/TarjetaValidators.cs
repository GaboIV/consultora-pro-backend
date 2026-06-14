using ConsultoraPro.Application.DTOs.Kanban;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Kanban;

public class CreateTarjetaValidator : AbstractValidator<CreateTarjetaDto>
{
    public CreateTarjetaValidator()
    {
        RuleFor(x => x.ColumnaId).NotEmpty();
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Prioridad).IsInEnum();
    }
}

public class UpdateTarjetaValidator : AbstractValidator<UpdateTarjetaDto>
{
    public UpdateTarjetaValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Prioridad).IsInEnum();
    }
}

public class MoverTarjetaValidator : AbstractValidator<MoverTarjetaDto>
{
    public MoverTarjetaValidator()
    {
        RuleFor(x => x.ColumnaDestinoId).NotEmpty();
    }
}

public class CreateComentarioValidator : AbstractValidator<CreateComentarioDto>
{
    public CreateComentarioValidator()
    {
        RuleFor(x => x.Texto).NotEmpty().MaximumLength(4000);
    }
}

public class CreateChecklistItemValidator : AbstractValidator<CreateChecklistItemDto>
{
    public CreateChecklistItemValidator()
    {
        RuleFor(x => x.Texto).NotEmpty().MaximumLength(500);
    }
}
