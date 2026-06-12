namespace ConsultoraPro.Application.DTOs.Search;

public class SearchResultDto
{
    public List<SearchItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public string Query { get; set; } = string.Empty;
    public bool Local { get; set; }
}

public class SearchItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string BadgeVariant { get; set; } = "gray";
    public string Icon { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string NavigateTo { get; set; } = string.Empty;
}
