using ConsultoraPro.Application.DTOs.Credenciales;
using ConsultoraPro.Application.Exceptions;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Application.Services;
using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using FluentValidation;
using Moq;

namespace ConsultoraPro.Tests;

public class CredencialRevealFlowTests
{
    private readonly Mock<ICredencialRepository> _credRepo = new();
    private readonly Mock<ISolicitudRevelacionRepository> _solRepo = new();
    private readonly Mock<IProyectoRepository> _proyRepo = new();
    private readonly Mock<IAmbienteRepository> _ambRepo = new();
    private readonly Mock<IEncryptionService> _encryption = new();
    private readonly Mock<IValidator<CreateCredencialDto>> _validator = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IProjectScope> _projectScope = new();

    private static readonly Guid CredId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    private CredencialService BuildService()
    {
        _credRepo.Setup(r => r.GetByIdAsync(CredId)).ReturnsAsync(new Credencial
        {
            Id = CredId,
            Nombre = "DB Prod",
            ValorCifrado = "cifrado",
            Activo = true,
            ProyectoId = Guid.NewGuid()
        });
        _encryption.Setup(e => e.Decrypt("cifrado")).Returns("s3cr3t");

        return new CredencialService(
            _credRepo.Object, _solRepo.Object, _proyRepo.Object,
            _ambRepo.Object, _encryption.Object, _validator.Object,
            _currentUser.Object, _projectScope.Object);
    }

    [Fact]
    public async Task Reveal_DirectPermission_ReturnsSecret_WithoutApproval()
    {
        var service = BuildService();

        var result = await service.RevealAsync(CredId, UserId, puedeRevelarDirecto: true, "ip", "ua");

        Assert.Equal("s3cr3t", result.Valor);
        _solRepo.Verify(r => r.GetVigenteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>()), Times.Never);
        _credRepo.Verify(r => r.AddAuditAsync(It.Is<AuditoriaCredencial>(a => a.Accion == "Lectura")), Times.Once);
    }

    [Fact]
    public async Task Reveal_BasicLevel_WithoutApproval_Throws()
    {
        _solRepo.Setup(r => r.GetVigenteAsync(CredId, UserId, It.IsAny<DateTime>()))
            .ReturnsAsync((SolicitudRevelacionCredencial?)null);
        var service = BuildService();

        await Assert.ThrowsAsync<RevelacionRequiereSolicitudException>(() =>
            service.RevealAsync(CredId, UserId, puedeRevelarDirecto: false, "ip", "ua"));

        _credRepo.Verify(r => r.AddAuditAsync(It.IsAny<AuditoriaCredencial>()), Times.Never);
    }

    [Fact]
    public async Task Reveal_BasicLevel_WithVigenteApproval_ReturnsSecret_AndAudits()
    {
        _solRepo.Setup(r => r.GetVigenteAsync(CredId, UserId, It.IsAny<DateTime>()))
            .ReturnsAsync(new SolicitudRevelacionCredencial
            {
                Id = Guid.NewGuid(),
                Estado = EstadoSolicitudRevelacion.Aprobada,
                VigenteHasta = DateTime.UtcNow.AddMinutes(10)
            });
        var service = BuildService();

        var result = await service.RevealAsync(CredId, UserId, puedeRevelarDirecto: false, "ip", "ua");

        Assert.Equal("s3cr3t", result.Valor);
        _credRepo.Verify(r => r.AddAuditAsync(It.Is<AuditoriaCredencial>(a => a.Detalle != null)), Times.Once);
    }

    [Fact]
    public async Task ResolverSolicitud_Approve_SetsVigenteHasta_InFuture()
    {
        var solicitud = new SolicitudRevelacionCredencial
        {
            Id = Guid.NewGuid(),
            CredencialId = CredId,
            SolicitanteId = UserId,
            Estado = EstadoSolicitudRevelacion.Pendiente
        };
        _solRepo.Setup(r => r.GetByIdAsync(solicitud.Id)).ReturnsAsync(solicitud);
        var service = BuildService();

        var dto = await service.ResolverSolicitudAsync(solicitud.Id, Guid.NewGuid(), aprobar: true, "ok");

        Assert.Equal(EstadoSolicitudRevelacion.Aprobada, dto.Estado);
        Assert.NotNull(dto.VigenteHasta);
        Assert.True(dto.VigenteHasta > DateTime.UtcNow);
        _solRepo.Verify(r => r.UpdateAsync(It.IsAny<SolicitudRevelacionCredencial>()), Times.Once);
    }

    [Fact]
    public async Task ResolverSolicitud_AlreadyResolved_Throws()
    {
        var solicitud = new SolicitudRevelacionCredencial
        {
            Id = Guid.NewGuid(),
            Estado = EstadoSolicitudRevelacion.Aprobada
        };
        _solRepo.Setup(r => r.GetByIdAsync(solicitud.Id)).ReturnsAsync(solicitud);
        var service = BuildService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ResolverSolicitudAsync(solicitud.Id, Guid.NewGuid(), aprobar: false, null));
    }
}
