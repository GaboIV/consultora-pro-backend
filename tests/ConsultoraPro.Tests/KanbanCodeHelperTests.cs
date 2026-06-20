using ConsultoraPro.Application.Kanban;

namespace ConsultoraPro.Tests;

public class KanbanCodeHelperTests
{
    [Theory]
    [InlineData("Repsol", 3, "REP")]
    [InlineData("Tareas", 3, "TAR")]
    [InlineData("QA / Bugs", 3, "QAB")]
    [InlineData("Diseño", 3, "DIS")]
    [InlineData("  espacios  iniciales", 3, "ESP")]
    [InlineData("Roadmap", 2, "RO")]
    public void DeriveKey_DerivaClaveEsperada(string nombre, int longitud, string esperado)
    {
        Assert.Equal(esperado, KanbanCodeHelper.DeriveKey(nombre, longitud));
    }

    [Fact]
    public void DeriveKey_SoloSimbolos_DevuelveFallback()
    {
        var clave = KanbanCodeHelper.DeriveKey("/// --- ###", 3);
        Assert.False(string.IsNullOrWhiteSpace(clave));
    }

    [Theory]
    [InlineData("REP", "TAR", 1, "REP-TAR-001")]
    [InlineData("REP", "TAR", 42, "REP-TAR-042")]
    [InlineData("REP", "TAR", 999, "REP-TAR-999")]
    [InlineData("REP", "TAR", 1000, "REP-TAR-1000")]
    public void FormatCodigo_AplicaPaddingYCrecimiento(string proyecto, string tablero, int numero, string esperado)
    {
        Assert.Equal(esperado, KanbanCodeHelper.FormatCodigo(proyecto, tablero, numero));
    }

    [Fact]
    public void FormatCodigo_SecuenciaIncrementalEsUnica()
    {
        var codigos = Enumerable.Range(1, 5)
            .Select(n => KanbanCodeHelper.FormatCodigo("REP", "TAR", n))
            .ToList();

        Assert.Equal(codigos.Count, codigos.Distinct().Count());
        Assert.Equal(new[] { "REP-TAR-001", "REP-TAR-002", "REP-TAR-003", "REP-TAR-004", "REP-TAR-005" }, codigos);
    }
}

public class FractionalOrderTests
{
    [Fact]
    public void Between_SinVecinos_DevuelveStep()
    {
        Assert.Equal(FractionalOrder.Step, FractionalOrder.Between(null, null));
    }

    [Fact]
    public void Between_SoloAnterior_AppendAlFinal()
    {
        Assert.Equal(1000d + FractionalOrder.Step, FractionalOrder.Between(1000d, null));
    }

    [Fact]
    public void Between_SoloSiguiente_PrependAlInicio()
    {
        Assert.Equal(1000d - FractionalOrder.Step, FractionalOrder.Between(null, 1000d));
    }

    [Fact]
    public void Between_DosVecinos_DevuelvePuntoMedio()
    {
        Assert.Equal(1500d, FractionalOrder.Between(1000d, 2000d));
    }

    [Fact]
    public void Between_InsercionesRepetidas_MantienenOrden()
    {
        // Insertar repetidamente entre A y el último valor calculado mantiene a < nuevo < b.
        double a = 1000d, b = 2000d;
        var anterior = b;
        for (var i = 0; i < 10; i++)
        {
            var nuevo = FractionalOrder.Between(a, anterior);
            Assert.True(nuevo > a && nuevo < anterior);
            anterior = nuevo;
        }
    }
}
