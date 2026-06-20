namespace ConsultoraPro.Application.DTOs.Kanban;

public class EtiquetaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ColorClass { get; set; } = "blue";
}

public class CreateEtiquetaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string ColorClass { get; set; } = "blue";
}

public class UpdateEtiquetaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string ColorClass { get; set; } = "blue";
}
