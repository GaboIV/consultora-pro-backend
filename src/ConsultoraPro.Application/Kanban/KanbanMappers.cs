using ConsultoraPro.Application.DTOs.Kanban;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Kanban;

/// <summary>
/// Mapeos entidad → DTO del módulo Kanban. Manuales (como AmbienteService) para tener control
/// total sobre proyecciones calculadas: avatares de responsables, conteos de checklist,
/// comentarios y adjuntos usados en los badges del board.
/// </summary>
public static class KanbanMappers
{
    private static string NombreCompleto(ApplicationUser? u) =>
        u is null ? "Usuario desconocido" : $"{u.Nombres} {u.Apellidos}".Trim();

    public static EtiquetaDto ToDto(EtiquetaKanban e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        ColorClass = e.ColorClass
    };

    public static ResponsableDto ToDto(TarjetaResponsable r) => new()
    {
        UsuarioId = r.UsuarioId,
        NombreCompleto = NombreCompleto(r.Usuario),
        Iniciales = r.Usuario?.Iniciales ?? string.Empty
    };

    public static TableroMiembroDto ToDto(TableroMiembro m) => new()
    {
        UsuarioId = m.UsuarioId,
        NombreCompleto = NombreCompleto(m.Usuario),
        Iniciales = m.Usuario?.Iniciales ?? string.Empty,
        Rol = m.Rol
    };

    public static ChecklistItemDto ToDto(ChecklistItem c) => new()
    {
        Id = c.Id,
        ChecklistId = c.ChecklistId,
        Texto = c.Texto,
        Completado = c.Completado,
        Orden = c.Orden
    };

    public static ChecklistDto ToDto(Checklist c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Orden = c.Orden,
        Items = c.Items.OrderBy(i => i.Orden).Select(ToDto).ToList()
    };

    public static ComentarioDto ToDto(ComentarioTarjeta c) => new()
    {
        Id = c.Id,
        Texto = c.Texto,
        AutorId = c.AutorId,
        AutorNombre = NombreCompleto(c.Autor),
        AutorIniciales = c.Autor?.Iniciales ?? string.Empty,
        FechaCreacion = c.FechaCreacion,
        EditadoEn = c.EditadoEn
    };

    // Url lleva temporalmente la StorageKey; el servicio la firma (SAS) antes de devolver el DTO.
    public static AdjuntoDto ToDto(AdjuntoTarjeta a) => new()
    {
        Id = a.Id,
        Nombre = a.Nombre,
        Url = a.StorageKey,
        ContentType = a.ContentType,
        TamanoBytes = a.TamanoBytes,
        SubidoPorId = a.SubidoPorId,
        SubidoPorNombre = a.SubidoPor is null ? null : NombreCompleto(a.SubidoPor),
        FechaSubida = a.FechaSubida
    };

    public static ActividadDto ToDto(ActividadTarjeta a) => new()
    {
        Id = a.Id,
        Tipo = a.Tipo,
        Detalle = a.Detalle,
        UsuarioId = a.UsuarioId,
        UsuarioNombre = a.Usuario is null ? null : NombreCompleto(a.Usuario),
        Fecha = a.Fecha
    };

    public static TarjetaDto ToDto(Tarjeta t)
    {
        return new TarjetaDto
        {
            Id = t.Id,
            ColumnaId = t.ColumnaId,
            TableroId = t.TableroId,
            Numero = t.Numero,
            Codigo = t.Codigo,
            Titulo = t.Titulo,
            Orden = t.Orden,
            Prioridad = t.Prioridad,
            FechaLimite = t.FechaLimite,
            Completada = t.Completada,
            Responsables = t.Responsables.Select(ToDto).ToList(),
            Etiquetas = t.Etiquetas.Where(e => e.Etiqueta is not null).Select(e => ToDto(e.Etiqueta)).ToList(),
            ChecklistCompletados = t.Checklists.Sum(cl => cl.Items.Count(i => i.Completado)),
            ChecklistTotal = t.Checklists.Sum(cl => cl.Items.Count),
            TotalComentarios = t.Comentarios.Count,
            TotalAdjuntos = t.Adjuntos.Count,
            Descripcion = t.Descripcion,
            // Lleva la StorageKey de la portada; el servicio la firma antes de devolver el DTO.
            PortadaAdjuntoUrl = t.Adjuntos
                .Where(a => a.ContentType != null && a.ContentType.StartsWith("image/"))
                .OrderBy(a => a.FechaSubida)
                .Select(a => a.StorageKey)
                .FirstOrDefault()
        };
    }

    public static TarjetaDetalleDto ToDetalleDto(Tarjeta t)
    {
        return new TarjetaDetalleDto
        {
            Id = t.Id,
            ColumnaId = t.ColumnaId,
            TableroId = t.TableroId,
            Numero = t.Numero,
            Codigo = t.Codigo,
            Titulo = t.Titulo,
            Orden = t.Orden,
            Prioridad = t.Prioridad,
            FechaLimite = t.FechaLimite,
            Completada = t.Completada,
            Responsables = t.Responsables.Select(ToDto).ToList(),
            Etiquetas = t.Etiquetas.Where(e => e.Etiqueta is not null).Select(e => ToDto(e.Etiqueta)).ToList(),
            ChecklistCompletados = t.Checklists.Sum(cl => cl.Items.Count(i => i.Completado)),
            ChecklistTotal = t.Checklists.Sum(cl => cl.Items.Count),
            TotalComentarios = t.Comentarios.Count,
            TotalAdjuntos = t.Adjuntos.Count,
            Descripcion = t.Descripcion,
            // Lleva la StorageKey de la portada; el servicio la firma antes de devolver el DTO.
            PortadaAdjuntoUrl = t.Adjuntos
                .Where(a => a.ContentType != null && a.ContentType.StartsWith("image/"))
                .OrderBy(a => a.FechaSubida)
                .Select(a => a.StorageKey)
                .FirstOrDefault(),
            FechaInicio = t.FechaInicio,
            FechaCreacion = t.FechaCreacion,
            UpdatedAt = t.UpdatedAt,
            CreadaPorId = t.CreadaPorId,
            CreadaPorNombre = t.CreadaPor is null ? null : NombreCompleto(t.CreadaPor),
            Checklists = t.Checklists.OrderBy(c => c.Orden).Select(ToDto).ToList(),
            Comentarios = t.Comentarios.OrderByDescending(c => c.FechaCreacion).Select(ToDto).ToList(),
            Adjuntos = t.Adjuntos.OrderByDescending(a => a.FechaSubida).Select(ToDto).ToList()
        };
    }

    public static ColumnaDto ToDto(ColumnaKanban c)
    {
        return new ColumnaDto
        {
            Id = c.Id,
            TableroId = c.TableroId,
            Nombre = c.Nombre,
            Orden = c.Orden,
            LimiteWip = c.LimiteWip,
            Tarjetas = c.Tarjetas
                .Where(t => t.Activo)
                .OrderBy(t => t.Orden)
                .Select(ToDto)
                .ToList()
        };
    }

    public static TableroDto ToDto(Tablero t)
    {
        var columnasActivas = t.Columnas.Where(c => c.Activo).ToList();
        return new TableroDto
        {
            Id = t.Id,
            ProyectoId = t.ProyectoId,
            ProyectoNombre = t.Proyecto?.Nombre,
            EsPersonal = !t.ProyectoId.HasValue,
            Nombre = t.Nombre,
            Clave = t.Clave,
            Descripcion = t.Descripcion,
            ColorClass = t.ColorClass,
            Orden = t.Orden,
            TotalColumnas = columnasActivas.Count,
            TotalTarjetas = columnasActivas.SelectMany(c => c.Tarjetas).Count(ta => ta.Activo),
            TotalMiembros = t.Miembros.Count,
            Activo = t.Activo,
            FechaCreacion = t.FechaCreacion
        };
    }

    public static TableroDetalleDto ToDetalleDto(Tablero t)
    {
        return new TableroDetalleDto
        {
            Id = t.Id,
            ProyectoId = t.ProyectoId,
            ProyectoNombre = t.Proyecto?.Nombre,
            ProyectoClave = t.Proyecto?.Clave,
            EsPersonal = !t.ProyectoId.HasValue,
            Nombre = t.Nombre,
            Clave = t.Clave,
            Descripcion = t.Descripcion,
            ColorClass = t.ColorClass,
            Orden = t.Orden,
            Columnas = t.Columnas
                .Where(c => c.Activo)
                .OrderBy(c => c.Orden)
                .Select(ToDto)
                .ToList(),
            Etiquetas = t.Etiquetas
                .Where(e => e.Activo)
                .OrderBy(e => e.Nombre)
                .Select(ToDto)
                .ToList(),
            Miembros = t.Miembros
                .OrderBy(m => m.Rol)
                .Select(ToDto)
                .ToList()
        };
    }
}
