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
        if (await context.Clientes.AnyAsync()) return;

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
    }
}
