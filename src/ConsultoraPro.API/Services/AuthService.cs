using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ConsultoraPro.API.Interfaces;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.DTOs.Auth;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ConsultoraPro.API.Services;

public class AuthService : IAuthService
{
    /// <summary>Proveedor del login externo de Google en la tabla AspNetUserLogins.</summary>
    private const string GoogleLoginProvider = "Google";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly AuthOptions _authOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        AppDbContext context,
        IConfiguration configuration,
        IOptions<AuthOptions> authOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _authOptions = authOptions.Value;
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
        // En modo "solo Google" (CredentialsEnabled=false) el login por contraseña solo se
        // admite para los emails break-glass; para el resto se rechaza igual que un credencial inválido.
        if (!_authOptions.IsPasswordLoginAllowed(dto.Email))
            throw new UnauthorizedAccessException("El acceso por contraseña está deshabilitado");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !user.Activo || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        user.UltimoAcceso = DateTime.UtcNow;
        ThrowIfFailed(await _userManager.UpdateAsync(user));

        return await GenerateTokenAsync(user);
    }

    public async Task<AuthResponseDto> LoginWithGoogleAsync(string email, string? fullName, string? pictureUrl)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new UnauthorizedAccessException("Google no proporcionó un email");

        email = email.Trim();

        if (!_authOptions.Google.IsDomainAllowed(email))
            throw new GoogleDomainNotAllowedException($"El dominio de {email} no está autorizado");

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            user = await ProvisionGoogleUserAsync(email, fullName, pictureUrl);
        else if (!user.Activo)
            throw new UnauthorizedAccessException("Usuario inactivo");

        // Vincula el login externo (AspNetUserLogins) si todavía no existe, y refresca el avatar.
        await EnsureGoogleLoginLinkedAsync(user, email);
        if (!string.IsNullOrWhiteSpace(pictureUrl) && user.AvatarUrl != pictureUrl)
            user.AvatarUrl = pictureUrl;

        user.UltimoAcceso = DateTime.UtcNow;
        ThrowIfFailed(await _userManager.UpdateAsync(user));

        return await GenerateTokenAsync(user);
    }

    // Crea un usuario nuevo a partir de su cuenta de Google. Se crea SIN rol: queda autenticado
    // pero sin permisos hasta que un administrador le asigne un rol desde el módulo de equipo.
    private async Task<ApplicationUser> ProvisionGoogleUserAsync(string email, string? fullName, string? pictureUrl)
    {
        var (nombres, apellidos) = SplitFullName(fullName, email);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Nombres = nombres,
            Apellidos = apellidos,
            Iniciales = BuildInitials(nombres, apellidos),
            Puesto = string.Empty,
            Telefono = string.Empty,
            Activo = true,
            AuthProvider = "google",
            AvatarUrl = pictureUrl,
            FechaAlta = DateTime.UtcNow
        };

        ThrowIfFailed(await _userManager.CreateAsync(user));
        return user;
    }

    private async Task EnsureGoogleLoginLinkedAsync(ApplicationUser user, string providerKey)
    {
        var logins = await _userManager.GetLoginsAsync(user);
        if (logins.Any(l => l.LoginProvider == GoogleLoginProvider))
            return;

        ThrowIfFailed(await _userManager.AddLoginAsync(
            user, new UserLoginInfo(GoogleLoginProvider, providerKey, GoogleLoginProvider)));
    }

    private static (string Nombres, string Apellidos) SplitFullName(string? fullName, string email)
    {
        var name = (fullName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name))
        {
            var local = email.Split('@')[0];
            return (local, string.Empty);
        }

        var parts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 1 ? (parts[0], string.Empty) : (parts[0], parts[1]);
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

    public async Task<AuthResponseDto> UpdatePerfilAsync(Guid userId, UpdatePerfilDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.Activo)
            throw new KeyNotFoundException("Usuario no encontrado o inactivo");

        user.Nombres = dto.Nombres.Trim();
        user.Apellidos = dto.Apellidos.Trim();
        user.Telefono = dto.Telefono.Trim();
        user.Iniciales = string.IsNullOrWhiteSpace(dto.Iniciales)
            ? BuildInitials(dto.Nombres, dto.Apellidos)
            : dto.Iniciales.Trim().ToUpperInvariant();

        var result = await _userManager.UpdateAsync(user);
        ThrowIfFailed(result);

        return await GenerateTokenAsync(user);
    }

    private async Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user)
    {
        var (roleName, permisos) = await GetRoleAndPermissionsAsync(user);
        var accesoTotal = await HasFullProjectAccessAsync(roleName);
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
            new("telefono", user.Telefono),
            new("fechaAlta", user.FechaAlta.ToString("o")),
            new("ultimoAcceso", user.UltimoAcceso?.ToString("o") ?? string.Empty),
            new("role", roleName),
            new("accesoTotalProyectos", accesoTotal ? "true" : "false"),
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

    private async Task<bool> HasFullProjectAccessAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        var role = await _roleManager.FindByNameAsync(roleName);
        return role?.AccesoTotalProyectos ?? false;
    }

    private static AuthUserDto ToAuthUserDto(ApplicationUser user, string roleName, IReadOnlyList<string> permisos)
    {
        return new AuthUserDto
        {
            Id = user.Id,
            Nombres = user.Nombres,
            Apellidos = user.Apellidos,
            Iniciales = user.Iniciales,
            Email = user.Email ?? string.Empty,
            Telefono = user.Telefono,
            Puesto = user.Puesto,
            Rol = roleName,
            FechaAlta = user.FechaAlta,
            UltimoAcceso = user.UltimoAcceso,
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
