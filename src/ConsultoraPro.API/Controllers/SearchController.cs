using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Search;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/search")]
[Authorize]
public class SearchController : ControllerBase
{
    private static readonly IReadOnlySet<string> ValidTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "proyecto",
        "cliente",
        "usuario",
        "credencial",
        "ambiente",
        "repositorio",
        "despliegue"
    };

    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SearchResultDto>>> Search(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] string? types,
        [FromQuery] int? limit)
    {
        var parsedTypes = ParseTypes(types);
        var safeQuery = query?.Trim() ?? string.Empty;

        if (parsedTypes.Length == 0 && safeQuery.Length < 2)
        {
            return BadRequest(new ApiResponse<SearchResultDto>
            {
                Success = false,
                Message = "El término de búsqueda debe tener al menos 2 caracteres"
            });
        }

        var data = await _searchService.SearchAsync(
            safeQuery,
            parsedTypes,
            Math.Clamp(limit ?? 12, 1, 30),
            User);

        return Ok(new ApiResponse<SearchResultDto>
        {
            Success = true,
            Data = data
        });
    }

    private static string[] ParseTypes(string? types)
    {
        return string.IsNullOrWhiteSpace(types)
            ? []
            : types
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(type => ValidTypes.Contains(type))
                .ToArray();
    }
}
