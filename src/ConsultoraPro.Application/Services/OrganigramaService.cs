using ConsultoraPro.Application.DTOs.Organigramas;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class OrganigramaService : IOrganigramaService
{
    private readonly IOrganigramaRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public OrganigramaService(IOrganigramaRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<List<OrganigramaResumenDto>> GetAllAsync()
    {
        var organigramas = await _repository.GetAllAsync();
        var stats = await _repository.GetNodoStatsAsync();

        return organigramas
            .Select(o =>
            {
                stats.TryGetValue(o.Id, out var s);
                return new OrganigramaResumenDto
                {
                    Id = o.Id,
                    Nombre = o.Nombre,
                    Descripcion = o.Descripcion,
                    TotalNodos = s.Nodos,
                    TotalAsignados = s.Asignados,
                    FechaCreacion = o.FechaCreacion,
                    FechaActualizacion = o.FechaActualizacion
                };
            })
            .ToList();
    }

    public async Task<OrganigramaDto?> GetByIdAsync(Guid id)
    {
        var organigrama = await _repository.GetByIdAsync(id);
        if (organigrama == null) return null;

        var usuarios = organigrama.Nodos
            .Where(n => n.Usuario != null)
            .Select(n => n.Usuario!)
            .DistinctBy(u => u.Id)
            .ToDictionary(u => u.Id);

        return ToDto(organigrama, organigrama.Nodos, usuarios);
    }

    public async Task<List<OrganigramaUsuarioDto>> GetUsuariosDisponiblesAsync()
    {
        var usuarios = await _repository.GetUsuariosDisponiblesAsync();
        return usuarios.Select(ToUsuarioDto).ToList();
    }

    public async Task<OrganigramaDto> CreateAsync(GuardarOrganigramaDto dto)
    {
        var nombre = dto.Nombre.Trim();
        if (await _repository.ExistsByNombreAsync(nombre))
            throw new InvalidOperationException($"Ya existe un organigrama con el nombre '{nombre}'.");

        var organigrama = new Organigrama
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
            CreadoPorId = _currentUser.UserId,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        var (nodos, usuarios) = await BuildNodosAsync(organigrama.Id, dto.Nodos);
        foreach (var nodo in nodos)
            organigrama.Nodos.Add(nodo);

        await _repository.CreateAsync(organigrama);
        return ToDto(organigrama, nodos, usuarios);
    }

    public async Task<OrganigramaDto> UpdateAsync(Guid id, GuardarOrganigramaDto dto)
    {
        var organigrama = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Organigrama con ID {id} no encontrado");

        var nombre = dto.Nombre.Trim();
        if (await _repository.ExistsByNombreAsync(nombre, id))
            throw new InvalidOperationException($"Ya existe un organigrama con el nombre '{nombre}'.");

        var (nodos, usuarios) = await BuildNodosAsync(id, dto.Nodos);

        organigrama.Nombre = nombre;
        organigrama.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
        organigrama.FechaActualizacion = DateTime.UtcNow;

        await _repository.UpdateAsync(organigrama, nodos);
        return ToDto(organigrama, nodos, usuarios);
    }

    public async Task DeleteAsync(Guid id)
    {
        var organigrama = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Organigrama con ID {id} no encontrado");

        await _repository.DeleteAsync(organigrama);
    }

    /// <summary>
    /// Valida la estructura enviada (ids únicos, padres existentes, sin ciclos, usuarios válidos)
    /// y la convierte en entidades con el orden normalizado por nivel.
    /// </summary>
    private async Task<(List<OrganigramaNodo> Nodos, Dictionary<Guid, ApplicationUser> Usuarios)> BuildNodosAsync(
        Guid organigramaId,
        IReadOnlyList<GuardarOrganigramaNodoDto> input)
    {
        var ids = new HashSet<Guid>();
        foreach (var nodo in input)
        {
            if (!ids.Add(nodo.Id))
                throw new InvalidOperationException("La estructura contiene posiciones duplicadas.");
        }

        var parentById = input.ToDictionary(n => n.Id, n => n.ParentId);
        foreach (var nodo in input)
        {
            if (nodo.ParentId is not { } parentId) continue;
            if (parentId == nodo.Id)
                throw new InvalidOperationException($"La posición '{nodo.Cargo}' no puede depender de sí misma.");
            if (!ids.Contains(parentId))
                throw new InvalidOperationException($"La posición '{nodo.Cargo}' depende de una posición que no existe.");
        }

        foreach (var nodo in input)
        {
            var visitados = new HashSet<Guid> { nodo.Id };
            var actual = nodo.ParentId;
            while (actual is { } parentId)
            {
                if (!visitados.Add(parentId))
                    throw new InvalidOperationException($"La posición '{nodo.Cargo}' forma un ciclo en la jerarquía.");
                actual = parentById[parentId];
            }
        }

        var ajenos = await _repository.GetNodoIdsEnOtrosOrganigramasAsync(ids, organigramaId);
        if (ajenos.Count > 0)
            throw new InvalidOperationException("La estructura contiene posiciones que pertenecen a otro organigrama.");

        var usuarioIds = input.Where(n => n.UsuarioId.HasValue).Select(n => n.UsuarioId!.Value).Distinct().ToList();
        var usuarios = (await _repository.GetUsuariosAsync(usuarioIds)).ToDictionary(u => u.Id);
        if (usuarios.Count != usuarioIds.Count)
            throw new InvalidOperationException("Alguno de los usuarios asignados ya no existe.");

        var nodos = input
            .Select((n, index) => (Dto: n, Index: index))
            .GroupBy(x => x.Dto.ParentId)
            .SelectMany(grupo => grupo
                .OrderBy(x => x.Dto.Orden)
                .ThenBy(x => x.Index)
                .Select((x, orden) => new OrganigramaNodo
                {
                    Id = x.Dto.Id,
                    OrganigramaId = organigramaId,
                    ParentId = x.Dto.ParentId,
                    Cargo = x.Dto.Cargo.Trim(),
                    Area = x.Dto.Area?.Trim() ?? string.Empty,
                    // Si hay un usuario vinculado, su nombre real prevalece sobre el texto libre.
                    NombreLibre = x.Dto.UsuarioId.HasValue ? string.Empty : x.Dto.NombreLibre?.Trim() ?? string.Empty,
                    UsuarioId = x.Dto.UsuarioId,
                    Notas = x.Dto.Notas?.Trim() ?? string.Empty,
                    Color = x.Dto.Color?.Trim() ?? string.Empty,
                    Orden = orden
                }))
            .ToList();

        return (nodos, usuarios);
    }

    private static OrganigramaDto ToDto(
        Organigrama organigrama,
        IEnumerable<OrganigramaNodo> nodos,
        IReadOnlyDictionary<Guid, ApplicationUser> usuarios)
    {
        return new OrganigramaDto
        {
            Id = organigrama.Id,
            Nombre = organigrama.Nombre,
            Descripcion = organigrama.Descripcion,
            FechaCreacion = organigrama.FechaCreacion,
            FechaActualizacion = organigrama.FechaActualizacion,
            Nodos = nodos
                .OrderBy(n => n.Orden)
                .Select(n => new OrganigramaNodoDto
                {
                    Id = n.Id,
                    ParentId = n.ParentId,
                    Cargo = n.Cargo,
                    Area = n.Area,
                    NombreLibre = n.NombreLibre,
                    UsuarioId = n.UsuarioId,
                    Usuario = n.UsuarioId is { } uid && usuarios.TryGetValue(uid, out var u) ? ToUsuarioDto(u) : null,
                    Notas = n.Notas,
                    Color = n.Color,
                    Orden = n.Orden
                })
                .ToList()
        };
    }

    private static OrganigramaUsuarioDto ToUsuarioDto(ApplicationUser u) => new()
    {
        Id = u.Id,
        NombreCompleto = $"{u.Nombres} {u.Apellidos}".Trim(),
        Iniciales = u.Iniciales,
        Correo = u.Email ?? string.Empty,
        Puesto = u.Puesto,
        Activo = u.Activo,
        AvatarUrl = u.AvatarUrl
    };
}
