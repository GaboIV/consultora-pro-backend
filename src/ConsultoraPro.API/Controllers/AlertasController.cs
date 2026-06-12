using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Alertas;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/alertas")]
public class AlertasController : ControllerBase
{
    private readonly IAlertaService _alertaService;

    public AlertasController(IAlertaService alertaService)
    {
        _alertaService = alertaService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<AlertaDto>>>> GetAlertas()
    {
        var data = await _alertaService.GetAlertasActivasAsync();
        return Ok(new ApiResponse<List<AlertaDto>> { Success = true, Data = data });
    }
}
