namespace ConsultoraPro.Application.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = "OK";
    public List<string> Errors { get; set; } = new();
}
