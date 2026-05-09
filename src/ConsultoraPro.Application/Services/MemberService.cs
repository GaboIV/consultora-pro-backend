using AutoMapper;
using ConsultoraPro.Application.DTOs.Members;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Application.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _repository;
    private readonly IMapper _mapper;

    public MemberService(IMemberRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MemberDto>> GetAllAsync()
    {
        var members = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<MemberDto>>(members);
    }

    public async Task<MemberDto?> GetByIdAsync(Guid id)
    {
        var member = await _repository.GetByIdAsync(id);
        return member == null ? null : _mapper.Map<MemberDto>(member);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
    {
        var member = _mapper.Map<Member>(dto);
        member.Id = Guid.NewGuid();
        var created = await _repository.CreateAsync(member);
        return _mapper.Map<MemberDto>(created);
    }

    public async Task UpdateAsync(Guid id, UpdateMemberDto dto)
    {
        var member = await _repository.GetByIdAsync(id);
        if (member == null)
            throw new KeyNotFoundException($"Miembro con ID {id} no encontrado");
        _mapper.Map(dto, member);
        await _repository.UpdateAsync(member);
    }

    public async Task DeleteAsync(Guid id)
    {
        var member = await _repository.GetByIdAsync(id);
        if (member == null)
            throw new KeyNotFoundException($"Miembro con ID {id} no encontrado");
        await _repository.DeleteAsync(member);
    }
}
