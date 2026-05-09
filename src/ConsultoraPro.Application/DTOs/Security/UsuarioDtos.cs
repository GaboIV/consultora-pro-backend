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
    public string Password { get; set; } = string.Empty;
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
