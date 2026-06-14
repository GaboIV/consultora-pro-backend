using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Models;

public class TableroMiembro
{
    public Guid Id { get; set; }
    public Guid TableroId { get; set; }
    public Tablero Tablero { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public ApplicationUser Usuario { get; set; } = null!;
    public RolTablero Rol { get; set; } = RolTablero.Colaborador;
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
}
