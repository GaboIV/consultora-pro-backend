namespace ConsultoraPro.Domain.Models;

/// <summary>
/// Esquema de organigrama redactado a mano. Sus nodos describen cargos con texto libre, de modo
/// que la estructura no depende de los roles de seguridad del portal.
/// </summary>
public class Organigrama
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public Guid? CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public ICollection<OrganigramaNodo> Nodos { get; set; } = new List<OrganigramaNodo>();
}
