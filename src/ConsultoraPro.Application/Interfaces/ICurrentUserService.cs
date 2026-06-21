using System;

namespace ConsultoraPro.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
}
