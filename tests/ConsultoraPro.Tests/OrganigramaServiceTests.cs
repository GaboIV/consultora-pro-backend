using ConsultoraPro.Application.DTOs.Organigramas;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Application.Services;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using Moq;

namespace ConsultoraPro.Tests;

public class OrganigramaServiceTests
{
    private readonly Mock<IOrganigramaRepository> _repo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private OrganigramaService BuildService()
    {
        _repo.Setup(r => r.ExistsByNombreAsync(It.IsAny<string>(), It.IsAny<Guid?>())).ReturnsAsync(false);
        _repo.Setup(r => r.GetNodoIdsEnOtrosOrganigramasAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<Guid>());
        _repo.Setup(r => r.GetUsuariosAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new List<ApplicationUser>());
        return new OrganigramaService(_repo.Object, _currentUser.Object);
    }

    private static GuardarOrganigramaNodoDto Nodo(Guid id, Guid? parentId, string cargo, int orden = 0) =>
        new() { Id = id, ParentId = parentId, Cargo = cargo, Orden = orden };

    [Fact]
    public async Task Create_ArbolValido_NormalizaOrdenPorNivel()
    {
        var raiz = Guid.NewGuid();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var service = BuildService();

        var result = await service.CreateAsync(new GuardarOrganigramaDto
        {
            Nombre = "  Equipo Delivery  ",
            Nodos = [Nodo(raiz, null, "Gerente de Operaciones"), Nodo(a, raiz, "Líder Front", 7), Nodo(b, raiz, "Líder Back", 3)]
        });

        Assert.Equal("Equipo Delivery", result.Nombre);
        Assert.Equal(0, result.Nodos.Single(n => n.Id == b).Orden);
        Assert.Equal(1, result.Nodos.Single(n => n.Id == a).Orden);
        _repo.Verify(r => r.CreateAsync(It.Is<Organigrama>(o => o.Nodos.Count == 3)), Times.Once);
    }

    [Fact]
    public async Task Create_Ciclo_Lanza()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var service = BuildService();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new GuardarOrganigramaDto
        {
            Nombre = "Ciclo",
            Nodos = [Nodo(a, b, "A"), Nodo(b, a, "B")]
        }));
        Assert.Contains("ciclo", ex.Message);
    }

    [Fact]
    public async Task Create_PadreInexistente_Lanza()
    {
        var service = BuildService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new GuardarOrganigramaDto
        {
            Nombre = "Huérfano",
            Nodos = [Nodo(Guid.NewGuid(), Guid.NewGuid(), "A")]
        }));
    }

    [Fact]
    public async Task Create_UsuarioInexistente_Lanza()
    {
        var service = BuildService();
        var nodo = Nodo(Guid.NewGuid(), null, "Director");
        nodo.UsuarioId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new GuardarOrganigramaDto
        {
            Nombre = "Con usuario",
            Nodos = [nodo]
        }));
    }

    [Fact]
    public async Task Create_UsuarioVinculado_DescartaNombreLibre()
    {
        var usuario = new ApplicationUser { Id = Guid.NewGuid(), Nombres = "Ana", Apellidos = "García", Email = "ana@x.pe" };
        var service = BuildService();
        _repo.Setup(r => r.GetUsuariosAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new List<ApplicationUser> { usuario });
        var nodo = Nodo(Guid.NewGuid(), null, "Directora");
        nodo.UsuarioId = usuario.Id;
        nodo.NombreLibre = "Texto que debe ignorarse";

        var result = await service.CreateAsync(new GuardarOrganigramaDto { Nombre = "X", Nodos = [nodo] });

        var dto = Assert.Single(result.Nodos);
        Assert.Equal(string.Empty, dto.NombreLibre);
        Assert.Equal("Ana García", dto.Usuario?.NombreCompleto);
    }
}
