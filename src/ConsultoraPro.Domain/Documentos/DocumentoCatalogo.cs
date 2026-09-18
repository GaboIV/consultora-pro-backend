using ConsultoraPro.Domain.Enums;

namespace ConsultoraPro.Domain.Documentos;

public sealed record CarpetaPlantilla(string Codigo, string Nombre, string Descripcion, IReadOnlyList<CarpetaPlantilla> Hijas);

/// <summary>
/// Lineamientos corporativos del repositorio documental: estructura estándar de carpetas para
/// proyectos SAP con integraciones (SAP, SUNAT, terceros) y siglas de nomenclatura por tipo.
/// </summary>
public static class DocumentoCatalogo
{
    public const int MaxProfundidadCarpetas = 3;
    public const int MaxEtiquetasPorDocumento = 15;

    public static readonly IReadOnlyDictionary<TipoDocumento, string> Siglas = new Dictionary<TipoDocumento, string>
    {
        [TipoDocumento.ActaReunion] = "ACT",
        [TipoDocumento.PlanProyecto] = "PLN",
        [TipoDocumento.Cronograma] = "CRO",
        [TipoDocumento.InformeAvance] = "INF",
        [TipoDocumento.Requerimiento] = "REQ",
        [TipoDocumento.EspecificacionFuncional] = "EF",
        [TipoDocumento.EspecificacionTecnica] = "ET",
        [TipoDocumento.Arquitectura] = "ARQ",
        [TipoDocumento.MatrizIntegracion] = "INT",
        [TipoDocumento.PlanPruebas] = "PP",
        [TipoDocumento.CasoPrueba] = "CP",
        [TipoDocumento.EvidenciaPruebas] = "EVI",
        [TipoDocumento.GuiaDespliegue] = "DEP",
        [TipoDocumento.ManualUsuario] = "MU",
        [TipoDocumento.ManualTecnico] = "MT",
        [TipoDocumento.Presentacion] = "PRE",
        [TipoDocumento.Contractual] = "CON",
        [TipoDocumento.Otro] = "DOC"
    };

    public static readonly IReadOnlyDictionary<TipoDocumento, string> Etiquetas = new Dictionary<TipoDocumento, string>
    {
        [TipoDocumento.ActaReunion] = "Acta de reunión",
        [TipoDocumento.PlanProyecto] = "Plan de proyecto",
        [TipoDocumento.Cronograma] = "Cronograma",
        [TipoDocumento.InformeAvance] = "Informe de avance",
        [TipoDocumento.Requerimiento] = "Requerimiento",
        [TipoDocumento.EspecificacionFuncional] = "Especificación funcional",
        [TipoDocumento.EspecificacionTecnica] = "Especificación técnica",
        [TipoDocumento.Arquitectura] = "Arquitectura",
        [TipoDocumento.MatrizIntegracion] = "Matriz de integración",
        [TipoDocumento.PlanPruebas] = "Plan de pruebas",
        [TipoDocumento.CasoPrueba] = "Casos de prueba",
        [TipoDocumento.EvidenciaPruebas] = "Evidencia de pruebas",
        [TipoDocumento.GuiaDespliegue] = "Guía de despliegue",
        [TipoDocumento.ManualUsuario] = "Manual de usuario",
        [TipoDocumento.ManualTecnico] = "Manual técnico",
        [TipoDocumento.Presentacion] = "Presentación",
        [TipoDocumento.Contractual] = "Contractual",
        [TipoDocumento.Otro] = "Otro"
    };

    public static readonly IReadOnlyDictionary<EstadoDocumento, string> EstadoEtiquetas = new Dictionary<EstadoDocumento, string>
    {
        [EstadoDocumento.Borrador] = "Borrador",
        [EstadoDocumento.EnRevision] = "En revisión",
        [EstadoDocumento.Aprobado] = "Aprobado",
        [EstadoDocumento.Obsoleto] = "Obsoleto"
    };

    private static CarpetaPlantilla C(string codigo, string nombre, string descripcion, params CarpetaPlantilla[] hijas)
        => new(codigo, nombre, descripcion, hijas);

    /// <summary>Estructura estándar que se provisiona en cada proyecto al abrir su documentación.</summary>
    public static readonly IReadOnlyList<CarpetaPlantilla> EstructuraCorporativa =
    [
        C("01", "Gestión del proyecto", "Plan, cronograma, riesgos y seguimiento.",
            C("01.1", "Actas de reunión", "Kick-off, comités y reuniones de seguimiento."),
            C("01.2", "Informes de avance", "Reportes de estado semanales/mensuales.")),
        C("02", "Requerimientos y análisis", "Levantamiento, procesos AS-IS/TO-BE y brechas (GAP)."),
        C("03", "Diseño funcional", "Especificaciones funcionales SAP (RICEFW) y documentos de proceso."),
        C("04", "Diseño técnico", "Especificaciones técnicas, arquitectura y modelo de datos."),
        C("05", "Integraciones", "Mapeos, contratos de interfaz y catálogos de mensajes.",
            C("05.1", "SAP", "IDocs, RFC/BAPI, OData, SAP CPI/PI-PO."),
            C("05.2", "SUNAT", "CPE, GRE, OSE/PSE, XML UBL 2.1 y CDR."),
            C("05.3", "Terceros y bancos", "APIs externas, archivos bancarios y otros sistemas.")),
        C("06", "Desarrollo y configuración", "Configuración (customizing), transportes y notas de desarrollo."),
        C("07", "Pruebas y QA", "Planes, casos, evidencias y UAT.",
            C("07.1", "Plan y casos de prueba", "Estrategia de pruebas y matrices de casos."),
            C("07.2", "Evidencias", "Resultados de pruebas unitarias, integrales y UAT.")),
        C("08", "Pase a producción", "Guías de despliegue, checklist de go-live, rollback y RFC de cambio."),
        C("09", "Capacitación y manuales", "Manuales de usuario, material de capacitación y videos."),
        C("10", "Soporte y operación", "Hypercare, runbooks, incidencias y lecciones aprendidas."),
        C("11", "Contractual y comercial", "Propuesta, contrato, órdenes de compra y actas de conformidad.")
    ];

    public static string Sigla(TipoDocumento tipo) => Siglas.TryGetValue(tipo, out var s) ? s : "DOC";

    /// <summary>Código corporativo: {PREFIJO}-{SIGLA}-{NNN}. El prefijo es la sigla del cliente o la clave del proyecto.</summary>
    public static string GenerarCodigo(string prefijo, TipoDocumento tipo, int correlativo)
    {
        var p = new string((prefijo ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (p.Length == 0) p = "PRJ";
        return $"{p}-{Sigla(tipo)}-{correlativo:D3}";
    }
}
