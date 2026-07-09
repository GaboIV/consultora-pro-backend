using System;
using System.IO;
using System.Threading.Tasks;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.Infrastructure.Storage;

/// <summary>
/// Backend de almacenamiento para desarrollo: persiste en el filesystem bajo una raíz configurable
/// (p.ej. D:\ConsultoraPro\uploads) y sirve los archivos como estáticos en /uploads. En dev no se
/// firman las URLs (acceso directo); la firma SAS aplica solo a Azure.
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _rootPath;
    private readonly string _baseUrl;

    public LocalStorageService(IOptions<StorageOptions> options)
    {
        var local = options.Value.Local;
        _rootPath = string.IsNullOrWhiteSpace(local.RootPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "uploads")
            : local.RootPath;
        _baseUrl = local.PublicBaseUrl.TrimEnd('/');
    }

    public async Task<StoredFile> SaveFileAsync(Stream content, string fileName, string contentType, string category)
    {
        var safeCategory = SanitizeCategory(category);
        var folder = Path.Combine(_rootPath, safeCategory);
        Directory.CreateDirectory(folder);

        var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(folder, uniqueName);

        await using (var ws = new FileStream(filePath, FileMode.Create))
        {
            await content.CopyToAsync(ws);
        }

        var key = $"{safeCategory}/{uniqueName}";
        return new StoredFile(key, contentType, new FileInfo(filePath).Length);
    }

    public Task<string> GetAccessUrlAsync(string key, TimeSpan? expiry = null)
    {
        // En local no se firma: la URL pública estática es suficiente para desarrollo.
        var url = $"{_baseUrl}/uploads/{key.TrimStart('/')}";
        return Task.FromResult(url);
    }

    public Task<Stream?> OpenReadAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult<Stream?>(null);

        var filePath = Path.Combine(_rootPath, key.Replace('/', Path.DirectorySeparatorChar));
        // Defensa contra path traversal: el resultado debe quedar dentro de la raíz.
        var fullRoot = Path.GetFullPath(_rootPath);
        var fullTarget = Path.GetFullPath(filePath);
        if (!fullTarget.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullTarget))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(new FileStream(fullTarget, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public Task DeleteFileAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.CompletedTask;

        try
        {
            var filePath = Path.Combine(_rootPath, key.Replace('/', Path.DirectorySeparatorChar));
            // Defensa contra path traversal: el resultado debe quedar dentro de la raíz.
            var fullRoot = Path.GetFullPath(_rootPath);
            var fullTarget = Path.GetFullPath(filePath);
            if (fullTarget.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullTarget))
                File.Delete(fullTarget);
        }
        catch
        {
            // No fallar si la key es inválida o el archivo no puede eliminarse.
        }

        return Task.CompletedTask;
    }

    public bool TryGetKeyFromUrl(string url, out string key)
    {
        key = string.Empty;
        if (string.IsNullOrWhiteSpace(url))
            return false;

        const string marker = "/uploads/";
        var idx = url.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return false;

        var rest = url[(idx + marker.Length)..];
        var queryAt = rest.IndexOf('?');
        if (queryAt >= 0)
            rest = rest[..queryAt];

        key = Uri.UnescapeDataString(rest.Trim('/'));
        return key.Length > 0;
    }

    private static string SanitizeCategory(string category) =>
        string.IsNullOrWhiteSpace(category)
            ? "misc"
            : category.Trim('/', '\\').Replace("..", string.Empty);
}
