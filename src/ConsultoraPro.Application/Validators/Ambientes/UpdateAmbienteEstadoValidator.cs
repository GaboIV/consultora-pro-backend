using ConsultoraPro.Application.DTOs.Ambientes;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Ambientes;

public class UpdateAmbienteEstadoValidator : AbstractValidator<UpdateAmbienteEstadoDto>
{
    public UpdateAmbienteEstadoValidator()
    {
        RuleFor(x => x.Estado).IsInEnum();
        RuleFor(x => x.UptimePorcentaje)
            .InclusiveBetween(0, 100)
            .When(x => x.UptimePorcentaje.HasValue);
    }
}
