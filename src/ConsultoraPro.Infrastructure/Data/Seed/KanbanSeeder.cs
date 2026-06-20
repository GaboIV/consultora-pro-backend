using ConsultoraPro.Application.Kanban;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Data.Seed;

public static class KanbanSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await BackfillProyectoClavesAsync(context);
        await SeedDemoBoardAsync(context);
    }

    /// <summary>
    /// Asigna una clave única a los proyectos que aún no la tienen (derivada del nombre),
    /// para que la generación de códigos de tarjeta funcione en bases ya pobladas.
    /// </summary>
    private static async Task BackfillProyectoClavesAsync(AppDbContext context)
    {
        var proyectos = await context.Proyectos.ToListAsync();
        if (proyectos.Count == 0)
            return;

        var usadas = proyectos
            .Where(p => !string.IsNullOrWhiteSpace(p.Clave))
            .Select(p => p.Clave)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var cambios = false;
        foreach (var proyecto in proyectos.Where(p => string.IsNullOrWhiteSpace(p.Clave)))
        {
            var baseKey = KanbanCodeHelper.DeriveKey(proyecto.Nombre, 3);
            var clave = baseKey;
            var suffix = 1;
            while (usadas.Contains(clave))
            {
                clave = $"{baseKey}{suffix}";
                suffix++;
            }

            proyecto.Clave = clave;
            usadas.Add(clave);
            cambios = true;
        }

        if (cambios)
            await context.SaveChangesAsync();
    }

    private static async Task SeedDemoBoardAsync(AppDbContext context)
    {
        if (await context.Tableros.AnyAsync())
            return;

        var proyecto = await context.Proyectos
            .OrderBy(p => p.Nombre)
            .FirstOrDefaultAsync();
        if (proyecto is null)
            return;

        var usuario = await context.Users.OrderBy(u => u.Nombres).FirstOrDefaultAsync();

        var tablero = new Tablero
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyecto.Id,
            Nombre = "Tareas",
            Clave = "TAR",
            Descripcion = "Tablero de tareas del proyecto",
            ColorClass = "blue",
            Orden = 1,
            Activo = true
        };

        var porHacer = new ColumnaKanban { Id = Guid.NewGuid(), Nombre = "Por hacer", Orden = FractionalOrder.Step, Activo = true };
        var enProgreso = new ColumnaKanban { Id = Guid.NewGuid(), Nombre = "En progreso", Orden = FractionalOrder.Step * 2, Activo = true };
        var hecho = new ColumnaKanban { Id = Guid.NewGuid(), Nombre = "Hecho", Orden = FractionalOrder.Step * 3, Activo = true };
        tablero.Columnas.Add(porHacer);
        tablero.Columnas.Add(enProgreso);
        tablero.Columnas.Add(hecho);

        var etiquetaBackend = new EtiquetaKanban { Id = Guid.NewGuid(), Nombre = "Backend", ColorClass = "purple", Activo = true };
        var etiquetaUrgente = new EtiquetaKanban { Id = Guid.NewGuid(), Nombre = "Urgente", ColorClass = "red", Activo = true };
        tablero.Etiquetas.Add(etiquetaBackend);
        tablero.Etiquetas.Add(etiquetaUrgente);

        var demos = new[]
        {
            (Col: porHacer, Titulo: "Configurar pipeline CI/CD", Prioridad: PrioridadTarjeta.Alta, Completada: false, Etiqueta: etiquetaBackend),
            (Col: porHacer, Titulo: "Diseñar modelo de datos", Prioridad: PrioridadTarjeta.Media, Completada: false, Etiqueta: (EtiquetaKanban?)null),
            (Col: enProgreso, Titulo: "Implementar autenticación JWT", Prioridad: PrioridadTarjeta.Critica, Completada: false, Etiqueta: etiquetaUrgente),
            (Col: hecho, Titulo: "Levantar esqueleto del proyecto", Prioridad: PrioridadTarjeta.Baja, Completada: true, Etiqueta: (EtiquetaKanban?)null)
        };

        var numero = 0;
        foreach (var demo in demos)
        {
            numero++;
            var tarjeta = new Tarjeta
            {
                Id = Guid.NewGuid(),
                ColumnaId = demo.Col.Id,
                TableroId = tablero.Id,
                Numero = numero,
                Codigo = KanbanCodeHelper.FormatCodigo(proyecto.Clave, tablero.Clave, numero),
                Titulo = demo.Titulo,
                Orden = FractionalOrder.Step,
                Prioridad = demo.Prioridad,
                Completada = demo.Completada,
                CreadaPorId = usuario?.Id,
                Activo = true
            };

            if (usuario is not null)
            {
                tarjeta.Responsables.Add(new TarjetaResponsable
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = usuario.Id
                });
            }

            if (demo.Etiqueta is not null)
            {
                tarjeta.Etiquetas.Add(new TarjetaEtiqueta { EtiquetaId = demo.Etiqueta.Id });
            }

            demo.Col.Tarjetas.Add(tarjeta);
        }

        tablero.SecuenciaActual = numero;
        context.Tableros.Add(tablero);
        await context.SaveChangesAsync();
    }
}
