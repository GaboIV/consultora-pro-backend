using ConsultoraPro.Application.DTOs.Credenciales;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Credenciales;

public class UpdateCredencialValorValidator : AbstractValidator<UpdateCredencialValorDto>
{
    public UpdateCredencialValorValidator()
    {
        RuleFor(x => x.Valor).NotEmpty().MaximumLength(5000);
    }
}
