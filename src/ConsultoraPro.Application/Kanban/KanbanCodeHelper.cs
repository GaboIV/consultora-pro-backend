using System.Globalization;
using System.Text;

namespace ConsultoraPro.Application.Kanban;

/// <summary>
/// Utilidades puras (sin dependencias de infraestructura) para la generación de claves
/// y códigos del módulo Kanban. Son funciones determinísticas y fáciles de testear.
/// </summary>
public static class KanbanCodeHelper
{
    /// <summary>
    /// Deriva una clave por defecto a partir de un nombre: quita tildes/espacios/símbolos,
    /// conserva letras y dígitos, pasa a MAYÚSCULAS y recorta a <paramref name="longitud"/>.
    /// "Repsol" → "REP"; "QA / Bugs" → "QAB"; "Tareas" → "TAR".
    /// </summary>
    public static string DeriveKey(string? nombre, int longitud)
    {
        if (longitud <= 0)
            longitud = 3;

        var normalized = RemoveDiacritics(nombre ?? string.Empty);
        var sb = new StringBuilder(longitud);

        foreach (var c in normalized)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToUpperInvariant(c));
                if (sb.Length >= longitud)
                    break;
            }
        }

        // Fallback si el nombre no aportó caracteres válidos (p. ej. solo símbolos).
        return sb.Length == 0 ? "KEY".Substring(0, Math.Min(3, longitud)) : sb.ToString();
    }

    /// <summary>
    /// Formatea el código legible de una tarjeta.
    /// Con proyecto: "REP-TAR-001". Sin proyecto (tablero personal): "TAR-001".
    /// El número crece de forma natural si supera 999 (p. ej. "TAR-1000").
    /// </summary>
    public static string FormatCodigo(string? proyectoClave, string tableroClave, int numero)
    {
        return string.IsNullOrEmpty(proyectoClave)
            ? $"{tableroClave}-{numero:D3}"
            : $"{proyectoClave}-{tableroClave}-{numero:D3}";
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
