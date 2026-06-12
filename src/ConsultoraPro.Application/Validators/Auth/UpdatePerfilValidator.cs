using ConsultoraPro.Application.DTOs.Auth;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Auth;

public class UpdatePerfilValidator : AbstractValidator<UpdatePerfilDto>
{
    public UpdatePerfilValidator()
    {
        RuleFor(x => x.Nombres)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres");

        RuleFor(x => x.Apellidos)
            .NotEmpty().WithMessage("Los apellidos son requeridos")
            .MaximumLength(100).WithMessage("Los apellidos no pueden tener más de 100 caracteres");

        RuleFor(x => x.Telefono)
            .MaximumLength(20).WithMessage("El teléfono no puede tener más de 20 caracteres");

        RuleFor(x => x.Iniciales)
            .MaximumLength(2).WithMessage("Las iniciales no pueden tener más de 2 caracteres");
    }
}
