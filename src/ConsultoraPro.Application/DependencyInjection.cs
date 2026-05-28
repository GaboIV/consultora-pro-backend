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
        services.AddScoped<ICredencialService, CredencialService>();
        services.AddScoped<IAmbienteService, AmbienteService>();
        services.AddScoped<IRepositorioService, RepositorioService>();
        services.AddScoped<IDespliegueService, DespliegueService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IAmbienteComponenteService, AmbienteComponenteService>();
        services.AddScoped<IAmbienteTestUserService, AmbienteTestUserService>();
        services.AddScoped<IAmbienteCloudResourceService, AmbienteCloudResourceService>();
        services.AddScoped<IAzureSubscriptionTenantMappingService, AzureSubscriptionTenantMappingService>();
        services.AddScoped<IAzurePortalUrlResolver, AzurePortalUrlResolver>();
        return services;
    }
}
