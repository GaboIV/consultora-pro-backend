using ConsultoraPro.Application.DTOs.AmbienteCloudResources;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class AmbienteCloudResourceService : IAmbienteCloudResourceService
{
    private readonly IAmbienteCloudResourceRepository _repository;
    private readonly IAmbienteRepository _ambienteRepository;

    public AmbienteCloudResourceService(IAmbienteCloudResourceRepository repository, IAmbienteRepository ambienteRepository)
    {
        _repository = repository;
        _ambienteRepository = ambienteRepository;
    }

    public async Task<IEnumerable<AmbienteCloudResourceDto>> GetByAmbienteAsync(Guid ambienteId)
    {
        var items = await _repository.GetByAmbienteAsync(ambienteId);
        return items.Where(x => x.Activo).Select(ToDto);
    }

    public async Task<AmbienteCloudResourceDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null || !entity.Activo ? null : ToDto(entity);
    }

    public async Task<AmbienteCloudResourceDto> CreateAsync(CreateAmbienteCloudResourceDto dto)
    {
        await EnsureAmbienteExistsAsync(dto.AmbienteId);

        var entity = new AmbienteCloudResource
        {
            Id = Guid.NewGuid(),
            AmbienteId = dto.AmbienteId,
            TipoRecurso = dto.TipoRecurso.Trim(),
            NombreRecurso = dto.NombreRecurso.Trim(),
            DeepLink = dto.DeepLink?.Trim(),
            Plataforma = string.IsNullOrWhiteSpace(dto.Plataforma) ? "Azure" : dto.Plataforma.Trim(),
            Ubicacion = dto.Ubicacion?.Trim(),
            Nota = dto.Nota?.Trim(),
            Activo = true
        };

        var created = await _repository.CreateAsync(entity);
        return ToDto(created);
    }

    public async Task UpdateAsync(Guid id, UpdateAmbienteCloudResourceDto dto)
    {
        var entity = await GetActiveEntityAsync(id);

        entity.TipoRecurso = dto.TipoRecurso.Trim();
        entity.NombreRecurso = dto.NombreRecurso.Trim();
        entity.DeepLink = dto.DeepLink?.Trim();
        entity.Plataforma = string.IsNullOrWhiteSpace(dto.Plataforma) ? "Azure" : dto.Plataforma.Trim();
        entity.Ubicacion = dto.Ubicacion?.Trim();
        entity.Nota = dto.Nota?.Trim();

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetActiveEntityAsync(id);
        entity.Activo = false;
        await _repository.UpdateAsync(entity);
    }

    public async Task<ImportCloudResourcesCsvResponse> ImportFromCsvAsync(Guid ambienteId, ImportCloudResourcesCsvRequest request)
    {
        await EnsureAmbienteExistsAsync(ambienteId);

        var response = new ImportCloudResourcesCsvResponse();
        var plataforma = string.IsNullOrWhiteSpace(request.Plataforma) ? "Azure" : request.Plataforma.Trim();

        if (string.IsNullOrWhiteSpace(request.CsvContent))
        {
            response.Errors.Add("El contenido CSV está vacío.");
            return response;
        }

        var lines = request.CsvContent
            .Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length < 2)
        {
            response.Errors.Add("El CSV debe contener una fila de encabezado y al menos una fila de datos.");
            return response;
        }

        var delimiter = lines[0].Contains('\t') ? '\t' : ',';
        var headers = lines[0].Split(delimiter, StringSplitOptions.TrimEntries).Select(SanitizeCsvValue).ToArray();
        var expectedHeaders = new[] { "NAME", "TYPE", "LOCATION", "RESOURCE LINK" };

        for (int i = 0; i < expectedHeaders.Length; i++)
        {
            if (i >= headers.Length || !string.Equals(headers[i], expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
            {
                response.Errors.Add($"El encabezado de columna esperado '{expectedHeaders[i]}' no se encontró en la posición {i + 1}. Encabezados actuales: {string.Join(", ", headers)}");
                return response;
            }
        }

        var resources = new List<AmbienteCloudResource>();
        var existingNames = (await _repository.GetByAmbienteAsync(ambienteId))
            .Where(r => r.Activo)
            .Select(r => r.NombreRecurso.ToLowerInvariant())
            .ToHashSet();

        for (int i = 1; i < lines.Length; i++)
        {
            var columns = lines[i].Split(delimiter, StringSplitOptions.TrimEntries);
            var lineNumber = i + 1;

            if (columns.Length < 4)
            {
                response.Errors.Add($"Línea {lineNumber}: se esperaban 4 columnas (NAME, TYPE, LOCATION, RESOURCE LINK), pero se encontraron {columns.Length}.");
                response.SkippedCount++;
                continue;
            }

            var name = SanitizeCsvValue(columns[0]);
            var type = SanitizeCsvValue(columns[1]);
            var location = SanitizeCsvValue(columns[2]);
            var resourceLink = SanitizeCsvValue(columns[3]);

            if (string.IsNullOrWhiteSpace(name))
            {
                response.Errors.Add($"Línea {lineNumber}: el campo NAME está vacío.");
                response.SkippedCount++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                response.Errors.Add($"Línea {lineNumber}: el campo TYPE está vacío.");
                response.SkippedCount++;
                continue;
            }

            if (existingNames.Contains(name.ToLowerInvariant()))
            {
                response.Errors.Add($"Línea {lineNumber}: ya existe un recurso con el nombre '{name}'.");
                response.SkippedCount++;
                continue;
            }

            existingNames.Add(name.ToLowerInvariant());

            resources.Add(new AmbienteCloudResource
            {
                Id = Guid.NewGuid(),
                AmbienteId = ambienteId,
                TipoRecurso = type,
                NombreRecurso = name,
                DeepLink = resourceLink,
                Plataforma = plataforma,
                Ubicacion = location,
                Activo = true
            });
        }

        foreach (var resource in resources)
        {
            await _repository.CreateAsync(resource);
            response.ImportedCount++;
        }

        return response;
    }

    private async Task<AmbienteCloudResource> GetActiveEntityAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null || !entity.Activo)
            throw new KeyNotFoundException($"Recurso en la nube con ID {id} no encontrado");

        return entity;
    }

    private async Task EnsureAmbienteExistsAsync(Guid ambienteId)
    {
        var ambiente = await _ambienteRepository.GetByIdAsync(ambienteId);
        if (ambiente is null || !ambiente.Activo)
            throw new KeyNotFoundException($"Ambiente con ID {ambienteId} no encontrado");
    }

    private static AmbienteCloudResourceDto ToDto(AmbienteCloudResource entity)
    {
        return new AmbienteCloudResourceDto
        {
            Id = entity.Id,
            AmbienteId = entity.AmbienteId,
            TipoRecurso = entity.TipoRecurso,
            NombreRecurso = entity.NombreRecurso,
            DeepLink = entity.DeepLink,
            Plataforma = entity.Plataforma,
            Ubicacion = entity.Ubicacion,
            Nota = entity.Nota
        };
    }

    private static string SanitizeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var result = value.Trim();
        if (result.Length >= 2 && result[0] == '"' && result[^1] == '"')
            result = result[1..^1];
        return result.Trim();
    }
}
