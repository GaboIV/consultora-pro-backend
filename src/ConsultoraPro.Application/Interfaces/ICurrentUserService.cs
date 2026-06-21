using System;

namespace ConsultoraPro.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Role { get; }

    /// <summary>True si el rol del usuario actual tiene acceso a todos los proyectos.</summary>
    bool HasFullProjectAccess { get; }

    bool IsInRole(string role);
    bool HasPermission(string permission);
}
