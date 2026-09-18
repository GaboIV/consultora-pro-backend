namespace ConsultoraPro.Domain.Enums;

/// <summary>Ciclo de vida documental: Borrador → EnRevision → Aprobado → Obsoleto.</summary>
public enum EstadoDocumento
{
    Borrador,
    EnRevision,
    Aprobado,
    Obsoleto
}
