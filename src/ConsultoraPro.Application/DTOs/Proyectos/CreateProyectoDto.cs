using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Proyectos;

public class CreateProyectoDto
{
    public string Nombre { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Guid TipoSolucionId { get; set; }
    public EtapaProyecto Etapa { get; set; }
    public EstadoProyecto Estado { get; set; }
    public List<CreateDesarrolladorDto> Desarrolladores { get; set; } = new();
}
