namespace ConsultoraPro.Application.DTOs.Clientes;

public class CreateClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Industria { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public string ColorClass { get; set; } = "blue";
}
