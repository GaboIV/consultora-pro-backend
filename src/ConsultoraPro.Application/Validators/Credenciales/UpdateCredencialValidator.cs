using ConsultoraPro.Application.DTOs.Credenciales;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Credenciales;

public class UpdateCredencialValidator : AbstractValidator<UpdateCredencialDto>
{
    public UpdateCredencialValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.Servidor).NotEmpty().MaximumLength(220);
        RuleFor(x => x.Host).MaximumLength(200);
        RuleFor(x => x.Puerto).InclusiveBetween(1, 65535).When(x => x.Puerto.HasValue);
        RuleFor(x => x.Usuario).MaximumLength(160);
        RuleFor(x => x.Url).MaximumLength(500);
        RuleFor(x => x.Notas).MaximumLength(1000);
        RuleFor(x => x.CamposExtra)
            .Must(d => CredencialExtraRules.FitsAsJson(d, CredencialExtraRules.MaxCamposExtraJson))
            .WithMessage("Los campos específicos exceden el tamaño máximo permitido");
        RuleFor(x => x.ProyectoId).NotEmpty();
        RuleFor(x => x.FechaVencimiento).NotEmpty();
    }
}
