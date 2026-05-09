using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ConsultoraPro.API.Interfaces;
using ConsultoraPro.Application.DTOs.Auth;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ConsultoraPro.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        AppDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nombres = dto.Nombre,
            Apellidos = string.Empty,
            Iniciales = BuildInitials(dto.Nombre, string.Empty),
            Puesto = string.Empty,
            Activo = true,
            FechaAlta = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        ThrowIfFailed(result);

        if (!string.IsNullOrWhiteSpace(dto.Rol))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Rol))
                ThrowIfFailed(await _roleManager.CreateAsync(new ApplicationRole(dto.Rol)));

            ThrowIfFailed(await _userManager.AddToRoleAsync(user, dto.Rol));
        }

        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !user.Activo || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        user.UltimoAcceso = DateTime.UtcNow;
        ThrowIfFailed(await _userManager.UpdateAsync(user));

        return await GenerateTokenAsync(user);
    }

    public async Task<AuthUserDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.Activo)
            throw new UnauthorizedAccessException("Usuario no encontrado o inactivo");

        var (roleName, permisos) = await GetRoleAndPermissionsAsync(user);
        return ToAuthUserDto(user, roleName, permisos);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            throw new KeyNotFoundException("Usuario no encontrado");

        var result = await _userManager.ChangePasswordAsync(user, dto.PasswordActual, dto.PasswordNueva);
        ThrowIfFailed(result);
    }

    private async Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user)
    {
        var (roleName, permisos) = await GetRoleAndPermissionsAsync(user);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("userId", user.Id.ToString()),
            new("email", user.Email ?? string.Empty),
            new("nombres", user.Nombres),
            new("apellidos", user.Apellidos),
            new("iniciales", user.Iniciales),
            new("puesto", user.Puesto),
            new("role", roleName),
            new("permisos", JsonSerializer.Serialize(permisos), JsonClaimValueTypes.JsonArray)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            User = ToAuthUserDto(user, roleName, permisos)
        };
    }

    private async Task<(string RoleName, IReadOnlyList<string> Permisos)> GetRoleAndPermissionsAsync(ApplicationUser user)
    {
        var roleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(roleName))
            return (string.Empty, Array.Empty<string>());

        var role = await _roleManager.FindByNameAsync(roleName);
        if (role is null)
            return (roleName, Array.Empty<string>());

        var permisos = await _context.RolPermisos
            .AsNoTracking()
            .Where(rp => rp.RolId == role.Id && rp.Concedido)
            .OrderBy(rp => rp.Permiso.Modulo)
            .ThenBy(rp => rp.Permiso.Clave)
            .Select(rp => rp.Permiso.Clave)
            .ToListAsync();

        return (roleName, permisos);
    }

    private static AuthUserDto ToAuthUserDto(ApplicationUser user, string roleName, IReadOnlyList<string> permisos)
    {
        return new AuthUserDto
        {
            Id = user.Id,
            Nombres = user.Nombres,
            Apellidos = user.Apellidos,
            Iniciales = user.Iniciales,
            Puesto = user.Puesto,
            Rol = roleName,
            Permisos = permisos
        };
    }

    private static string BuildInitials(string nombres, string apellidos)
    {
        var first = FirstLetter(nombres);
        var second = FirstLetter(apellidos);
        return $"{first}{second}".ToUpperInvariant();
    }

    private static string FirstLetter(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()[0].ToString();
    }

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded)
            return;

        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
