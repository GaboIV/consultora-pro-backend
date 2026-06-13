using System;
using System.IO;
using System.Threading.Tasks;
using ConsultoraPro.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ConsultoraPro.Infrastructure.Storage;

public class LocalStorageService : IStorageService
{
    private readonly IConfiguration _configuration;
    private readonly string _uploadsFolder;
    private readonly string _baseUrl;

    public LocalStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrl = _configuration["Storage:BaseUrl"] ?? "https://localhost:7001";
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (!Directory.Exists(_uploadsFolder))
        {
            Directory.CreateDirectory(_uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(_uploadsFolder, uniqueFileName);

        using (var ws = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(ws);
        }

        return $"{_baseUrl.TrimEnd('/')}/uploads/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return Task.CompletedTask;

        try
        {
            var uri = new Uri(fileUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            var filePath = Path.Combine(_uploadsFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Fail silently if URL is invalid or file cannot be deleted
        }

        return Task.CompletedTask;
    }
}
