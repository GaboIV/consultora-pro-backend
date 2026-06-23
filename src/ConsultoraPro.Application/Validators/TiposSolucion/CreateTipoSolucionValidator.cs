using ConsultoraPro.Application.DTOs.TiposSolucion;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.TiposSolucion;

public class CreateTipoSolucionValidator : AbstractValidator<CreateTipoSolucionDto>
{
    public CreateTipoSolucionValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
    }
}
