using System.Security.Claims;
using ConsultoraPro.Application.DTOs.Search;

namespace ConsultoraPro.Application.Interfaces;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(
        string query,
        IReadOnlyCollection<string> types,
        int limit,
        ClaimsPrincipal user);
}
