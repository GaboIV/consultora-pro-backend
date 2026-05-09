using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
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
                TechLead = "Carlos Ruiz",
                TechLeadIniciales = "CR",
                TotalMiembros = 12,
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
                TechLead = "Ana García",
                TechLeadIniciales = "AG",
                TotalMiembros = 20,
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
                TechLead = "Miguel Torres",
                TechLeadIniciales = "MT",
                TotalMiembros = 8,
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
                TechLead = "Laura Martínez",
                TechLeadIniciales = "LM",
                TotalMiembros = 6,
                CreatedAt = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        context.Proyectos.AddRange(proyectos);
        await context.SaveChangesAsync();
    }
}
