using ConsultoraPro.Application.Kanban;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class TarjetaRepository : ITarjetaRepository
{
    private readonly AppDbContext _context;

    public TarjetaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tarjeta?> GetByIdAsync(Guid id)
    {
        return await _context.Tarjetas
            .Include(t => t.Tablero)
            .Include(t => t.Columna)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tarjeta?> GetDetalleAsync(Guid id)
    {
        return await _context.Tarjetas
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Columna)
            .Include(t => t.Tablero)
                .ThenInclude(tb => tb.Proyecto)
            .Include(t => t.CreadaPor)
            .Include(t => t.Responsables)
                .ThenInclude(r => r.Usuario)
            .Include(t => t.Etiquetas)
                .ThenInclude(te => te.Etiqueta)
            .Include(t => t.Checklist)
            .Include(t => t.Comentarios)
                .ThenInclude(c => c.Autor)
            .Include(t => t.Adjuntos)
                .ThenInclude(a => a.SubidoPor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tarjeta?> GetWithResponsablesAsync(Guid id)
    {
        return await _context.Tarjetas
            .Include(t => t.Responsables)
                .ThenInclude(r => r.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tarjeta?> GetWithEtiquetasAsync(Guid id)
    {
        return await _context.Tarjetas
            .Include(t => t.Etiquetas)
                .ThenInclude(te => te.Etiqueta)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tarjeta?> GetWithChecklistAsync(Guid id)
    {
        return await _context.Tarjetas
            .Include(t => t.Checklist)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tarjeta?> GetWithComentariosAsync(Guid id)
    {
        return await _context.Tarjetas
            .Include(t => t.Comentarios)
                .ThenInclude(c => c.Autor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tarjeta?> GetWithAdjuntosAsync(Guid id)
    {
        return await _context.Tarjetas
            .Include(t => t.Adjuntos)
                .ThenInclude(a => a.SubidoPor)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Tarjeta>> GetActiveByColumnaAsync(Guid columnaId)
    {
        return await _context.Tarjetas
            .AsNoTracking()
            .Where(t => t.ColumnaId == columnaId && t.Activo)
            .OrderBy(t => t.Orden)
            .ToListAsync();
    }

    public async Task<Tarjeta> CreateAsync(Tarjeta tarjeta)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Bloqueo pesimista sobre la fila del tablero para serializar la asignación del número
        // (dos creaciones simultáneas en el mismo tablero no pueden obtener el mismo código).
        var tableros = await _context.Tableros
            .FromSqlRaw("SELECT * FROM `Tableros` WHERE `Id` = {0} LIMIT 1 FOR UPDATE", tarjeta.TableroId)
            .ToListAsync();
        var tablero = tableros.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Tablero con ID {tarjeta.TableroId} no encontrado");

        string? proyectoClave = null;
        if (tablero.ProyectoId.HasValue)
        {
            proyectoClave = await _context.Proyectos
                .Where(p => p.Id == tablero.ProyectoId.Value)
                .Select(p => p.Clave)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(proyectoClave))
                proyectoClave = KanbanCodeHelper.DeriveKey("PRO", 3);
        }

        var nuevoNumero = tablero.SecuenciaActual + 1;
        tablero.SecuenciaActual = nuevoNumero;
        tarjeta.Numero = nuevoNumero;
        tarjeta.Codigo = KanbanCodeHelper.FormatCodigo(proyectoClave, tablero.Clave, nuevoNumero);

        _context.Tarjetas.Add(tarjeta);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return tarjeta;
    }

    public async Task UpdateAsync(Tarjeta tarjeta)
    {
        await _context.SaveChangesAsync();
    }

    public async Task AddChildrenAndSaveAsync(params object[] entidadesNuevas)
    {
        // _context.Add fija el estado Added (y propaga a los hijos no rastreados), evitando
        // que DetectChanges interprete una clave ya asignada como una fila existente.
        foreach (var entidad in entidadesNuevas)
            _context.Add(entidad);

        await _context.SaveChangesAsync();
    }

    public async Task AddActividadAsync(ActividadTarjeta actividad)
    {
        _context.ActividadesTarjeta.Add(actividad);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ActividadTarjeta>> GetActividadAsync(Guid tarjetaId)
    {
        return await _context.ActividadesTarjeta
            .AsNoTracking()
            .Include(a => a.Usuario)
            .Where(a => a.TarjetaId == tarjetaId)
            .OrderByDescending(a => a.Fecha)
            .ToListAsync();
    }
}
