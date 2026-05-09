using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Management;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/management")]
public class ManagementController : ControllerBase
{
    private readonly IManagementService _managementService;

    public ManagementController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    [HttpGet("snapshot")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ManagementSnapshotDto>>> GetSnapshot()
    {
        var data = await _managementService.GetSnapshotAsync();
        return Ok(new ApiResponse<ManagementSnapshotDto> { Success = true, Data = data });
    }
}
