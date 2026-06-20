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
        if (string.IsNullOrWhiteSpace(content) || !content.Contains(PlaceholderScheme))
            return content;

        var matches = PlaceholderRegex().Matches(content);
        var result = content;
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var url = await _storage.GetAccessUrlAsync(key);
            result = result.Replace(match.Value, url);
        }
        return result;
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
        if (_storage.TryGetKeyFromUrl(url, out var key))
            return original.Replace(url, $"{PlaceholderScheme}{key}");
        return original;
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
