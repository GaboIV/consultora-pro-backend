using ConsultoraPro.Application.DTOs.Credenciales;
using FluentValidation;

namespace ConsultoraPro.Application.Validators.Credenciales;

/// <summary>
/// Validación liviana del lote: las filas se validan una a una en el servicio
/// para poder reportar errores por fila en lugar de rechazar todo el archivo.
/// </summary>
public class ImportCredencialesValidator : AbstractValidator<ImportCredencialesDto>
{
    public const int MaxFilas = 500;

    public ImportCredencialesValidator()
    {
        RuleFor(x => x.Filas)
            .NotEmpty().WithMessage("El archivo no contiene filas para importar")
            .Must(f => f.Count <= MaxFilas).WithMessage($"Máximo {MaxFilas} filas por importación");
    }
}
