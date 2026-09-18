using ConsultoraPro.Domain.Documentos;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Security;

namespace ConsultoraPro.Tests;

public class DocumentoCatalogoTests
{
    [Theory]
    [InlineData("CMH", TipoDocumento.EspecificacionFuncional, 3, "CMH-EF-003")]
    [InlineData("c.m-h", TipoDocumento.MatrizIntegracion, 12, "CMH-INT-012")]
    [InlineData("", TipoDocumento.Otro, 1, "PRJ-DOC-001")]
    [InlineData("REP", TipoDocumento.ActaReunion, 1000, "REP-ACT-1000")]
    public void GenerarCodigo_AplicaNomenclaturaCorporativa(string prefijo, TipoDocumento tipo, int correlativo, string esperado)
    {
        Assert.Equal(esperado, DocumentoCatalogo.GenerarCodigo(prefijo, tipo, correlativo));
    }

    [Fact]
    public void TodosLosTipos_TienenSiglaUnicaYEtiqueta()
    {
        var tipos = Enum.GetValues<TipoDocumento>();
        Assert.All(tipos, t => Assert.True(DocumentoCatalogo.Siglas.ContainsKey(t), $"Falta sigla para {t}"));
        Assert.All(tipos, t => Assert.True(DocumentoCatalogo.Etiquetas.ContainsKey(t), $"Falta etiqueta para {t}"));
        Assert.Equal(DocumentoCatalogo.Siglas.Count, DocumentoCatalogo.Siglas.Values.Distinct().Count());
    }

    [Fact]
    public void EstructuraCorporativa_CodigosUnicosYProfundidadPermitida()
    {
        var codigos = new List<string>();
        void Recorrer(IReadOnlyList<CarpetaPlantilla> carpetas, int nivel)
        {
            Assert.True(nivel <= DocumentoCatalogo.MaxProfundidadCarpetas);
            foreach (var c in carpetas)
            {
                codigos.Add(c.Codigo);
                Recorrer(c.Hijas, nivel + 1);
            }
        }
        Recorrer(DocumentoCatalogo.EstructuraCorporativa, 1);

        Assert.Equal(codigos.Count, codigos.Distinct().Count());
        Assert.Contains("05.2", codigos); // SUNAT
    }

    [Fact]
    public void PermisosDeDocumentos_EditarYEliminarImplicanVer()
    {
        Assert.Contains("documentos.ver", PermissionCatalog.Implies["documentos.editar"]);
        Assert.Contains("documentos.ver", PermissionCatalog.Implies["documentos.eliminar"]);
    }
}
