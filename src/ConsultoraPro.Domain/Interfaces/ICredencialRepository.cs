using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface ICredencialRepository
{
    Task<IEnumerable<Credencial>> GetAllAsync(Guid? proyectoId = null);
    Task<Credencial?> GetByIdAsync(Guid id);
    Task<Credencial> CreateAsync(Credencial credencial);
    Task UpdateAsync(Credencial credencial);
    Task<IEnumerable<Credencial>> GetExpiringWithinAsync(int days);
    Task AddAuditAsync(AuditoriaCredencial auditoria);
    Task<IEnumerable<AuditoriaCredencial>> GetAuditAsync(Guid credencialId);
}
