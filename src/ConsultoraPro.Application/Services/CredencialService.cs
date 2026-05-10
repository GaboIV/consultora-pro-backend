using ConsultoraPro.Application.DTOs.Credenciales;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class CredencialService : ICredencialService
{
    private readonly ICredencialRepository _repository;
    private readonly IProyectoRepository _proyectoRepository;
    private readonly IAmbienteRepository _ambienteRepository;
    private readonly IEncryptionService _encryptionService;

    public CredencialService(
        ICredencialRepository repository,
        IProyectoRepository proyectoRepository,
        IAmbienteRepository ambienteRepository,
        IEncryptionService encryptionService)
    {
        _repository = repository;
        _proyectoRepository = proyectoRepository;
        _ambienteRepository = ambienteRepository;
        _encryptionService = encryptionService;
    }

    public async Task<IEnumerable<CredencialListDto>> GetAllAsync(Guid? proyectoId = null)
    {
        var credenciales = await _repository.GetAllAsync(proyectoId);
        return credenciales.Select(ToListDto);
    }

    public async Task<CredencialDetalleDto?> GetByIdAsync(Guid id)
    {
        var credencial = await _repository.GetByIdAsync(id);
        return credencial is null || !credencial.Activo ? null : ToDetalleDto(credencial);
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
            ProyectoId = dto.ProyectoId,
            AmbienteId = dto.AmbienteId,
            ValorCifrado = _encryptionService.Encrypt(dto.Valor),
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

    public async Task<CredencialRevealDto> RevealAsync(Guid id, Guid userId, string ip, string userAgent)
    {
        var credencial = await GetActiveEntityAsync(id);
        var revealedAt = DateTime.UtcNow;

        await _repository.AddAuditAsync(new AuditoriaCredencial
        {
            Id = Guid.NewGuid(),
            CredencialId = credencial.Id,
            UsuarioId = userId,
            FechaRevelacion = revealedAt,
            Ip = ip,
            UserAgent = userAgent
        });

        return new CredencialRevealDto
        {
            Id = credencial.Id,
            Nombre = credencial.Nombre,
            Valor = _encryptionService.Decrypt(credencial.ValorCifrado),
            ReveladoEn = revealedAt
        };
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
            ProyectoId = credencial.ProyectoId,
            ProyectoNombre = credencial.Proyecto?.Nombre ?? string.Empty,
            AmbienteId = credencial.AmbienteId,
            AmbienteNombre = credencial.Ambiente?.Nombre,
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
        dto.ProyectoId = list.ProyectoId;
        dto.ProyectoNombre = list.ProyectoNombre;
        dto.AmbienteId = list.AmbienteId;
        dto.AmbienteNombre = list.AmbienteNombre;
        dto.FechaVencimiento = list.FechaVencimiento;
        dto.DiasParaVencer = list.DiasParaVencer;
        dto.EstadoVencimiento = list.EstadoVencimiento;
        dto.Activo = list.Activo;
        dto.FechaCreacion = list.FechaCreacion;
        return dto;
    }

    private static string MapExpirationState(int days) => days switch
    {
        < 0 => "Vencida",
        < 7 => "Critica",
        <= 30 => "PorVencer",
        _ => "Vigente"
    };
}
