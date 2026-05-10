using ConsultoraPro.Application.DTOs.Credenciales;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Credenciales;

public class CreateCredencialValidator : AbstractValidator<CreateCredencialDto>
{
    public CreateCredencialValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Servidor).NotEmpty().MaximumLength(220);
        RuleFor(x => x.ProyectoId).NotEmpty();
        RuleFor(x => x.Valor).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.FechaVencimiento).NotEmpty();
    }
}
