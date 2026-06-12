using ConsultoraPro.Application.DTOs.AmbienteCloudResources;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/ambientes/{ambienteId}/cloud-resources")]
public class AmbienteCloudResourcesController : ControllerBase
{
    private readonly IAmbienteCloudResourceService _service;

    public AmbienteCloudResourcesController(IAmbienteCloudResourceService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AmbienteCloudResourceDto>>>> GetAll(Guid ambienteId)
    {
        var data = await _service.GetByAmbienteAsync(ambienteId);
        return Ok(new ApiResponse<IEnumerable<AmbienteCloudResourceDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ambientes.ver")]
    public async Task<ActionResult<ApiResponse<AmbienteCloudResourceDto>>> GetById(Guid ambienteId, Guid id)
    {
        var data = await _service.GetByIdAsync(id);
        if (data is null)
            return NotFound(new ApiResponse<AmbienteCloudResourceDto> { Success = false, Message = $"Recurso en la nube con ID {id} no encontrado" });

        return Ok(new ApiResponse<AmbienteCloudResourceDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<AmbienteCloudResourceDto>>> Create(Guid ambienteId, [FromBody] CreateAmbienteCloudResourceDto dto)
    {
        dto.AmbienteId = ambienteId;
        var data = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { ambienteId, id = data.Id }, new ApiResponse<AmbienteCloudResourceDto>
        {
            Success = true,
            Data = data,
            Message = "Recurso en la nube creado exitosamente"
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid ambienteId, Guid id, [FromBody] UpdateAmbienteCloudResourceDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Recurso en la nube actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid ambienteId, Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Recurso en la nube desactivado exitosamente" });
    }

    [HttpPost("import-csv")]
    [Authorize(Policy = "ambientes.editar")]
    public async Task<ActionResult<ApiResponse<ImportCloudResourcesCsvResponse>>> ImportCsv(Guid ambienteId, [FromBody] ImportCloudResourcesCsvRequest request)
    {
        var result = await _service.ImportFromCsvAsync(ambienteId, request);

        var message = result.ImportedCount > 0
            ? $"Se importaron {result.ImportedCount} recurso(s) correctamente."
            : "No se importaron recursos.";

        if (result.Errors.Count > 0)
            message += $" {result.Errors.Count} error(es): {string.Join(" | ", result.Errors.Take(5))}";

        return Ok(new ApiResponse<ImportCloudResourcesCsvResponse>
        {
            Success = result.Errors.Count == 0 || result.ImportedCount > 0,
            Data = result,
            Message = message
        });
    }
}
