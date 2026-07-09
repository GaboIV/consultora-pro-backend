using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ConsultoraPro.Infrastructure.Storage;

/// <summary>
/// Backend de almacenamiento para QA/Prod sobre Azure Blob Storage. El contenedor es privado:
/// las descargas se sirven mediante URLs SAS de solo lectura y corta expiración, firmadas con la
/// account key incluida en la connection string. El contenedor se provisiona por infraestructura.
/// </summary>
public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _container;
    private readonly TimeSpan _defaultExpiry;

    public AzureBlobStorageService(IOptions<StorageOptions> options)
    {
        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.AzureBlob.ConnectionString))
            throw new InvalidOperationException("Storage:AzureBlob:ConnectionString no está configurada.");
        if (string.IsNullOrWhiteSpace(o.AzureBlob.ContainerName))
            throw new InvalidOperationException("Storage:AzureBlob:ContainerName no está configurado.");

        _container = new BlobContainerClient(o.AzureBlob.ConnectionString, o.AzureBlob.ContainerName);
        _defaultExpiry = TimeSpan.FromMinutes(o.SignedUrlExpiryMinutes <= 0 ? 15 : o.SignedUrlExpiryMinutes);
    }

    public async Task<StoredFile> SaveFileAsync(Stream content, string fileName, string contentType, string category)
    {
        var key = $"{SanitizeCategory(category)}/{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var blob = _container.GetBlobClient(key);

        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        var size = content.CanSeek ? content.Length : 0L;
        return new StoredFile(key, contentType, size);
    }

    public Task<string> GetAccessUrlAsync(string key, TimeSpan? expiry = null)
    {
        var blob = _container.GetBlobClient(key);
        if (!blob.CanGenerateSasUri)
            throw new InvalidOperationException(
                "No se puede generar SAS: la connection string no incluye account key.");

        var builder = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = key,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? _defaultExpiry)
        };
        builder.SetPermissions(BlobSasPermissions.Read);

        return Task.FromResult(blob.GenerateSasUri(builder).ToString());
    }

    public async Task<Stream?> OpenReadAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var blob = _container.GetBlobClient(key);
        if (!await blob.ExistsAsync())
            return null;

        return await blob.OpenReadAsync();
    }

    public async Task DeleteFileAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        await _container.DeleteBlobIfExistsAsync(key);
    }

    public bool TryGetKeyFromUrl(string url, out string key)
    {
        key = string.Empty;
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Path: /{container}/{key}. Solo reconocemos URLs de NUESTRO contenedor.
        var prefix = $"/{_container.Name}/";
        var path = uri.AbsolutePath;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        key = Uri.UnescapeDataString(path[prefix.Length..].Trim('/'));
        return key.Length > 0;
    }

    private static string SanitizeCategory(string category) =>
        string.IsNullOrWhiteSpace(category)
            ? "misc"
            : category.Trim('/', '\\').Replace("..", string.Empty);
}
