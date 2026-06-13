namespace ConsultoraPro.Application.DTOs.Security;

public class RolListDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool EsActivo { get; set; }
    public int UsuariosCount { get; set; }
    public IReadOnlyList<PermisoModuloDto> Permisos { get; set; } = Array.Empty<PermisoModuloDto>();
}

public class RolDetalleDto : RolListDto
{
    public IReadOnlyList<int> PermisosIds { get; set; } = Array.Empty<int>();
}

public class CreateRolDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class UpdateRolDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool EsActivo { get; set; } = true;
}

public class UpdateRolPermisosDto
{
    public IReadOnlyList<int> PermisosIds { get; set; } = Array.Empty<int>();
}

public class PermisoDto
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Concedido { get; set; }
}

public class PermisoModuloDto
{
    public string Modulo { get; set; } = string.Empty;
    public IReadOnlyList<PermisoDto> Permisos { get; set; } = Array.Empty<PermisoDto>();
}
