using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultoraPro.Domain.Models;

namespace ConsultoraPro.Domain.Interfaces;

public interface IScreenshotRepository
{
    Task<IEnumerable<Screenshot>> GetByProyectoIdAsync(Guid proyectoId);
    Task<Screenshot?> GetByIdAsync(Guid id);
    Task<Screenshot> CreateAsync(Screenshot screenshot);
    Task DeleteAsync(Screenshot screenshot);
}
