namespace ConsultoraPro.Domain.Models;

public class DocumentoEtiqueta
{
    public Guid Id { get; set; }
    public Guid DocumentoId { get; set; }
    public Documento Documento { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
}
