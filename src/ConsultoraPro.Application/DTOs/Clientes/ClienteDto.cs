using ConsultoraPro.Application.DTOs.Proyectos;

namespace ConsultoraPro.Application.DTOs.Clientes;

public class ClienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Industria { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;
    public int TotalProyectos { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaAlta { get; set; }
    public List<ProyectoDto> Proyectos { get; set; } = new();
}
