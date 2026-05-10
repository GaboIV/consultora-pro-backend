using ConsultoraPro.Application.DTOs.Ambientes;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Ambientes;

public class UpdateAmbienteValidator : AbstractValidator<UpdateAmbienteDto>
{
    public UpdateAmbienteValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(300)
            .Must(BeHttpUrl)
            .WithMessage("La URL debe ser absoluta y empezar con http:// o https://.");
        RuleFor(x => x.ProyectoId).NotEmpty();
        RuleFor(x => x.Tecnologia).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Estado).IsInEnum();
        RuleFor(x => x.UptimePorcentaje).InclusiveBetween(0, 100);
    }

    private static bool BeHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
