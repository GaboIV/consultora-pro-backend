using ConsultoraPro.Domain.Security;

namespace ConsultoraPro.Tests;

public class PermissionExpanderTests
{
    [Fact]
    public void VerTodos_Implies_VerBase()
    {
        var result = PermissionExpander.Expand(["clientes.ver.todos"]);

        Assert.Contains("clientes.ver.todos", result);
        Assert.Contains("clientes.ver", result);
    }

    [Fact]
    public void NivelFull_Expands_ToAllCredentialActions()
    {
        var result = PermissionExpander.Expand(["credenciales.nivel.full"]);

        Assert.Contains("credenciales.nivel.ver-todo", result);
        Assert.Contains("credenciales.nivel.basico", result);
        Assert.Contains("credenciales.ver", result);
        Assert.Contains("credenciales.revelar", result);
        Assert.Contains("credenciales.crear", result);
        Assert.Contains("credenciales.editar", result);
        Assert.Contains("credenciales.solicitud.aprobar", result);
    }

    [Fact]
    public void NivelBasico_CanList_ButCannotReveal()
    {
        var result = PermissionExpander.Expand(["credenciales.nivel.basico"]);

        Assert.Contains("credenciales.ver", result);
        Assert.DoesNotContain("credenciales.revelar", result);
    }

    [Fact]
    public void NivelVerTodo_CanReveal_ButCannotCreateOrEdit()
    {
        var result = PermissionExpander.Expand(["credenciales.nivel.ver-todo"]);

        Assert.Contains("credenciales.revelar", result);
        Assert.DoesNotContain("credenciales.crear", result);
        Assert.DoesNotContain("credenciales.editar", result);
    }

    [Fact]
    public void UsuariosEditar_Implies_UsuariosVer()
    {
        var result = PermissionExpander.Expand(["usuarios.editar"]);

        Assert.Contains("usuarios.ver", result);
    }

    [Fact]
    public void ScreenshotsEditar_Implies_ScreenshotsVer()
    {
        var result = PermissionExpander.Expand(["screenshots.editar"]);

        Assert.Contains("screenshots.ver", result);
    }

    [Fact]
    public void Expand_IsIdempotent_OverAlreadyExpandedSet()
    {
        var once = PermissionExpander.Expand(["credenciales.nivel.full"]);
        var twice = PermissionExpander.Expand(once);

        Assert.Equal(once.OrderBy(x => x), twice.OrderBy(x => x));
    }

    [Fact]
    public void EveryImplicationTarget_ExistsInCatalog()
    {
        var catalog = PermissionCatalog.All.Select(p => p.Clave)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, implied) in PermissionCatalog.Implies)
        {
            Assert.Contains(key, catalog);
            foreach (var target in implied)
                Assert.Contains(target, catalog);
        }
    }
}
