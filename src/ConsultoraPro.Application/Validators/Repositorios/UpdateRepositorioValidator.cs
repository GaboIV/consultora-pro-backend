using ConsultoraPro.Application.DTOs.Repositorios;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Repositorios;

public class UpdateRepositorioValidator : AbstractValidator<UpdateRepositorioDto>
{
    public UpdateRepositorioValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(x => x.ProyectoId).NotEmpty();
        RuleFor(x => x.Proveedor).IsInEnum();
        RuleFor(x => x.RamaPrincipal).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeHttpUrl)
            .WithMessage("La URL debe ser absoluta y empezar con http:// o https://.");
        RuleFor(x => x.EstadoPipeline).IsInEnum();
    }

    private static bool BeHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
