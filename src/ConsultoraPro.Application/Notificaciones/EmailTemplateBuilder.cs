using System.Net;
using System.Text;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Domain.Notificaciones;

namespace ConsultoraPro.Application.Notificaciones;

/// <summary>
/// Renderiza los correos con la identidad visual de la app (tema oscuro, acento azul).
/// Todo va con estilos inline y tablas para máxima compatibilidad (Gmail, Outlook).
/// </summary>
public static class EmailTemplateBuilder
{
    private const string Bg = "#0a0b0f";
    private const string Card = "#14161d";
    private const string Border = "#262b36";
    private const string Text = "#e8eaf0";
    private const string Text2 = "#9ba3b8";
    private const string Text3 = "#626a7e";
    private const string Accent = "#4f8ef7";

    public static EmailMessage BuildIndividual(CorreoPendiente correo, ApplicationUser usuario, string frontendBaseUrl)
    {
        var def = NotificacionCatalog.Get(correo.Tipo);
        var link = BuildLink(frontendBaseUrl, correo.Url);
        var body = BuildShell(
            preheader: correo.Mensaje,
            contenido: $"""
                <p style="margin:0 0 6px;font-size:12px;letter-spacing:.08em;text-transform:uppercase;color:{Accent};font-weight:700;">{Html(def.Grupo)}</p>
                <h1 style="margin:0 0 14px;font-size:20px;line-height:1.35;color:{Text};font-weight:700;">{Html(correo.Titulo)}</h1>
                <p style="margin:0 0 24px;font-size:14px;line-height:1.7;color:{Text2};">{Html(correo.Mensaje)}</p>
                {(link is null ? string.Empty : Button(link, "Ver en ConsultoraPro"))}
                """,
            frontendBaseUrl: frontendBaseUrl);

        var text = $"{correo.Titulo}\n\n{correo.Mensaje}" + (link is null ? string.Empty : $"\n\n{link}");
        return new EmailMessage(usuario.Email!, $"{usuario.Nombres} {usuario.Apellidos}".Trim(), correo.Titulo, body, text);
    }

    public static EmailMessage BuildResumen(IReadOnlyList<CorreoPendiente> correos, ApplicationUser usuario, string frontendBaseUrl)
    {
        var items = new StringBuilder();
        foreach (var correo in correos.OrderBy(c => c.FechaCreacion))
        {
            var link = BuildLink(frontendBaseUrl, correo.Url);
            var titulo = link is null
                ? $"<span style=\"color:{Text};font-weight:600;\">{Html(correo.Titulo)}</span>"
                : $"<a href=\"{link}\" style=\"color:{Text};font-weight:600;text-decoration:none;\">{Html(correo.Titulo)}</a>";
            items.Append($"""
                <tr>
                  <td style="padding:14px 0;border-bottom:1px solid {Border};">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr>
                      <td width="10" valign="top" style="padding-top:7px;">
                        <div style="width:6px;height:6px;border-radius:50%;background:{Accent};"></div>
                      </td>
                      <td style="padding-left:10px;">
                        <p style="margin:0 0 3px;font-size:14px;line-height:1.5;">{titulo}</p>
                        <p style="margin:0;font-size:13px;line-height:1.6;color:{Text2};">{Html(correo.Mensaje)}</p>
                      </td>
                    </tr></table>
                  </td>
                </tr>
                """);
        }

        var asunto = $"Resumen de actividad: {correos.Count} novedades en ConsultoraPro";
        var body = BuildShell(
            preheader: $"{correos.Count} novedades mientras no estabas.",
            contenido: $"""
                <p style="margin:0 0 6px;font-size:12px;letter-spacing:.08em;text-transform:uppercase;color:{Accent};font-weight:700;">Resumen de actividad</p>
                <h1 style="margin:0 0 8px;font-size:20px;line-height:1.35;color:{Text};font-weight:700;">Hola {Html(usuario.Nombres)}, tienes {correos.Count} novedades</h1>
                <p style="margin:0 0 10px;font-size:14px;line-height:1.7;color:{Text2};">Agrupamos la actividad reciente para no llenar tu bandeja:</p>
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0">{items}</table>
                <div style="padding-top:24px;">{Button(frontendBaseUrl.TrimEnd('/'), "Abrir ConsultoraPro")}</div>
                """,
            frontendBaseUrl: frontendBaseUrl);

        var text = new StringBuilder($"Tienes {correos.Count} novedades en ConsultoraPro:\n\n");
        foreach (var correo in correos.OrderBy(c => c.FechaCreacion))
            text.AppendLine($"- {correo.Titulo}: {correo.Mensaje}");

        return new EmailMessage(usuario.Email!, $"{usuario.Nombres} {usuario.Apellidos}".Trim(), asunto, body, text.ToString());
    }

    private static string Button(string href, string label) => $"""
        <table role="presentation" cellpadding="0" cellspacing="0"><tr>
          <td style="border-radius:8px;background:{Accent};">
            <a href="{href}" style="display:inline-block;padding:11px 22px;font-size:14px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:8px;">{Html(label)}</a>
          </td>
        </tr></table>
        """;

    private static string BuildShell(string preheader, string contenido, string frontendBaseUrl)
    {
        var prefsUrl = $"{frontendBaseUrl.TrimEnd('/')}/configuracion/notificaciones";
        return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <meta name="color-scheme" content="dark">
              <meta name="supported-color-schemes" content="dark">
            </head>
            <body style="margin:0;padding:0;background:{Bg};font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
              <div style="display:none;max-height:0;overflow:hidden;">{Html(preheader)}</div>
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:{Bg};">
                <tr><td align="center" style="padding:32px 16px;">
                  <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;">
                    <tr><td style="padding:0 4px 18px;">
                      <span style="font-size:18px;font-weight:800;color:{Text};letter-spacing:-.01em;">Consultora<span style="color:{Accent};">Pro</span></span>
                    </td></tr>
                    <tr><td style="background:{Card};border:1px solid {Border};border-radius:14px;padding:32px;">
                      {contenido}
                    </td></tr>
                    <tr><td style="padding:20px 4px 0;">
                      <p style="margin:0;font-size:12px;line-height:1.7;color:{Text3};">
                        Recibes este correo por tu actividad en ConsultoraPro.
                        Puedes ajustar qué avisos llegan a tu correo en
                        <a href="{prefsUrl}" style="color:{Text2};">Preferencias de notificación</a>.
                        Los avisos de seguridad no se pueden desactivar.
                      </p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string? BuildLink(string frontendBaseUrl, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;
        return $"{frontendBaseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
    }

    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
