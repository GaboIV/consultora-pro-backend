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
        RuleFor(x => x.Host).MaximumLength(200);
        RuleFor(x => x.Puerto).InclusiveBetween(1, 65535).When(x => x.Puerto.HasValue);
        RuleFor(x => x.Usuario).MaximumLength(160);
        RuleFor(x => x.Url).MaximumLength(500);
        RuleFor(x => x.Notas).MaximumLength(1000);
        RuleFor(x => x.ProyectoId).NotEmpty();
        RuleFor(x => x.Valor).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.FechaVencimiento).NotEmpty();
    }
}
