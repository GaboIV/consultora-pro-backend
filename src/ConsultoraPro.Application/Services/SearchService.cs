using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ConsultoraPro.Application.DTOs.Search;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConsultoraPro.Application.Services;

public class SearchService : ISearchService
{
    private const int DefaultPerTypeLimit = 3;
    private const int EmptyTypedLimit = 10;

    private static readonly IReadOnlySet<string> AllTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "proyecto",
        "cliente",
        "usuario",
        "credencial",
        "ambiente",
        "repositorio",
        "despliegue"
    };

    private readonly IServiceScopeFactory _scopeFactory;

    public SearchService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<SearchResultDto> SearchAsync(
        string query,
        IReadOnlyCollection<string> types,
        int limit,
        ClaimsPrincipal user)
    {
        var safeQuery = query.Trim();
        var safeLimit = Math.Clamp(limit, 1, 30);
        var requestedTypes = NormalizeTypes(types);
        var searchTypes = requestedTypes.Count == 0 ? AllTypes : requestedTypes;
        var context = await BuildSearchContextAsync(user);

        var tasks = new List<Task<List<SearchItemDto>>>();

        if (searchTypes.Contains("proyecto") && HasPermission(context, "proyectos.ver"))
            tasks.Add(SearchProyectosAsync(safeQuery, context));

        if (searchTypes.Contains("cliente") && HasPermission(context, "clientes.ver"))
            tasks.Add(SearchClientesAsync(safeQuery));

        if (searchTypes.Contains("usuario") && HasPermission(context, "roles.ver"))
            tasks.Add(SearchUsuariosAsync(safeQuery));

        if (searchTypes.Contains("credencial") && HasPermission(context, "credenciales.ver"))
            tasks.Add(SearchCredencialesAsync(safeQuery, context));

        if (searchTypes.Contains("ambiente") && HasPermission(context, "ambientes.ver"))
            tasks.Add(SearchAmbientesAsync(safeQuery, context));

        if (searchTypes.Contains("repositorio") && HasPermission(context, "proyectos.ver"))
            tasks.Add(SearchRepositoriosAsync(safeQuery, context));

        var matches = tasks.Count == 0
            ? new List<SearchItemDto>()
            : (await Task.WhenAll(tasks))
                .SelectMany(items => items)
                .ToList();

        var perTypeLimit = string.IsNullOrWhiteSpace(safeQuery) && requestedTypes.Count == 1
            ? EmptyTypedLimit
            : DefaultPerTypeLimit;

        var perTypeItems = matches
            .GroupBy(item => item.Type)
            .SelectMany(group => group
                .OrderByDescending(item => item.Score)
                .ThenByDescending(item => item.UpdatedAt)
                .Take(perTypeLimit))
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.UpdatedAt)
            .ToList();

        return new SearchResultDto
        {
            Items = perTypeItems.Take(safeLimit).ToList(),
            Total = matches.Count,
            Query = safeQuery
        };
    }

    private async Task<SearchContext> BuildSearchContextAsync(ClaimsPrincipal user)
    {
        var role = user.FindFirstValue("role") ?? string.Empty;
        var permissions = ReadPermissions(user)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var userIdValue = user.FindFirstValue("userId") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = Guid.TryParse(userIdValue, out var userId);

        if (IsPrivilegedRole(role))
            return new SearchContext(userId, role, permissions, null);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var projectRepository = scope.ServiceProvider.GetRequiredService<IProyectoRepository>();
        var proyectos = await projectRepository.GetAllAsync();
        var accessibleProjectIds = proyectos
            .Where(project => userId != Guid.Empty && project.ProyectoMiembros.Any(member => member.UsuarioId == userId))
            .Select(project => project.Id)
            .ToHashSet();

        return new SearchContext(userId, role, permissions, accessibleProjectIds);
    }

    private async Task<List<SearchItemDto>> SearchProyectosAsync(string query, SearchContext context)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProyectoRepository>();
        var proyectos = await repository.GetAllAsync();

        return proyectos
            .Where(project => CanSeeProject(project.Id, context))
            .Select(project =>
            {
                var leadNames = project.ProyectoMiembros
                    .Select(member => $"{member.Usuario.Nombres} {member.Usuario.Apellidos}")
                    .ToArray();
                var repoNames = project.Repositorios.Select(repo => repo.Nombre).ToArray();
                var score = CombinedScore(
                    project.Nombre,
                    query,
                    [project.Cliente.Nombre, project.Etapa.ToString(), project.Estado.ToString(), .. leadNames, .. repoNames]);

                return new SearchItemDto
                {
                    Id = project.Id.ToString(),
                    Type = "proyecto",
                    Name = project.Nombre,
                    Subtitle = $"{project.Cliente.Nombre} · {MapStageLabel(project.Etapa)} · {project.Progreso}%",
                    Badge = MapProjectStatusLabel(project.Estado),
                    BadgeVariant = MapProjectStatusTone(project.Estado),
                    Icon = "folder-kanban",
                    Score = score,
                    UpdatedAt = project.UpdatedAt,
                    NavigateTo = $"/proyectos?proyectoId={project.Id}"
                };
            })
            .Where(item => ShouldInclude(query, item.Score))
            .ToList();
    }

    private async Task<List<SearchItemDto>> SearchClientesAsync(string query)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
        var clientes = await repository.GetAllAsync();

        return clientes
            .Select(cliente =>
            {
                var score = CombinedScore(cliente.Nombre, query, [cliente.Industria]);
                return new SearchItemDto
                {
                    Id = cliente.Id.ToString(),
                    Type = "cliente",
                    Name = cliente.Nombre,
                    Subtitle = $"{cliente.TotalProyectos} proyectos · {cliente.Industria}",
                    Badge = cliente.Activo ? "Activo" : "Inactivo",
                    BadgeVariant = cliente.Activo ? "green" : "gray",
                    Icon = "building-skyscraper",
                    Score = score,
                    UpdatedAt = cliente.FechaAlta,
                    NavigateTo = $"/clientes?clienteId={cliente.Id}"
                };
            })
            .Where(item => ShouldInclude(query, item.Score))
            .ToList();
    }

    private async Task<List<SearchItemDto>> SearchUsuariosAsync(string query)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var users = await userManager.Users
            .AsNoTracking()
            .Where(user => user.Activo)
            .OrderBy(user => user.Nombres)
            .ThenBy(user => user.Apellidos)
            .ToListAsync();

        var items = new List<SearchItemDto>();
        foreach (var user in users)
        {
            var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? user.Puesto;
            var fullName = $"{user.Nombres} {user.Apellidos}".Trim();
            var score = CombinedScore(fullName, query, [user.Email ?? string.Empty, user.Puesto, user.Iniciales]);
            if (!ShouldInclude(query, score))
                continue;

            items.Add(new SearchItemDto
            {
                Id = user.Id.ToString(),
                Type = "usuario",
                Name = fullName,
                Subtitle = $"{user.Email} · {user.Puesto}",
                Badge = role,
                BadgeVariant = MapRoleTone(role),
                Icon = "user",
                Score = score,
                UpdatedAt = user.UltimoAcceso ?? user.FechaAlta,
                NavigateTo = $"/equipo/usuarios?usuarioId={user.Id}"
            });
        }

        return items;
    }

    private async Task<List<SearchItemDto>> SearchCredencialesAsync(string query, SearchContext context)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICredencialRepository>();
        var credenciales = await repository.GetAllAsync();

        return credenciales
            .Where(credencial => CanSeeProject(credencial.ProyectoId, context))
            .Select(credencial =>
            {
                var ambienteNombre = credencial.Ambiente?.Nombre ?? string.Empty;
                var score = CombinedScore(
                    credencial.Nombre,
                    query,
                    [credencial.Tipo.ToString(), credencial.Servidor, ambienteNombre, credencial.Proyecto.Nombre]);
                var days = DaysUntil(credencial.FechaVencimiento);

                return new SearchItemDto
                {
                    Id = credencial.Id.ToString(),
                    Type = "credencial",
                    Name = credencial.Nombre,
                    Subtitle = $"{credencial.Proyecto.Nombre} · {MapCredentialTypeLabel(credencial.Tipo)} · {ExpirationLabel(days)}",
                    Badge = ExpirationBadge(days),
                    BadgeVariant = ExpirationTone(days),
                    Icon = "lock",
                    Score = score,
                    UpdatedAt = credencial.UpdatedAt,
                    NavigateTo = $"/credenciales?proyectoId={credencial.ProyectoId}"
                };
            })
            .Where(item => ShouldInclude(query, item.Score))
            .ToList();
    }

    private async Task<List<SearchItemDto>> SearchAmbientesAsync(string query, SearchContext context)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAmbienteRepository>();
        var ambientes = await repository.GetAllAsync();

        return ambientes
            .Where(ambiente => CanSeeProject(ambiente.ProyectoId, context))
            .Select(ambiente =>
            {
                var score = CombinedScore(
                    ambiente.Nombre,
                    query,
                    [ambiente.Tipo.ToString(), ambiente.Url, ambiente.Tecnologia, ambiente.Proyecto.Nombre]);

                return new SearchItemDto
                {
                    Id = ambiente.Id.ToString(),
                    Type = "ambiente",
                    Name = ambiente.Nombre,
                    Subtitle = $"{ambiente.Proyecto.Nombre} · {ambiente.Tecnologia} · {MapEnvironmentStateLabel(ambiente.Estado)}",
                    Badge = MapEnvironmentStateLabel(ambiente.Estado),
                    BadgeVariant = MapEnvironmentStateTone(ambiente.Estado),
                    Icon = "server",
                    Score = score,
                    UpdatedAt = ambiente.FechaCreacion,
                    NavigateTo = $"/ambientes?proyectoId={ambiente.ProyectoId}"
                };
            })
            .Where(item => ShouldInclude(query, item.Score))
            .ToList();
    }

    private async Task<List<SearchItemDto>> SearchRepositoriosAsync(string query, SearchContext context)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepositorioRepository>();
        var repositorios = await repository.GetAllAsync();

        return repositorios
            .Where(repo => CanSeeProject(repo.ProyectoId, context))
            .Select(repo =>
            {
                var score = CombinedScore(
                    repo.Nombre,
                    query,
                    [repo.Proveedor.ToString(), repo.RamaPrincipal, repo.Proyecto.Nombre]);

                return new SearchItemDto
                {
                    Id = repo.Id.ToString(),
                    Type = "repositorio",
                    Name = repo.Nombre,
                    Subtitle = $"{repo.Proyecto.Nombre} · {MapRepositoryProviderLabel(repo.Proveedor)} · {repo.RamaPrincipal}",
                    Badge = MapPipelineLabel(repo.EstadoPipeline),
                    BadgeVariant = MapPipelineTone(repo.EstadoPipeline),
                    Icon = "git-branch",
                    Score = score,
                    UpdatedAt = repo.FechaCreacion,
                    NavigateTo = $"/repositorios?proyectoId={repo.ProyectoId}"
                };
            })
            .Where(item => ShouldInclude(query, item.Score))
            .ToList();
    }

    private static HashSet<string> NormalizeTypes(IReadOnlyCollection<string> types)
    {
        return types
            .Select(type => type.Trim().ToLowerInvariant())
            .Where(type => AllTypes.Contains(type))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool HasPermission(SearchContext context, string permission)
    {
        return context.Permissions.Contains(permission);
    }

    private static bool CanSeeProject(Guid projectId, SearchContext context)
    {
        return context.AccessibleProjectIds is null || context.AccessibleProjectIds.Contains(projectId);
    }

    private static bool IsPrivilegedRole(string role)
    {
        return role.Equals(PermissionCatalog.Arquitecto, StringComparison.OrdinalIgnoreCase)
            || role.Equals(PermissionCatalog.Gerencia, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> ReadPermissions(ClaimsPrincipal user)
    {
        foreach (var claim in user.FindAll("permisos"))
        {
            if (string.IsNullOrWhiteSpace(claim.Value))
                continue;

            if (!claim.Value.TrimStart().StartsWith("[", StringComparison.Ordinal))
            {
                yield return claim.Value;
                continue;
            }

            string[]? values;
            try
            {
                values = JsonSerializer.Deserialize<string[]>(claim.Value);
            }
            catch (JsonException)
            {
                values = [];
            }

            foreach (var permission in values ?? [])
                yield return permission;
        }
    }

    private static double CombinedScore(string name, string query, IReadOnlyCollection<string> secondaryFields)
    {
        if (string.IsNullOrWhiteSpace(query))
            return 0.5;

        var nameScore = CalculateScore(name, query);
        var secondaryScore = secondaryFields
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Select(field => CalculateScore(field, query))
            .DefaultIfEmpty(0)
            .Max();

        return Math.Max(nameScore, secondaryScore * 0.7);
    }

    private static double CalculateScore(string field, string query)
    {
        var normalizedField = NormalizeText(field);
        var normalizedQuery = NormalizeText(query);

        if (normalizedField == normalizedQuery) return 1.0;
        if (normalizedField.StartsWith(normalizedQuery, StringComparison.Ordinal)) return 0.9;
        if (normalizedField.Contains($" {normalizedQuery}", StringComparison.Ordinal)) return 0.8;
        if (normalizedField.Contains(normalizedQuery, StringComparison.Ordinal)) return 0.6;
        return 0.0;
    }

    private static string NormalizeText(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        return Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC), @"\s+", " ");
    }

    private static bool ShouldInclude(string query, double score)
    {
        return string.IsNullOrWhiteSpace(query) || score > 0;
    }

    private static int DaysUntil(DateTime date)
    {
        return (int)Math.Ceiling((date.Date - DateTime.UtcNow.Date).TotalDays);
    }

    private static string ExpirationLabel(int days) => days switch
    {
        < 0 => $"Vencida hace {Math.Abs(days)}d",
        0 => "Vence hoy",
        1 => "Vence mañana",
        _ => $"Vence en {days}d"
    };

    private static string ExpirationBadge(int days) => days switch
    {
        < 0 => "Vencida",
        <= 7 => ExpirationLabel(days),
        <= 30 => "Por vencer",
        _ => "OK"
    };

    private static string ExpirationTone(int days) => days switch
    {
        < 0 => "red",
        <= 7 => "red",
        <= 30 => "amber",
        _ => "green"
    };

    private static string MapStageLabel(EtapaProyecto etapa) => etapa switch
    {
        EtapaProyecto.Analisis => "Análisis",
        EtapaProyecto.Diseno => "Diseño",
        EtapaProyecto.Desarrollo => "Desarrollo",
        EtapaProyecto.QA => "QA",
        EtapaProyecto.Deploy => "Deploy",
        EtapaProyecto.Soporte => "Soporte",
        _ => etapa.ToString()
    };

    private static string MapProjectStatusLabel(EstadoProyecto estado) => estado switch
    {
        EstadoProyecto.Planificacion => "Planificación",
        EstadoProyecto.EnCurso => "En curso",
        EstadoProyecto.Completado => "Completado",
        EstadoProyecto.PorVencer => "Por vencer",
        _ => estado.ToString()
    };

    private static string MapProjectStatusTone(EstadoProyecto estado) => estado switch
    {
        EstadoProyecto.Planificacion => "blue",
        EstadoProyecto.EnCurso => "amber",
        EstadoProyecto.Completado => "green",
        EstadoProyecto.PorVencer => "red",
        _ => "gray"
    };

    private static string MapRoleTone(string role) => role switch
    {
        var value when value.Equals(PermissionCatalog.Arquitecto, StringComparison.OrdinalIgnoreCase) => "purple",
        var value when value.Equals(PermissionCatalog.Gerencia, StringComparison.OrdinalIgnoreCase) => "blue",
        var value when value.Equals(PermissionCatalog.LT, StringComparison.OrdinalIgnoreCase) => "teal",
        var value when value.Equals(PermissionCatalog.Dev, StringComparison.OrdinalIgnoreCase) => "green",
        _ => "gray"
    };

    private static string MapCredentialTypeLabel(TipoCredencial tipo) => tipo switch
    {
        TipoCredencial.BaseDatos => "Base de datos",
        TipoCredencial.SSHKey => "SSH Key",
        TipoCredencial.APIKey => "API Key",
        TipoCredencial.ServiceAccount => "Service Account",
        TipoCredencial.CertificadoSSL => "Certificado SSL",
        TipoCredencial.Otro => "Otro",
        _ => tipo.ToString()
    };

    private static string MapEnvironmentStateLabel(EstadoAmbiente estado) => estado switch
    {
        EstadoAmbiente.Online => "Online",
        EstadoAmbiente.Offline => "Offline",
        EstadoAmbiente.Alerta => "Alerta",
        EstadoAmbiente.Configurando => "Configurando",
        _ => estado.ToString()
    };

    private static string MapEnvironmentStateTone(EstadoAmbiente estado) => estado switch
    {
        EstadoAmbiente.Online => "green",
        EstadoAmbiente.Alerta => "amber",
        EstadoAmbiente.Offline => "red",
        EstadoAmbiente.Configurando => "blue",
        _ => "gray"
    };

    private static string MapRepositoryProviderLabel(ProveedorRepositorio proveedor) => proveedor switch
    {
        ProveedorRepositorio.AzureDevOps => "Azure DevOps",
        _ => proveedor.ToString()
    };

    private static string MapPipelineLabel(EstadoPipeline estado) => estado switch
    {
        EstadoPipeline.EnEjecucion => "En ejecución",
        EstadoPipeline.Desconocido => "Desconocido",
        _ => estado.ToString()
    };

    private static string MapPipelineTone(EstadoPipeline estado) => estado switch
    {
        EstadoPipeline.Passing => "green",
        EstadoPipeline.Failed => "red",
        EstadoPipeline.EnEjecucion => "amber",
        EstadoPipeline.Desconocido => "gray",
        _ => "gray"
    };

    private sealed record SearchContext(
        Guid UserId,
        string Role,
        HashSet<string> Permissions,
        HashSet<Guid>? AccessibleProjectIds);
}
