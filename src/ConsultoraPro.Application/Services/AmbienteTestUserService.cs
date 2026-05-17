using ConsultoraPro.Application.DTOs.AmbienteTestUsers;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class AmbienteTestUserService : IAmbienteTestUserService
{
    private readonly IAmbienteTestUserRepository _repository;
    private readonly IAmbienteRepository _ambienteRepository;
    private readonly IEncryptionService _encryptionService;

    public AmbienteTestUserService(
        IAmbienteTestUserRepository repository,
        IAmbienteRepository ambienteRepository,
        IEncryptionService encryptionService)
    {
        _repository = repository;
        _ambienteRepository = ambienteRepository;
        _encryptionService = encryptionService;
    }

    public async Task<IEnumerable<AmbienteTestUserDto>> GetByAmbienteAsync(Guid ambienteId)
    {
        var items = await _repository.GetByAmbienteAsync(ambienteId);
        return items.Where(x => x.Activo).Select(ToDto);
    }

    public async Task<AmbienteTestUserDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null || !entity.Activo ? null : ToDto(entity);
    }

    public async Task<AmbienteTestUserDto> CreateAsync(CreateAmbienteTestUserDto dto)
    {
        await EnsureAmbienteExistsAsync(dto.AmbienteId);

        var entity = new AmbienteTestUser
        {
            Id = Guid.NewGuid(),
            AmbienteId = dto.AmbienteId,
            RolAplicacion = dto.RolAplicacion.Trim(),
            Correo = dto.Correo.Trim().ToLowerInvariant(),
            PasswordCifrado = _encryptionService.Encrypt(dto.Password),
            Notas = dto.Notas?.Trim(),
            Activo = true
        };

        var created = await _repository.CreateAsync(entity);
        return ToDto(created);
    }

    public async Task UpdateAsync(Guid id, UpdateAmbienteTestUserDto dto)
    {
        var entity = await GetActiveEntityAsync(id);

        entity.RolAplicacion = dto.RolAplicacion.Trim();
        entity.Correo = dto.Correo.Trim().ToLowerInvariant();
        entity.Notas = dto.Notas?.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            entity.PasswordCifrado = _encryptionService.Encrypt(dto.Password);
        }

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetActiveEntityAsync(id);
        entity.Activo = false;
        await _repository.UpdateAsync(entity);
    }

    private async Task<AmbienteTestUser> GetActiveEntityAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null || !entity.Activo)
            throw new KeyNotFoundException($"Usuario de prueba con ID {id} no encontrado");

        return entity;
    }

    private async Task EnsureAmbienteExistsAsync(Guid ambienteId)
    {
        var ambiente = await _ambienteRepository.GetByIdAsync(ambienteId);
        if (ambiente is null || !ambiente.Activo)
            throw new KeyNotFoundException($"Ambiente con ID {ambienteId} no encontrado");
    }

    private AmbienteTestUserDto ToDto(AmbienteTestUser entity)
    {
        return new AmbienteTestUserDto
        {
            Id = entity.Id,
            AmbienteId = entity.AmbienteId,
            RolAplicacion = entity.RolAplicacion,
            Correo = entity.Correo,
            PasswordCifrado = entity.PasswordCifrado,
            Notas = entity.Notas
        };
    }
}
