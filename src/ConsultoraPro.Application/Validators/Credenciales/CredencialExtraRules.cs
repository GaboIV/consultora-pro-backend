using System.Text.Json;

namespace ConsultoraPro.Application.Validators.Credenciales;

/// <summary>
/// Límites compartidos para los diccionarios dinámicos de credenciales.
/// Los serializados deben caber en las columnas CamposExtra (4000) y
/// SecretosExtraCifrado (7000, que tras AES-GCM + base64 admite ~5000 en claro).
/// </summary>
public static class CredencialExtraRules
{
    public const int MaxCamposExtraJson = 4000;
    public const int MaxSecretosExtraJson = 5000;

    public static bool FitsAsJson(Dictionary<string, string>? dict, int maxLength)
    {
        if (dict is null || dict.Count == 0)
            return true;

        return JsonSerializer.Serialize(dict).Length <= maxLength;
    }
}
