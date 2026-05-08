using ConsultoraPro.Application.DTOs.Clientes;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Clientes;

public class UpdateClienteValidator : AbstractValidator<UpdateClienteDto>
{
    public UpdateClienteValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Iniciales).MaximumLength(2);
        RuleFor(x => x.Industria).MaximumLength(100);
        RuleFor(x => x.ColorClass)
            .Must(c => new[] { "blue", "purple", "green", "amber", "red" }.Contains(c))
            .When(x => !string.IsNullOrEmpty(x.ColorClass));
    }
}
