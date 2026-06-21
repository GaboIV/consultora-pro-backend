namespace ConsultoraPro.API.Authorization;

/// <summary>Nombres de esquemas de autenticación usados en el flujo OAuth de Google.</summary>
public static class AuthSchemes
{
    /// <summary>
    /// Cookie temporal donde el handler de Google deposita los claims externos durante el
    /// handshake OAuth. El controller la lee en /complete, emite el JWT propio y la limpia.
    /// </summary>
    public const string External = "External";
}
