namespace ConsultoraPro.Application.DTOs.Organigramas;

public class OrganigramaResumenDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int TotalNodos { get; set; }
    public int TotalAsignados { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class OrganigramaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public List<OrganigramaNodoDto> Nodos { get; set; } = new();
}

public class OrganigramaNodoDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string NombreLibre { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
    public OrganigramaUsuarioDto? Usuario { get; set; }
    public string Notas { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Orden { get; set; }
}

/// <summary>Datos mínimos de un usuario para mostrarlo dentro de un organigrama.</summary>
public class OrganigramaUsuarioDto
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string? AvatarUrl { get; set; }
}

/// <summary>Payload de creación y edición: el árbol completo se envía en cada guardado.</summary>
public class GuardarOrganigramaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public List<GuardarOrganigramaNodoDto> Nodos { get; set; } = new();
}

public class GuardarOrganigramaNodoDto
{
    /// <summary>Id generado por el cliente, necesario para que los hijos referencien a su padre.</summary>
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? NombreLibre { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? Notas { get; set; }
    public string? Color { get; set; }
    public int Orden { get; set; }
}
