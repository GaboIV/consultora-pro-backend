using System.Collections.Generic;
using System.Threading.Tasks;
using ConsultoraPro.Application.DTOs.Alertas;

namespace ConsultoraPro.Application.Interfaces;

public interface IAlertaService
{
    Task<List<AlertaDto>> GetAlertasActivasAsync();
}
