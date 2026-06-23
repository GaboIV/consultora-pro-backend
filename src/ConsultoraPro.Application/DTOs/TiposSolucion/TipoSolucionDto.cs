namespace ConsultoraPro.Application.DTOs.TiposSolucion;

public class TipoSolucionDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public int TotalProyectos { get; set; }
}
