using ConsultoraPro.Application.DTOs.Members;

namespace ConsultoraPro.Application.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<MemberDto>> GetAllAsync();
    Task<MemberDto?> GetByIdAsync(Guid id);
    Task<MemberDto> CreateAsync(CreateMemberDto dto);
    Task UpdateAsync(Guid id, UpdateMemberDto dto);
    Task DeleteAsync(Guid id);
}
