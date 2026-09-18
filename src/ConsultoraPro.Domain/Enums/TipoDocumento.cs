namespace ConsultoraPro.Domain.Enums;

/// <summary>
/// Tipología corporativa de documentos de proyecto. Cada tipo aporta la sigla usada en la
/// nomenclatura del código de documento ({CLIENTE}-{SIGLA}-{NNN}); ver <c>DocumentoCatalogo</c>.
/// </summary>
public enum TipoDocumento
{
    ActaReunion,
    PlanProyecto,
    Cronograma,
    InformeAvance,
    Requerimiento,
    EspecificacionFuncional,
    EspecificacionTecnica,
    Arquitectura,
    MatrizIntegracion,
    PlanPruebas,
    CasoPrueba,
    EvidenciaPruebas,
    GuiaDespliegue,
    ManualUsuario,
    ManualTecnico,
    Presentacion,
    Contractual,
    Otro
}
