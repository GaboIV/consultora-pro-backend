using System.Text.Json;
using ConsultoraPro.Application.DTOs.Credenciales;
using ConsultoraPro.Application.DTOs.Common;
using ConsultoraPro.Application.DTOs.Notificaciones;
using ConsultoraPro.Application.Exceptions;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using FluentValidation;

namespace ConsultoraPro.Application.Services;

public class CredencialService : ICredencialService
{
    /// <summary>Ventana de vigencia de una revelación aprobada para un usuario de nivel básico.</summary>
    private static readonly TimeSpan RevelacionTemporalTtl = TimeSpan.FromMinutes(15);

    private readonly ICredencialRepository _repository;
    private readonly ISolicitudRevelacionRepository _solicitudRepository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IAmbienteRepository _ambienteRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IValidator<CreateCredencialDto> _createValidator;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectScope _projectScope;
    private readonly INotificacionService _notificacionService;
    private readonly INotificacionRepository _notificacionRepository;

    public CredencialService(
        ICredencialRepository repository,
        ISolicitudRevelacionRepository solicitudRepository,
        IProyectoRepository proyectoRepository,
        IAmbienteRepository ambienteRepository,
        IEncryptionService encryptionService,
        IValidator<CreateCredencialDto> createValidator,
        ICurrentUserService currentUser,
        IProjectScope projectScope,
        INotificacionService notificacionService,
        INotificacionRepository notificacionRepository)
    {
        _repository = repository;
        _solicitudRepository = solicitudRepository;
        _proyectoRepository = proyectoRepository;
        _ambienteRepository = ambienteRepository;
        _encryptionService = encryptionService;
        _createValidator = createValidator;
        _currentUser = currentUser;
        _projectScope = projectScope;
        _notificacionService = notificacionService;
        _notificacionRepository = notificacionRepository;
    }

    public async Task<PagedResultDto<CredencialListDto>> GetAllAsync(int page = 1, int pageSize = 20, Guid? proyectoId = null)
    {
        var items = await _repository.GetPagedAsync(page, pageSize, proyectoId);
        var total = await _repository.GetTotalCountAsync(proyectoId);

        IEnumerable<Credencial> filtered = items;
        if (!_projectScope.VeTodos("proyectos"))
        {
            var proyectosAsignados = await _projectScope.ProyectosAsignadosAsync();
            filtered = items.Where(c => proyectosAsignados.Contains(c.ProyectoId));
        }

        return new PagedResultDto<CredencialListDto>
        {
            Data = filtered.Select(ToListDto).ToList(),
            TotalCount = _projectScope.VeTodos("proyectos") ? total : filtered.Count(),
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CredencialDetalleDto?> GetByIdAsync(Guid id)
    {
        var credencial = await _repository.GetByIdAsync(id);
        if (credencial is null || !credencial.Activo) return null;

        if (!_projectScope.VeTodos("proyectos"))
        {
            var proyectosAsignados = await _projectScope.ProyectosAsignadosAsync();
            if (!proyectosAsignados.Contains(credencial.ProyectoId))
                return null;
        }

        return ToDetalleDto(credencial);
    }

    public async Task<CredencialListDto> CreateAsync(CreateCredencialDto dto, Guid userId)
    {
        await EnsureProjectExistsAsync(dto.ProyectoId);
        await EnsureEnvironmentBelongsToProjectAsync(dto.AmbienteId, dto.ProyectoId);

        var credencial = new Credencial
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Tipo = dto.Tipo,
            Servidor = dto.Servidor.Trim(),
            Host = Normalize(dto.Host),
            Puerto = dto.Puerto,
            Usuario = Normalize(dto.Usuario),
            Url = Normalize(dto.Url),
            Notas = Normalize(dto.Notas),
            CamposExtra = SerializeCamposExtra(dto.CamposExtra),
            ProyectoId = dto.ProyectoId,
            AmbienteId = dto.AmbienteId,
            ValorCifrado = _encryptionService.Encrypt(dto.Valor),
            SecretosExtraCifrado = EncryptSecretosExtra(dto.SecretosExtra),
            FechaVencimiento = DateTime.SpecifyKind(dto.FechaVencimiento, DateTimeKind.Utc),
            CreadoPor = userId,
            FechaCreacion = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Activo = true
        };

        var created = await _repository.CreateAsync(credencial);
        var reloaded = await _repository.GetByIdAsync(created.Id);
        return ToListDto(reloaded ?? created);
    }

    public async Task UpdateAsync(Guid id, UpdateCredencialDto dto)
    {
        var credencial = await GetActiveEntityAsync(id);
        await EnsureProjectExistsAsync(dto.ProyectoId);
        await EnsureEnvironmentBelongsToProjectAsync(dto.AmbienteId, dto.ProyectoId);

        credencial.Nombre = dto.Nombre.Trim();
        credencial.Tipo = dto.Tipo;
        credencial.Servidor = dto.Servidor.Trim();
        credencial.Host = Normalize(dto.Host);
        credencial.Puerto = dto.Puerto;
        credencial.Usuario = Normalize(dto.Usuario);
        credencial.Url = Normalize(dto.Url);
        credencial.Notas = Normalize(dto.Notas);
        credencial.CamposExtra = SerializeCamposExtra(dto.CamposExtra);
        credencial.ProyectoId = dto.ProyectoId;
        credencial.AmbienteId = dto.AmbienteId;
        credencial.FechaVencimiento = DateTime.SpecifyKind(dto.FechaVencimiento, DateTimeKind.Utc);
        credencial.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(credencial);
    }

    public async Task UpdateValorAsync(Guid id, UpdateCredencialValorDto dto)
    {
        var credencial = await GetActiveEntityAsync(id);
        credencial.ValorCifrado = _encryptionService.Encrypt(dto.Valor);
        if (dto.SecretosExtra is not null)
            credencial.SecretosExtraCifrado = EncryptSecretosExtra(dto.SecretosExtra);
        credencial.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(credencial);
    }

    public async Task DeleteAsync(Guid id)
    {
        var credencial = await GetActiveEntityAsync(id);
        credencial.Activo = false;
        credencial.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(credencial);
    }

    public async Task<CredencialRevealDto> RevealAsync(Guid id, Guid userId, bool puedeRevelarDirecto, string ip, string userAgent)
    {
        var credencial = await GetActiveEntityAsync(id);
        var revealedAt = DateTime.UtcNow;

        // El nivel básico (sin permiso directo de revelar) solo puede ver el secreto si tiene una
        // aprobación vigente. La decisión se toma SIEMPRE en servidor: el claim del JWT no basta.
        SolicitudRevelacionCredencial? aprobacion = null;
        if (!puedeRevelarDirecto)
        {
            aprobacion = await _solicitudRepository.GetVigenteAsync(id, userId, revealedAt);
            if (aprobacion is null)
                throw new RevelacionRequiereSolicitudException();
        }

        await _repository.AddAuditAsync(new AuditoriaCredencial
        {
            Id = Guid.NewGuid(),
            CredencialId = credencial.Id,
            UsuarioId = userId,
            Accion = "Lectura",
            Detalle = aprobacion is null ? null : $"Revelación autorizada por solicitud {aprobacion.Id}",
            FechaRevelacion = revealedAt,
            Ip = ip,
            UserAgent = userAgent
        });

        return new CredencialRevealDto
        {
            Id = credencial.Id,
            Nombre = credencial.Nombre,
            Valor = _encryptionService.Decrypt(credencial.ValorCifrado),
            SecretosExtra = DecryptSecretosExtra(credencial.SecretosExtraCifrado),
            ReveladoEn = revealedAt
        };
    }

    public async Task<SolicitudRevelacionDto> CrearSolicitudAsync(Guid credencialId, Guid solicitanteId, string? motivo)
    {
        var credencial = await GetActiveEntityAsync(credencialId);

        if (!_projectScope.VeTodos("proyectos"))
        {
            var proyectosAsignados = await _projectScope.ProyectosAsignadosAsync();
            if (!proyectosAsignados.Contains(credencial.ProyectoId))
                throw new KeyNotFoundException($"Credencial con ID {credencialId} no encontrada");
        }

        var now = DateTime.UtcNow;

        // Si ya hay una aprobación vigente o una solicitud pendiente, se devuelve esa (idempotente).
        var vigente = await _solicitudRepository.GetVigenteAsync(credencialId, solicitanteId, now);
        if (vigente is not null)
        {
            vigente.Credencial = credencial;
            return ToSolicitudDto(vigente);
        }

        var pendiente = await _solicitudRepository.GetPendienteAsync(credencialId, solicitanteId);
        if (pendiente is not null)
        {
            pendiente.Credencial = credencial;
            return ToSolicitudDto(pendiente);
        }

        var solicitud = new SolicitudRevelacionCredencial
        {
            Id = Guid.NewGuid(),
            CredencialId = credencialId,
            Credencial = credencial,
            SolicitanteId = solicitanteId,
            Estado = EstadoSolicitudRevelacion.Pendiente,
            Motivo = Truncate(Normalize(motivo), 500),
            FechaSolicitud = now
        };

        await _solicitudRepository.CreateAsync(solicitud);

        // Avisar a quienes pueden aprobar (permiso credenciales.solicitud.aprobar).
        var aprobadores = await _notificacionRepository.GetUsuarioIdsConPermisoAsync("credenciales.solicitud.aprobar");
        await _notificacionService.PublicarAsync(new PublicarNotificacionDto
        {
            Tipo = TipoNotificacion.CredencialSolicitud,
            DestinatarioIds = aprobadores.ToList(),
            ActorId = solicitanteId,
            Titulo = "Solicitud de revelación de credencial",
            Mensaje = $"{{actor}} solicitó revelar la credencial «{credencial.Nombre}»." +
                      (string.IsNullOrWhiteSpace(solicitud.Motivo) ? string.Empty : $" Motivo: {solicitud.Motivo}"),
            Url = "/credenciales"
        });

        return ToSolicitudDto(solicitud);
    }

    public async Task<IEnumerable<SolicitudRevelacionDto>> GetSolicitudesAsync(EstadoSolicitudRevelacion? estado)
    {
        var solicitudes = await _solicitudRepository.ListAsync(estado);
        return solicitudes.Select(ToSolicitudDto).ToList();
    }

    public async Task<IEnumerable<SolicitudRevelacionDto>> GetMisSolicitudesAsync(Guid solicitanteId)
    {
        var solicitudes = await _solicitudRepository.ListBySolicitanteAsync(solicitanteId);
        return solicitudes.Select(ToSolicitudDto).ToList();
    }

    public async Task<SolicitudRevelacionDto> ResolverSolicitudAsync(Guid solicitudId, Guid aprobadorId, bool aprobar, string? nota)
    {
        var solicitud = await _solicitudRepository.GetByIdAsync(solicitudId);
        if (solicitud is null)
            throw new KeyNotFoundException($"Solicitud con ID {solicitudId} no encontrada");

        if (solicitud.Estado != EstadoSolicitudRevelacion.Pendiente)
            throw new InvalidOperationException("La solicitud ya fue resuelta.");

        var now = DateTime.UtcNow;
        solicitud.AprobadorId = aprobadorId;
        solicitud.Estado = aprobar ? EstadoSolicitudRevelacion.Aprobada : EstadoSolicitudRevelacion.Rechazada;
        solicitud.NotaResolucion = Truncate(Normalize(nota), 500);
        solicitud.FechaResolucion = now;
        solicitud.VigenteHasta = aprobar ? now.Add(RevelacionTemporalTtl) : null;

        await _solicitudRepository.UpdateAsync(solicitud);

        await _notificacionService.PublicarAsync(new PublicarNotificacionDto
        {
            Tipo = TipoNotificacion.CredencialSolicitudResuelta,
            DestinatarioIds = new List<Guid> { solicitud.SolicitanteId },
            ActorId = aprobadorId,
            Titulo = aprobar ? "Solicitud de credencial aprobada" : "Solicitud de credencial rechazada",
            Mensaje = aprobar
                ? $"{{actor}} aprobó tu solicitud sobre «{solicitud.Credencial?.Nombre}». " +
                  $"Tienes {(int)RevelacionTemporalTtl.TotalMinutes} minutos para revelarla."
                : $"{{actor}} rechazó tu solicitud sobre «{solicitud.Credencial?.Nombre}»." +
                  (string.IsNullOrWhiteSpace(solicitud.NotaResolucion) ? string.Empty : $" Nota: {solicitud.NotaResolucion}"),
            Url = "/credenciales"
        });

        return ToSolicitudDto(solicitud);
    }

    private static SolicitudRevelacionDto ToSolicitudDto(SolicitudRevelacionCredencial s) => new()
    {
        Id = s.Id,
        CredencialId = s.CredencialId,
        CredencialNombre = s.Credencial?.Nombre ?? string.Empty,
        ProyectoId = s.Credencial?.ProyectoId ?? Guid.Empty,
        ProyectoNombre = s.Credencial?.Proyecto?.Nombre ?? string.Empty,
        SolicitanteId = s.SolicitanteId,
        SolicitanteNombre = FullName(s.Solicitante),
        AprobadorId = s.AprobadorId,
        AprobadorNombre = s.Aprobador is null ? null : FullName(s.Aprobador),
        Estado = s.Estado,
        Motivo = s.Motivo,
        NotaResolucion = s.NotaResolucion,
        FechaSolicitud = s.FechaSolicitud,
        FechaResolucion = s.FechaResolucion,
        VigenteHasta = s.VigenteHasta
    };

    private static string FullName(ApplicationUser? user) =>
        user is null ? string.Empty : $"{user.Nombres} {user.Apellidos}".Trim();

    public async Task RegistrarCopiadoAsync(Guid id, Guid userId, string ip, string userAgent, string? campo)
    {
        var credencial = await GetActiveEntityAsync(id);

        await _repository.AddAuditAsync(new AuditoriaCredencial
        {
            Id = Guid.NewGuid(),
            CredencialId = credencial.Id,
            UsuarioId = userId,
            Accion = "Copiado",
            Detalle = Truncate(Normalize(campo), 120),
            FechaRevelacion = DateTime.UtcNow,
            Ip = ip,
            UserAgent = userAgent
        });
    }

    public async Task<ImportResultDto> ImportAsync(ImportCredencialesDto dto, Guid userId)
    {
        var result = new ImportResultDto { Total = dto.Filas.Count };

        foreach (var fila in dto.Filas)
        {
            var validation = await _createValidator.ValidateAsync(fila);
            if (!validation.IsValid)
            {
                result.Errores.Add(new ImportRowErrorDto
                {
                    Fila = fila.Fila,
                    Nombre = fila.Nombre,
                    Error = string.Join(" · ", validation.Errors.Select(e => e.ErrorMessage).Distinct())
                });
                continue;
            }

            try
            {
                await CreateAsync(fila, userId);
                result.Importadas++;
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
            {
                result.Errores.Add(new ImportRowErrorDto
                {
                    Fila = fila.Fila,
                    Nombre = fila.Nombre,
                    Error = ex.Message
                });
            }
        }

        return result;
    }

    public async Task<IEnumerable<AuditoriaCredencialDto>> GetAuditAsync(Guid credencialId)
    {
        var credencial = await _repository.GetByIdAsync(credencialId);
        if (credencial is null)
            throw new KeyNotFoundException($"Credencial con ID {credencialId} no encontrada");

        var auditorias = await _repository.GetAuditAsync(credencialId);
        return auditorias.Select(a => new AuditoriaCredencialDto
        {
            Id = a.Id,
            CredencialId = a.CredencialId,
            UsuarioId = a.UsuarioId,
            UsuarioNombre = a.Usuario is null ? "Usuario no disponible" : $"{a.Usuario.Nombres} {a.Usuario.Apellidos}".Trim(),
            Accion = a.Accion,
            Detalle = a.Detalle,
            FechaRevelacion = a.FechaRevelacion,
            Ip = a.Ip,
            UserAgent = a.UserAgent
        });
    }

    private async Task<Credencial> GetActiveEntityAsync(Guid id)
    {
        var credencial = await _repository.GetByIdAsync(id);
        if (credencial is null || !credencial.Activo)
            throw new KeyNotFoundException($"Credencial con ID {id} no encontrada");

        return credencial;
    }

    private async Task EnsureProjectExistsAsync(Guid proyectoId)
    {
        var proyecto = await _proyectoRepository.GetByIdAsync(proyectoId);
        if (proyecto is null)
            throw new KeyNotFoundException($"Proyecto con ID {proyectoId} no encontrado");
    }

    private async Task EnsureEnvironmentBelongsToProjectAsync(Guid? ambienteId, Guid proyectoId)
    {
        if (!ambienteId.HasValue)
            return;

        var ambiente = await _ambienteRepository.GetByIdAsync(ambienteId.Value);
        if (ambiente is null || !ambiente.Activo)
            throw new KeyNotFoundException($"Ambiente con ID {ambienteId.Value} no encontrado");

        if (ambiente.ProyectoId != proyectoId)
            throw new InvalidOperationException("El ambiente seleccionado no pertenece al proyecto indicado");
    }

    private static CredencialListDto ToListDto(Credencial credencial)
    {
        var dias = (int)Math.Ceiling((credencial.FechaVencimiento.Date - DateTime.UtcNow.Date).TotalDays);
        return new CredencialListDto
        {
            Id = credencial.Id,
            Nombre = credencial.Nombre,
            Tipo = credencial.Tipo,
            Servidor = credencial.Servidor,
            Host = credencial.Host,
            Puerto = credencial.Puerto,
            Usuario = credencial.Usuario,
            Url = credencial.Url,
            Notas = credencial.Notas,
            CamposExtra = DeserializeCamposExtra(credencial.CamposExtra),
            ProyectoId = credencial.ProyectoId,
            ProyectoNombre = credencial.Proyecto?.Nombre ?? string.Empty,
            AmbienteId = credencial.AmbienteId,
            AmbienteNombre = credencial.Ambiente?.Nombre,
            AmbienteTipo = credencial.Ambiente?.Tipo.ToString(),
            FechaVencimiento = credencial.FechaVencimiento,
            DiasParaVencer = dias,
            EstadoVencimiento = MapExpirationState(dias),
            Activo = credencial.Activo,
            FechaCreacion = credencial.FechaCreacion
        };
    }

    private static CredencialDetalleDto ToDetalleDto(Credencial credencial)
    {
        var dto = new CredencialDetalleDto
        {
            CreadoPor = credencial.CreadoPor,
            CreadoPorNombre = credencial.Creador is null ? string.Empty : $"{credencial.Creador.Nombres} {credencial.Creador.Apellidos}".Trim()
        };

        var list = ToListDto(credencial);
        dto.Id = list.Id;
        dto.Nombre = list.Nombre;
        dto.Tipo = list.Tipo;
        dto.Servidor = list.Servidor;
        dto.Host = list.Host;
        dto.Puerto = list.Puerto;
        dto.Usuario = list.Usuario;
        dto.Url = list.Url;
        dto.Notas = list.Notas;
        dto.CamposExtra = list.CamposExtra;
        dto.ProyectoId = list.ProyectoId;
        dto.ProyectoNombre = list.ProyectoNombre;
        dto.AmbienteId = list.AmbienteId;
        dto.AmbienteNombre = list.AmbienteNombre;
        dto.AmbienteTipo = list.AmbienteTipo;
        dto.FechaVencimiento = list.FechaVencimiento;
        dto.DiasParaVencer = list.DiasParaVencer;
        dto.EstadoVencimiento = list.EstadoVencimiento;
        dto.Activo = list.Activo;
        dto.FechaCreacion = list.FechaCreacion;
        return dto;
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is { Length: > 0 } && value.Length > maxLength ? value[..maxLength] : value;

    private static string? SerializeCamposExtra(Dictionary<string, string>? campos)
    {
        var limpio = campos?
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
            .ToDictionary(kv => kv.Key.Trim(), kv => kv.Value.Trim());

        return limpio is { Count: > 0 } ? JsonSerializer.Serialize(limpio) : null;
    }

    private static Dictionary<string, string>? DeserializeCamposExtra(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private string? EncryptSecretosExtra(Dictionary<string, string>? secretos)
    {
        var limpio = secretos?
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrEmpty(kv.Value))
            .ToDictionary(kv => kv.Key.Trim(), kv => kv.Value);

        return limpio is { Count: > 0 } ? _encryptionService.Encrypt(JsonSerializer.Serialize(limpio)) : null;
    }

    private Dictionary<string, string>? DecryptSecretosExtra(string? cifrado)
    {
        if (string.IsNullOrWhiteSpace(cifrado))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(_encryptionService.Decrypt(cifrado));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string MapExpirationState(int days) => days switch
    {
        < 0 => "Vencida",
        < 7 => "Critica",
        <= 30 => "PorVencer",
        _ => "Vigente"
    };
}
