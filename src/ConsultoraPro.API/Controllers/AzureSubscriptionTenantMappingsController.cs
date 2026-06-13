using ConsultoraPro.Application.DTOs.AzureSubscriptionTenantMappings;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/azure-subscription-tenant-mappings")]
public class AzureSubscriptionTenantMappingsController : ControllerBase
{
    private readonly IAzureSubscriptionTenantMappingService _service;

    public AzureSubscriptionTenantMappingsController(IAzureSubscriptionTenantMappingService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AzureSubscriptionTenantMappingDto>>>> GetAll()
    {
        var data = await _service.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<AzureSubscriptionTenantMappingDto>>
        {
            Success = true,
            Data = data
        });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<AzureSubscriptionTenantMappingDto>>> GetById(Guid id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<AzureSubscriptionTenantMappingDto>
            {
                Success = false,
                Message = "Mapping no encontrado"
            });
        return Ok(new ApiResponse<AzureSubscriptionTenantMappingDto>
        {
            Success = true,
            Data = data
        });
    }

    [HttpPost]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<AzureSubscriptionTenantMappingDto>>> Create(
        [FromBody] CreateAzureSubscriptionTenantMappingRequest request)
    {
        try
        {
            var data = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = data.Id },
                new ApiResponse<AzureSubscriptionTenantMappingDto>
                {
                    Success = true,
                    Data = data,
                    Message = "Mapping creado exitosamente"
                });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<AzureSubscriptionTenantMappingDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateAzureSubscriptionTenantMappingRequest request)
    {
        try
        {
            await _service.UpdateAsync(id, request);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Mapping actualizado exitosamente"
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Mapping no encontrado"
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Mapping desactivado exitosamente"
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Mapping no encontrado"
            });
        }
    }
}
