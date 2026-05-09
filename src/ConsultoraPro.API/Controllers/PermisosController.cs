using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Security;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermisosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PermisosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Policy = "roles.ver")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PermisoModuloDto>>>> GetAll()
    {
        var permisos = await _context.Permisos
            .AsNoTracking()
            .OrderBy(p => p.Modulo)
            .ThenBy(p => p.Clave)
            .Select(p => new PermisoDto
            {
                Id = p.Id,
                Clave = p.Clave,
                Nombre = p.Nombre,
                Modulo = p.Modulo,
                Descripcion = p.Descripcion,
                Concedido = false
            })
            .ToListAsync();

        var data = permisos
            .GroupBy(p => p.Modulo)
            .Select(group => new PermisoModuloDto
            {
                Modulo = group.Key,
                Permisos = group.ToList()
            })
            .ToList();

        return Ok(new ApiResponse<IEnumerable<PermisoModuloDto>> { Success = true, Data = data });
    }
}
