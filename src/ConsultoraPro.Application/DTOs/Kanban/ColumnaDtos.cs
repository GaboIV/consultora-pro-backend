namespace ConsultoraPro.Application.DTOs.Kanban;

public class ColumnaDto
{
    public Guid Id { get; set; }
    public Guid TableroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public double Orden { get; set; }
    public int? LimiteWip { get; set; }
    public List<TarjetaDto> Tarjetas { get; set; } = new();
}

public class CreateColumnaDto
{
    public Guid TableroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? LimiteWip { get; set; }
}

public class UpdateColumnaDto
{
    public string Nombre { get; set; } = string.Empty;
    public int? LimiteWip { get; set; }
}

public class ReordenarColumnaDto
{
    public Guid? AntesDeColumnaId { get; set; }
    public Guid? DespuesDeColumnaId { get; set; }
}
