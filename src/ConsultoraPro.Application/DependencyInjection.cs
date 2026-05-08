using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsultoraPro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<IManagementService, ManagementService>();
        return services;
    }
}
