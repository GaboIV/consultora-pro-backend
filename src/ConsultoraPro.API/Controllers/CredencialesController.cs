using ConsultoraPro.Application.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CredencialesController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "credenciales.ver")]
    public ActionResult<ApiResponse<IEnumerable<object>>> GetAll()
    {
        return Ok(new ApiResponse<IEnumerable<object>>
        {
            Success = true,
            Data = Array.Empty<object>()
        });
    }
}
