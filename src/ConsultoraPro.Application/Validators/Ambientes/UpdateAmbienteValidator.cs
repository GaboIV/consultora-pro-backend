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
            .MaximumLength(300)
            .Must(BeHttpUrlOrEmpty)
            .WithMessage("La URL debe ser absoluta y empezar con http:// o https://.");
        RuleFor(x => x.HealthCheckUrl)
            .MaximumLength(300)
            .Must(BeHttpUrlOrEmpty)
            .WithMessage("El Health Check URL debe ser una URL absoluta empezando con http:// o https://.");
        RuleFor(x => x.ProyectoId).NotEmpty();
        RuleFor(x => x.Tecnologia).MaximumLength(120);
        RuleFor(x => x.Estado).IsInEnum();
    }

    private static bool BeHttpUrlOrEmpty(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
