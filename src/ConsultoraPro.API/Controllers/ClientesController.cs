using ConsultoraPro.Application.DTOs.Clientes;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    [Authorize(Policy = "clientes.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClienteDto>>>> GetAll()
    {
        var data = await _clienteService.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<ClienteDto>> { Success = true, Data = data });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "clientes.ver")]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> GetById(Guid id)
    {
        var data = await _clienteService.GetByIdAsync(id);
        if (data == null)
            return NotFound(new ApiResponse<ClienteDto> { Success = false, Message = $"Cliente con ID {id} no encontrado" });
        return Ok(new ApiResponse<ClienteDto> { Success = true, Data = data });
    }

    [HttpPost]
    [Authorize(Policy = "clientes.crear")]
    public async Task<ActionResult<ApiResponse<ClienteDto>>> Create([FromBody] CreateClienteDto dto)
    {
        var data = await _clienteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, new ApiResponse<ClienteDto> { Success = true, Data = data, Message = "Cliente creado exitosamente" });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "clientes.editar")]
    public async Task<ActionResult<ApiResponse<object>>> Update(Guid id, [FromBody] UpdateClienteDto dto)
    {
        await _clienteService.UpdateAsync(id, dto);
        return Ok(new ApiResponse<object> { Success = true, Message = "Cliente actualizado exitosamente" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "clientes.eliminar")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _clienteService.DeleteAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Cliente desactivado exitosamente" });
    }
}
