using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Screenshots;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ScreenshotsController : ControllerBase
{
    private readonly IScreenshotRepository _screenshotRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IStorageService _storageService;
    private readonly IFileUrlResolver _urlResolver;
    private readonly StorageOptions _storage;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public ScreenshotsController(
        IScreenshotRepository screenshotRepository,
        IProyectoRepository proyectoRepository,
        IStorageService storageService,
        IFileUrlResolver urlResolver,
        IOptions<StorageOptions> storageOptions,
        UserManager<ApplicationUser> userManager,
        IMapper mapper)
    {
        _screenshotRepository = screenshotRepository;
        _proyectoRepository = proyectoRepository;
        _storageService = storageService;
        _urlResolver = urlResolver;
        _storage = storageOptions.Value;
        _userManager = userManager;
        _mapper = mapper;
    }

    [HttpGet("proyecto/{proyectoId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ScreenshotDto>>>> GetByProyecto(Guid proyectoId)
    {
        if (User.IsInRole(ConsultoraPro.Domain.Security.PermissionCatalog.Soporte) || User.IsInRole(ConsultoraPro.Domain.Security.PermissionCatalog.Dev))
        {
            var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId);
            var isMember = proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == GetUserId()) ?? false;
            if (!isMember)
                return Forbid();
        }

        var screenshots = await _screenshotRepository.GetByProyectoIdAsync(proyectoId);
        var dtos = _mapper.Map<List<ScreenshotDto>>(screenshots);
        // Url lleva la StorageKey desde el mapper: se firma (SAS) en lectura.
        foreach (var dto in dtos)
            dto.Url = await _urlResolver.ResolveAsync(dto.Url) ?? dto.Url;
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

        // Validate size
        if (file.Length > _storage.Limits.MaxImageBytes)
        {
            return BadRequest(new ApiResponse<ScreenshotDto> { Success = false, Message = $"El tamaño máximo permitido es de {_storage.Limits.MaxImageBytes / (1024 * 1024)}MB" });
        }

        // Validate extension
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_storage.Limits.AllowedImageExtensions.Contains(ext))
        {
            return BadRequest(new ApiResponse<ScreenshotDto> { Success = false, Message = $"Solo se permiten imágenes: {string.Join(", ", _storage.Limits.AllowedImageExtensions)}" });
        }

        // Verify project exists
        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId);
        if (proyecto == null)
        {
            return NotFound(new ApiResponse<ScreenshotDto> { Success = false, Message = "Proyecto no encontrado" });
        }

        var userId = GetUserId();
        if (User.IsInRole(ConsultoraPro.Domain.Security.PermissionCatalog.Soporte) || User.IsInRole(ConsultoraPro.Domain.Security.PermissionCatalog.Dev))
        {
            var isMember = proyecto.ProyectoMiembros.Any(pm => pm.UsuarioId == userId);
            if (!isMember)
                return Forbid();
        }
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Unauthorized(new ApiResponse<ScreenshotDto> { Success = false, Message = "Usuario no válido" });
        }

        // Save file
        StoredFile stored;
        using (var stream = file.OpenReadStream())
        {
            stored = await _storageService.SaveFileAsync(stream, file.FileName, file.ContentType, "screenshots");
        }

        // Create database record
        var screenshot = new Screenshot
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyectoId,
            Nombre = string.IsNullOrWhiteSpace(nombre) ? file.FileName : nombre.Trim(),
            Version = version?.Trim() ?? string.Empty,
            Descripcion = descripcion?.Trim() ?? string.Empty,
            StorageKey = stored.Key,
            SubidoPorId = userId,
            SubidoPor = user,
            FechaSubida = DateTime.UtcNow,
            Activo = true
        };

        var created = await _screenshotRepository.CreateAsync(screenshot);
        var dto = _mapper.Map<ScreenshotDto>(created);
        dto.Url = await _urlResolver.ResolveAsync(dto.Url) ?? dto.Url;

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

        if (User.IsInRole(ConsultoraPro.Domain.Security.PermissionCatalog.Soporte) || User.IsInRole(ConsultoraPro.Domain.Security.PermissionCatalog.Dev))
        {
            var proyecto = await _proyectoRepository.GetByIdAsync(screenshot.ProyectoId);
            var isMember = proyecto?.ProyectoMiembros.Any(pm => pm.UsuarioId == GetUserId()) ?? false;
            if (!isMember)
                return Forbid();
        }

        // Delete physical file
        await _storageService.DeleteFileAsync(screenshot.StorageKey);

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
