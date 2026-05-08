using ConsultoraPro.Application.DTOs.Management;

namespace ConsultoraPro.Application.Interfaces;

public interface IManagementService
{
    Task<ManagementSnapshotDto> GetSnapshotAsync();
}
