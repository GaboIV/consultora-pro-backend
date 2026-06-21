namespace ConsultoraPro.Application.DTOs.Security;

public class UsuarioListDto
{
    public Guid Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public Guid? RolId { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaAlta { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

public class UsuarioDetalleDto : UsuarioListDto
{
    public IReadOnlyList<string> Permisos { get; set; } = Array.Empty<string>();
}

public class CreateUsuarioDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Iniciales { get; set; }
    public string Puesto { get; set; } = string.Empty;
    public Guid RolId { get; set; }
    public string? Password { get; set; }
}

public class UpdateUsuarioDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Iniciales { get; set; }
    public string Puesto { get; set; } = string.Empty;
    public Guid RolId { get; set; }
}

public class UpdateUsuarioPasswordDto
{
    public string Password { get; set; } = string.Empty;
}

/// <summary>Proyecto del catálogo con el indicador de si el usuario tiene acceso asignado.</summary>
public class UsuarioProyectoAccesoDto
{
    public Guid ProyectoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public bool Asignado { get; set; }
}

/// <summary>Estado del acceso a proyectos de un usuario: si su rol tiene acceso total y los proyectos del catálogo.</summary>
public class UsuarioProyectosAccesoDto
{
    public bool AccesoTotal { get; set; }
    public IReadOnlyList<UsuarioProyectoAccesoDto> Proyectos { get; set; } = Array.Empty<UsuarioProyectoAccesoDto>();
}

public class UpdateUsuarioProyectosDto
{
    public IReadOnlyList<Guid> ProyectoIds { get; set; } = Array.Empty<Guid>();
}
