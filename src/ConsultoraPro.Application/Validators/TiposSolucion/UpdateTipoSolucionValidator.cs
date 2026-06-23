using ConsultoraPro.Application.DTOs.TiposSolucion;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.TiposSolucion;

public class UpdateTipoSolucionValidator : AbstractValidator<UpdateTipoSolucionDto>
{
    public UpdateTipoSolucionValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
    }
}
