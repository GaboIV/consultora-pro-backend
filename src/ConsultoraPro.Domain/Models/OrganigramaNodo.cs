namespace ConsultoraPro.Domain.Models;

/// <summary>
/// Posición dentro de un <see cref="Organigrama"/>. El cargo es texto libre y la persona puede ser un
/// usuario del portal (<see cref="UsuarioId"/>), un nombre escrito a mano (<see cref="NombreLibre"/>)
/// o quedar vacante. La jerarquía se expresa con <see cref="ParentId"/>, que el servicio valida
/// (sin FK para poder reemplazar el árbol completo en una sola operación).
/// </summary>
public class OrganigramaNodo
{
    public Guid Id { get; set; }
    public Guid OrganigramaId { get; set; }
    public Organigrama Organigrama { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string NombreLibre { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
    public ApplicationUser? Usuario { get; set; }
    public string Notas { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Orden { get; set; }
}
