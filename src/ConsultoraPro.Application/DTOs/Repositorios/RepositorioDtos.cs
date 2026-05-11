using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Repositorios;

public class RepositorioDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public ProveedorRepositorio Proveedor { get; set; }
    public string RamaPrincipal { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public EstadoPipeline EstadoPipeline { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CreateRepositorioDto
{
    public string Nombre { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public ProveedorRepositorio Proveedor { get; set; }
    public string RamaPrincipal { get; set; } = "main";
    public string Url { get; set; } = string.Empty;
    public EstadoPipeline EstadoPipeline { get; set; } = EstadoPipeline.Desconocido;
}

public class UpdateRepositorioDto
{
    public string Nombre { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public ProveedorRepositorio Proveedor { get; set; }
    public string RamaPrincipal { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public EstadoPipeline EstadoPipeline { get; set; }
}
