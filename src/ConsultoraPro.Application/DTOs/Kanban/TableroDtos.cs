using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Application.DTOs.Kanban;

public class TableroDto
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string ColorClass { get; set; } = "blue";
    public int Orden { get; set; }
    public int TotalColumnas { get; set; }
    public int TotalTarjetas { get; set; }
    public int TotalMiembros { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class TableroDetalleDto
{
    public Guid Id { get; set; }
    public Guid ProyectoId { get; set; }
    public string ProyectoNombre { get; set; } = string.Empty;
    public string ProyectoClave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string ColorClass { get; set; } = "blue";
    public int Orden { get; set; }
    public List<ColumnaDto> Columnas { get; set; } = new();
    public List<EtiquetaDto> Etiquetas { get; set; } = new();
    public List<TableroMiembroDto> Miembros { get; set; } = new();
}

public class CreateTableroDto
{
    public Guid ProyectoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Clave { get; set; }
    public string? Descripcion { get; set; }
    public string ColorClass { get; set; } = "blue";
    public bool CrearColumnasPorDefecto { get; set; } = true;
}

public class UpdateTableroDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string ColorClass { get; set; } = "blue";
}

public class TableroMiembroDto
{
    public Guid UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public RolTablero Rol { get; set; }
}

public class TableroMiembroInputDto
{
    public Guid UsuarioId { get; set; }
    public RolTablero Rol { get; set; } = RolTablero.Colaborador;
}

public class UpdateMiembrosDto
{
    public List<TableroMiembroInputDto> Miembros { get; set; } = new();
}
