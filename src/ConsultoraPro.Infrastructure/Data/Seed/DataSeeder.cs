using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(
        AppDbContext context, 
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        if (await context.Clientes.AnyAsync())
        {
            await SeedAmbientesAsync(context);
            await SeedRepositoriosAsync(context);
            await SeedDesplieguesAsync(context);
            return;
        }

        var portalProveedores = Guid.NewGuid();
        var factElectronica = Guid.NewGuid();
        var host2Host = Guid.NewGuid();
        var guiasRemision = Guid.NewGuid();

        var tipos = new List<TipoSolucion>
        {
            new() { Id = portalProveedores, Nombre = "Portal de Proveedores" },
            new() { Id = factElectronica, Nombre = "Facturación Electrónica" },
            new() { Id = host2Host, Nombre = "Host2Host" },
            new() { Id = guiasRemision, Nombre = "Guías de Remisión" }
        };

        context.TiposSolucion.AddRange(tipos);

        var repsolId = Guid.NewGuid();
        var telefonicaId = Guid.NewGuid();
        var bbvaId = Guid.NewGuid();

        var clientes = new List<Cliente>
        {
            new()
            {
                Id = repsolId,
                Nombre = "Repsol",
                Industria = "Energía",
                Iniciales = "RE",
                ColorClass = "blue",
                Activo = true,
                FechaAlta = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = telefonicaId,
                Nombre = "Telefónica",
                Industria = "Telecomunicaciones",
                Iniciales = "TE",
                ColorClass = "purple",
                Activo = true,
                FechaAlta = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = bbvaId,
                Nombre = "BBVA",
                Industria = "Banca",
                Iniciales = "BB",
                ColorClass = "green",
                Activo = true,
                FechaAlta = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        context.Clientes.AddRange(clientes);

        // Seed Users instead of Members
        var devUsers = new List<(string Email, string Nombres, string Apellidos, string Puesto, string Role)>
        {
            ("carlos.ruiz@consultorapro.com", "Carlos", "Ruiz", "Developer", PermissionCatalog.Dev),
            ("jorge.mendez@consultorapro.com", "Jorge", "Méndez", "Developer", PermissionCatalog.Dev),
            ("sofia.luna@consultorapro.com", "Sofía", "Luna", "Developer", PermissionCatalog.Dev),
            ("ana.garcia@consultorapro.com", "Ana", "García", "Lead Technical", PermissionCatalog.Dev),
            ("maria.vega@consultorapro.com", "María", "Vega", "Lead Technical", PermissionCatalog.Dev),
            ("miguel.torres@consultorapro.com", "Miguel", "Torres", "Lead Technical", PermissionCatalog.Dev),
            ("andres.paredes@consultorapro.com", "Andrés", "Paredes", "Lead Technical", PermissionCatalog.Dev),
            ("laura.martinez@consultorapro.com", "Laura", "Martínez", "Lead Technical", PermissionCatalog.Dev),
            ("rodrigo.castillo@consultorapro.com", "Rodrigo", "Castillo", "Arquitecto", PermissionCatalog.Arquitecto)
        };

        var userMap = new Dictionary<string, Guid>();

        foreach (var u in devUsers)
        {
            var user = await userManager.FindByEmailAsync(u.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = u.Email,
                    Email = u.Email,
                    Nombres = u.Nombres,
                    Apellidos = u.Apellidos,
                    Telefono = "+51999000000",
                    Iniciales = $"{u.Nombres[0]}{u.Apellidos[0]}".ToUpper(),
                    Puesto = u.Puesto,
                    Activo = true,
                    EmailConfirmed = true,
                    FechaAlta = DateTime.UtcNow
                };

                // Use the prefix as password as per user request
                var password = u.Email.Split('@')[0];
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, u.Role);
                }
            }
            userMap[u.Email] = user.Id;
        }

        var proyectos = new List<Proyecto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = "Plataforma Digital Upstream",
                ClienteId = repsolId,
                TipoSolucionId = portalProveedores,
                Etapa = EtapaProyecto.Desarrollo,
                Estado = EstadoProyecto.EnCurso,
                Progreso = 60,
                FechaInicio = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                FechaFin = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                TotalMiembros = 3,
                ProyectoMiembros = new List<ProyectoMiembro>
                {
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["carlos.ruiz@consultorapro.com"], Rol = RolDesarrollador.Principal },
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["jorge.mendez@consultorapro.com"], Rol = RolDesarrollador.Apoyo },
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["sofia.luna@consultorapro.com"], Rol = RolDesarrollador.Apoyo }
                },
                CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = "App Mi Movistar",
                ClienteId = telefonicaId,
                TipoSolucionId = factElectronica,
                Etapa = EtapaProyecto.QA,
                Estado = EstadoProyecto.EnCurso,
                Progreso = 85,
                FechaInicio = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                FechaFin = new DateTime(2024, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                TotalMiembros = 3,
                ProyectoMiembros = new List<ProyectoMiembro>
                {
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["ana.garcia@consultorapro.com"], Rol = RolDesarrollador.Principal },
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["maria.vega@consultorapro.com"], Rol = RolDesarrollador.Principal },
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["carlos.ruiz@consultorapro.com"], Rol = RolDesarrollador.Apoyo }
                },
                CreatedAt = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 3, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = "Banca Digital Empresas",
                ClienteId = bbvaId,
                TipoSolucionId = host2Host,
                Etapa = EtapaProyecto.Diseno,
                Estado = EstadoProyecto.Planificacion,
                Progreso = 15,
                FechaInicio = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                FechaFin = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                TotalMiembros = 2,
                ProyectoMiembros = new List<ProyectoMiembro>
                {
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["miguel.torres@consultorapro.com"], Rol = RolDesarrollador.Principal },
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["andres.paredes@consultorapro.com"], Rol = RolDesarrollador.Apoyo }
                },
                CreatedAt = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Nombre = "CRM Comercial Repsol",
                ClienteId = repsolId,
                TipoSolucionId = guiasRemision,
                Etapa = EtapaProyecto.Deploy,
                Estado = EstadoProyecto.Completado,
                Progreso = 100,
                FechaInicio = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                FechaFin = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                TotalMiembros = 2,
                ProyectoMiembros = new List<ProyectoMiembro>
                {
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["laura.martinez@consultorapro.com"], Rol = RolDesarrollador.Principal },
                    new() { Id = Guid.NewGuid(), UsuarioId = userMap["rodrigo.castillo@consultorapro.com"], Rol = RolDesarrollador.Apoyo }
                },
                CreatedAt = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        context.Proyectos.AddRange(proyectos);
        await context.SaveChangesAsync();
        await SeedAmbientesAsync(context);
        await SeedRepositoriosAsync(context);
        await SeedDesplieguesAsync(context);
    }

    private static async Task SeedAmbientesAsync(AppDbContext context)
    {
        if (await context.Ambientes.AnyAsync())
            return;

        var proyectos = await context.Proyectos
            .Include(p => p.Cliente)
            .Include(p => p.TipoSolucion)
            .OrderBy(p => p.Cliente.Nombre)
            .ThenBy(p => p.Nombre)
            .ToListAsync();

        if (proyectos.Count == 0)
            return;

        var ambientes = new List<Ambiente>();

        foreach (var proyecto in proyectos.Take(4))
        {
            ambientes.Add(new Ambiente
            {
                Id = Guid.NewGuid(),
                Nombre = "Producción",
                Tipo = TipoAmbiente.Produccion,
                Url = $"https://{Slug(proyecto.Cliente.Nombre)}-{Slug(proyecto.Nombre)}.consultorapro.local",
                ProyectoId = proyecto.Id,
                Tecnologia = StackForProject(proyecto),
                Estado = proyecto.Estado == EstadoProyecto.Completado ? EstadoAmbiente.Online : EstadoAmbiente.Online,
                UptimePorcentaje = 99.72m,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });

            ambientes.Add(new Ambiente
            {
                Id = Guid.NewGuid(),
                Nombre = "Staging",
                Tipo = TipoAmbiente.Staging,
                Url = $"https://stg-{Slug(proyecto.Cliente.Nombre)}-{Slug(proyecto.Nombre)}.consultorapro.local",
                ProyectoId = proyecto.Id,
                Tecnologia = StackForProject(proyecto),
                Estado = proyecto.Estado == EstadoProyecto.EnCurso ? EstadoAmbiente.Alerta : EstadoAmbiente.Online,
                UptimePorcentaje = proyecto.Estado == EstadoProyecto.EnCurso ? 94.35m : 98.2m,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });

            ambientes.Add(new Ambiente
            {
                Id = Guid.NewGuid(),
                Nombre = "Desarrollo",
                Tipo = TipoAmbiente.Desarrollo,
                Url = $"https://dev-{Slug(proyecto.Cliente.Nombre)}-{Slug(proyecto.Nombre)}.consultorapro.local",
                ProyectoId = proyecto.Id,
                Tecnologia = "Docker Compose · MySQL",
                Estado = proyecto.Estado == EstadoProyecto.Planificacion ? EstadoAmbiente.Configurando : EstadoAmbiente.Online,
                UptimePorcentaje = proyecto.Estado == EstadoProyecto.Planificacion ? 87.5m : 97.1m,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        context.Ambientes.AddRange(ambientes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRepositoriosAsync(AppDbContext context)
    {
        if (await context.Repositorios.AnyAsync())
            return;

        var proyectos = await context.Proyectos
            .Include(p => p.Cliente)
            .Include(p => p.TipoSolucion)
            .OrderBy(p => p.Cliente.Nombre)
            .ThenBy(p => p.Nombre)
            .ToListAsync();

        if (proyectos.Count == 0)
            return;

        var repositorios = new List<Repositorio>();

        foreach (var proyecto in proyectos.Take(4))
        {
            repositorios.Add(new Repositorio
            {
                Id = Guid.NewGuid(),
                Nombre = $"{Slug(proyecto.Cliente.Nombre)}-{Slug(proyecto.Nombre)}",
                ProyectoId = proyecto.Id,
                Proveedor = ProveedorRepositorio.GitHub,
                RamaPrincipal = "main",
                Url = $"https://github.com/consultorapro/{Slug(proyecto.Cliente.Nombre)}-{Slug(proyecto.Nombre)}",
                EstadoPipeline = proyecto.Estado == EstadoProyecto.Completado ? EstadoPipeline.Passing : EstadoPipeline.EnEjecucion,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            });

            if (proyecto.Estado != EstadoProyecto.Planificacion)
            {
                repositorios.Add(new Repositorio
                {
                    Id = Guid.NewGuid(),
                    Nombre = $"{Slug(proyecto.Cliente.Nombre)}-{Slug(proyecto.Nombre)}-worker",
                    ProyectoId = proyecto.Id,
                    Proveedor = ProveedorRepositorio.GitHub,
                    RamaPrincipal = "develop",
                    Url = $"https://github.com/consultorapro/{Slug(proyecto.Cliente.Nombre)}-{Slug(proyecto.Nombre)}-worker",
                    EstadoPipeline = proyecto.Estado == EstadoProyecto.Completado ? EstadoPipeline.Passing : EstadoPipeline.Failed,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                });
            }
        }

        context.Repositorios.AddRange(repositorios);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDesplieguesAsync(AppDbContext context)
    {
        if (await context.Despliegues.AnyAsync())
            return;

        var proyectos = await context.Proyectos
            .Include(p => p.Cliente)
            .Include(p => p.Ambientes)
            .OrderBy(p => p.Cliente.Nombre)
            .ThenBy(p => p.Nombre)
            .ToListAsync();

        if (proyectos.Count == 0)
            return;

        var users = await context.Users
            .OrderBy(u => u.Nombres)
            .ToListAsync();

        if (users.Count == 0)
            return;

        var rng = new Random(42);
        var despliegues = new List<Despliegue>();

        foreach (var proyecto in proyectos.Take(3))
        {
            var ambientes = proyecto.Ambientes.Where(a => a.Activo).ToList();
            var prod = ambientes.FirstOrDefault(a => a.Tipo == TipoAmbiente.Produccion);
            var staging = ambientes.FirstOrDefault(a => a.Tipo == TipoAmbiente.Staging);

            var destinos = new[] { prod, staging }.Where(a => a is not null).Cast<Ambiente>().ToList();
            if (destinos.Count == 0) continue;

            var user = users[rng.Next(users.Count)];
            var baseDate = DateTime.UtcNow.AddDays(-rng.Next(1, 45));

            for (var i = 0; i < rng.Next(3, 7); i++)
            {
                var destino = destinos[rng.Next(destinos.Count)];
                var fecha = baseDate.AddDays(i * rng.Next(2, 6));
                var esExitoso = rng.NextDouble() > 0.2;

                despliegues.Add(new Despliegue
                {
                    Id = Guid.NewGuid(),
                    ProyectoId = proyecto.Id,
                    AmbienteId = destino.Id,
                    Version = $"2.{rng.Next(0, 9)}.{rng.Next(0, 20)}",
                    EjecutadoPorId = user.Id,
                    FechaHora = fecha,
                    Estado = esExitoso ? EstadoDespliegue.Exitoso : EstadoDespliegue.Fallido,
                    DuracionSegundos = rng.Next(30, 1800),
                    Notas = esExitoso ? "Despliegue automático completado." : "Fallo en validaciones post-deploy.",
                    Activo = true
                });
            }
        }

        context.Despliegues.AddRange(despliegues);
        await context.SaveChangesAsync();
    }

    private static string StackForProject(Proyecto proyecto)
    {
        return proyecto.TipoSolucion?.Nombre switch
        {
            "Host2Host" => ".NET 8 · Worker Service · MySQL",
            "Facturación Electrónica" => ".NET 8 · Angular · SQL Server",
            "Guías de Remisión" => ".NET 8 · Angular · Azure",
            _ => ".NET 8 · Angular · MySQL"
        };
    }

    private static string Slug(string value)
    {
        return value
            .ToLowerInvariant()
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ñ", "n")
            .Replace(" ", "-")
            .Replace(".", string.Empty);
    }
}
