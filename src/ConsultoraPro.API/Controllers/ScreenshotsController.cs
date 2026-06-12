using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Screenshots;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ConsultoraPro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ScreenshotsController : ControllerBase
{
    private readonly IScreenshotRepository _screenshotRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IStorageService _storageService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public ScreenshotsController(
        IScreenshotRepository screenshotRepository,
        IProyectoRepository proyectoRepository,
        IStorageService storageService,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _screenshotRepository = screenshotRepository;
        _proyectoRepository = proyectoRepository;
        _storageService = storageService;
        _userManager = userManager;
        _mapper = mapper;
    }

    [HttpGet("proyecto/{proyectoId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ScreenshotDto>>>> GetByProyecto(Guid proyectoId)
    {
        var screenshots = await _screenshotRepository.GetByProyectoIdAsync(proyectoId);
        var dtos = _mapper.Map<IEnumerable<ScreenshotDto>>(screenshots);
        return Ok(new ApiResponse<IEnumerable<ScreenshotDto>> { Success = true, Data = dtos });
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ScreenshotDto>>> Upload(
        [FromForm] Guid proyectoId,
        [FromForm] string nombre,
        [FromForm] string version,
        [FromForm] string descripcion,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ApiResponse<ScreenshotDto> { Success = false, Message = "No se proporcionó ningún archivo" });
        }

        // Validate size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new ApiResponse<ScreenshotDto> { Success = false, Message = "El tamaño máximo permitido es de 5MB" });
        }

        // Validate extension
        var ext = Path.GetExtension(file.FileName).ToLower();
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(new ApiResponse<ScreenshotDto> { Success = false, Message = "Solo se permiten imágenes en formato PNG o JPG" });
        }

        // Verify project exists
        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId);
        if (proyecto == null)
        {
            return NotFound(new ApiResponse<ScreenshotDto> { Success = false, Message = "Proyecto no encontrado" });
        }

        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Unauthorized(new ApiResponse<ScreenshotDto> { Success = false, Message = "Usuario no válido" });
        }

        // Save file
        string fileUrl;
        using (var stream = file.OpenReadStream())
        {
            fileUrl = await _storageService.SaveFileAsync(stream, file.FileName, file.ContentType);
        }

        // Create database record
        var screenshot = new Screenshot
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyectoId,
            Nombre = string.IsNullOrWhiteSpace(nombre) ? file.FileName : nombre.Trim(),
            Version = version?.Trim() ?? string.Empty,
            Descripcion = descripcion?.Trim() ?? string.Empty,
            Url = fileUrl,
            SubidoPorId = userId,
            SubidoPor = user,
            FechaSubida = DateTime.UtcNow,
            Activo = true
        };

        var created = await _screenshotRepository.CreateAsync(screenshot);
        var dto = _mapper.Map<ScreenshotDto>(created);

        return Ok(new ApiResponse<ScreenshotDto> { Success = true, Data = dto, Message = "Imagen subida exitosamente" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        var screenshot = await _screenshotRepository.GetByIdAsync(id);
        if (screenshot == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Screenshot no encontrada" });
        }

        // Delete physical file
        await _storageService.DeleteFileAsync(screenshot.Url);

        // Delete from database
        await _screenshotRepository.DeleteAsync(screenshot);

        return Ok(new ApiResponse<object> { Success = true, Message = "Screenshot eliminada exitosamente" });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Token inválido");

        return userId;
    }
}
