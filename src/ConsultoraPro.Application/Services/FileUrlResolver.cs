using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsultoraPro.Application.Interfaces;

namespace ConsultoraPro.Application.Services;

public partial class FileUrlResolver : IFileUrlResolver
{
    /// <summary>Esquema estable que se persiste embebido en descripciones para imágenes inline.</summary>
    public const string PlaceholderScheme = "cpfile://";

    private readonly IStorageService _storage;

    public FileUrlResolver(IStorageService storage) => _storage = storage;

    public async Task<string?> ResolveAsync(string? keyOrPlaceholder)
    {
        if (string.IsNullOrWhiteSpace(keyOrPlaceholder))
            return keyOrPlaceholder;

        var key = StripPlaceholder(keyOrPlaceholder);
        return await _storage.GetAccessUrlAsync(key);
    }

    public async Task<string?> ResolveContentAsync(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return content;

        // Convierte URLs legacy (ej. https://localhost:7001/uploads/...) a placeholders cpfile://
        // antes de resolver, para que datos guardados antes del refactor también funcionen.
        var normalized = ToStoragePlaceholders(content);
        if (!normalized!.Contains(PlaceholderScheme))
            return normalized;

        var matches = PlaceholderRegex().Matches(normalized);
        var result = normalized;
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var url = await _storage.GetAccessUrlAsync(key);
            result = result.Replace(match.Value, url);
        }
        return result;
    }

    public IReadOnlyList<StorageImageRef> ExtractStorageImages(string? content)
    {
        var normalized = ToStoragePlaceholders(content);
        if (string.IsNullOrWhiteSpace(normalized) || !normalized.Contains(PlaceholderScheme))
            return Array.Empty<StorageImageRef>();

        var refs = new List<StorageImageRef>();
        var seen = new HashSet<string>();
        foreach (Match match in PlaceholderRegex().Matches(normalized))
        {
            // match.Value = "cpfile://{key}" (texto a reemplazar); grupo 1 = key.
            if (seen.Add(match.Value))
                refs.Add(new StorageImageRef(match.Value, match.Groups[1].Value));
        }
        return refs;
    }

    public string? ToStoragePlaceholders(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return content;

        // URLs de imagen embebidas: markdown ![alt](url) o HTML src="url". Si apuntan a nuestro
        // storage se sustituyen por el placeholder estable; el resto se deja intacto.
        var result = MarkdownImageRegex().Replace(content, m => ReplaceIfOwned(m.Value, m.Groups[1].Value));
        result = HtmlSrcRegex().Replace(result, m => ReplaceIfOwned(m.Value, m.Groups[1].Value));
        return result;
    }

    private string ReplaceIfOwned(string original, string url)
    {
        if (url.StartsWith(PlaceholderScheme))
            return original; // ya es un placeholder
        // 1) URL del proveedor activo (Azure: /{container}/{key}; Local: /uploads/{key}).
        if (_storage.TryGetKeyFromUrl(url, out var key))
            return original.Replace(url, $"{PlaceholderScheme}{key}");
        // 2) URL legacy del patrón estático /uploads/{key} (datos guardados con LocalStorageService
        //    antes del refactor). Se reconoce sea cual sea el proveedor actual, porque la convención
        //    /uploads/ es estable e independiente del backend de almacenamiento.
        if (TryGetLegacyUploadsKey(url, out var legacyKey))
            return original.Replace(url, $"{PlaceholderScheme}{legacyKey}");
        return original;
    }

    /// <summary>Extrae la key de una URL legacy con el marcador estático /uploads/{key}.</summary>
    private static bool TryGetLegacyUploadsKey(string url, out string key)
    {
        key = string.Empty;
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

    private static string StripPlaceholder(string value) =>
        value.StartsWith(PlaceholderScheme) ? value[PlaceholderScheme.Length..] : value;

    // cpfile://{key} hasta el siguiente delimitador de atributo/markup.
    [GeneratedRegex(@"cpfile://([^\s""'<>)]+)")]
    private static partial Regex PlaceholderRegex();

    // ![alt](url)
    [GeneratedRegex(@"!\[[^\]]*\]\(([^)\s]+)\)")]
    private static partial Regex MarkdownImageRegex();

    // src="url" | src='url'
    [GeneratedRegex(@"src=[""']([^""']+)[""']")]
    private static partial Regex HtmlSrcRegex();
}
