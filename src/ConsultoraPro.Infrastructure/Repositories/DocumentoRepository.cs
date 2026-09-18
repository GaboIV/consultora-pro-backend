using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Repositories;

public class DocumentoRepository : IDocumentoRepository
{
    private readonly AppDbContext _context;

    public DocumentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CarpetaDocumento>> GetCarpetasAsync(Guid proyectoId)
    {
        return await _context.CarpetasDocumento
            .Where(c => c.ProyectoId == proyectoId)
            .OrderBy(c => c.Orden)
            .ThenBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<CarpetaDocumento?> GetCarpetaAsync(Guid id)
    {
        return await _context.CarpetasDocumento.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddCarpetasAsync(IEnumerable<CarpetaDocumento> carpetas)
    {
        _context.CarpetasDocumento.AddRange(carpetas);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCarpetaAsync(CarpetaDocumento carpeta)
    {
        _context.CarpetasDocumento.Remove(carpeta);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CarpetaTieneContenidoAsync(Guid carpetaId)
    {
        // Los documentos eliminados lógicamente también cuentan: conservan su FK a la carpeta.
        return await _context.CarpetasDocumento.AnyAsync(c => c.ParentId == carpetaId)
            || await _context.Documentos.AnyAsync(d => d.CarpetaId == carpetaId);
    }

    public async Task<List<Documento>> GetByProyectoAsync(Guid proyectoId)
    {
        return await _context.Documentos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(d => d.CreadoPor)
            .Include(d => d.ActualizadoPor)
            .Include(d => d.Etiquetas)
            .Include(d => d.Versiones).ThenInclude(v => v.SubidoPor)
            .Where(d => d.ProyectoId == proyectoId && d.Activo)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Documento?> GetByIdAsync(Guid id)
    {
        return await _context.Documentos
            .AsSplitQuery()
            .Include(d => d.Proyecto).ThenInclude(p => p.Cliente)
            .Include(d => d.CreadoPor)
            .Include(d => d.ActualizadoPor)
            .Include(d => d.Etiquetas)
            .Include(d => d.Versiones).ThenInclude(v => v.SubidoPor)
            .FirstOrDefaultAsync(d => d.Id == id && d.Activo);
    }

    public async Task<DocumentoVersion?> GetVersionAsync(Guid documentoId, Guid versionId)
    {
        return await _context.DocumentoVersiones
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == versionId && v.DocumentoId == documentoId);
    }

    public async Task<int> CountByTipoAsync(Guid proyectoId, TipoDocumento tipo)
    {
        // Incluye inactivos para que un código nunca se reutilice tras un borrado lógico.
        return await _context.Documentos.CountAsync(d => d.ProyectoId == proyectoId && d.Tipo == tipo);
    }

    public async Task AddAsync(Documento documento)
    {
        _context.Documentos.Add(documento);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Documento>> SearchAsync(IReadOnlyCollection<string> terms, IReadOnlySet<Guid>? proyectoIds, int take)
    {
        var query = _context.Documentos
            .AsNoTracking()
            .Where(d => d.Activo);

        if (proyectoIds is not null)
        {
            var ids = proyectoIds.ToList();
            query = query.Where(d => ids.Contains(d.ProyectoId));
        }

        // AND entre términos, OR entre campos. La collation por defecto de MySQL (utf8mb4_*_ci)
        // resuelve mayúsculas/acentos en el LIKE.
        foreach (var term in terms)
        {
            var pattern = $"%{EscapeLike(term)}%";
            query = query.Where(d =>
                EF.Functions.Like(d.Titulo, pattern)
                || EF.Functions.Like(d.Codigo, pattern)
                || EF.Functions.Like(d.NombreArchivo, pattern)
                || EF.Functions.Like(d.Descripcion, pattern)
                || EF.Functions.Like(d.Carpeta.Nombre, pattern)
                || d.Etiquetas.Any(e => EF.Functions.Like(e.Nombre, pattern)));
        }

        return await query
            .AsSplitQuery()
            .Include(d => d.Proyecto).ThenInclude(p => p.Cliente)
            .Include(d => d.Carpeta)
            .Include(d => d.Etiquetas)
            .OrderByDescending(d => d.UpdatedAt)
            .Take(take)
            .ToListAsync();
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
