using ConsultoraPro.Application.Configuration;
using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using ConsultoraPro.Infrastructure.Data.Seed;
using ConsultoraPro.Infrastructure.Repositories;
using ConsultoraPro.Infrastructure.Security;
using ConsultoraPro.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsultoraPro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 0))
            ));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProyectoRepository, ProyectoRepository>();
        services.AddScoped<ITipoSolucionRepository, TipoSolucionRepository>();
        services.AddScoped<ICredencialRepository, CredencialRepository>();
        services.AddScoped<ISolicitudRevelacionRepository, SolicitudRevelacionRepository>();
        services.AddScoped<IAmbienteRepository, AmbienteRepository>();
        services.AddScoped<IRepositorioRepository, RepositorioRepository>();
        services.AddScoped<IDespliegueRepository, DespliegueRepository>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IAmbienteComponenteRepository, AmbienteComponenteRepository>();
        services.AddScoped<IAmbienteTestUserRepository, AmbienteTestUserRepository>();
        services.AddScoped<IAmbienteCloudResourceRepository, AmbienteCloudResourceRepository>();
        services.AddScoped<IAzureSubscriptionTenantMappingRepository, AzureSubscriptionTenantMappingRepository>();
        services.AddScoped<IScreenshotRepository, ScreenshotRepository>();

        // Almacenamiento de archivos: proveedor seleccionable por configuración.
        // Local (filesystem) en dev; Azure Blob (contenedor privado + SAS) en QA/Prod.
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .ValidateOnStart();

        if (string.Equals(configuration[$"{StorageOptions.SectionName}:Provider"], "AzureBlob",
                StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IStorageService, AzureBlobStorageService>();
        else
            services.AddScoped<IStorageService, LocalStorageService>();

        services.AddScoped<ITableroRepository, TableroRepository>();
        services.AddScoped<IColumnaKanbanRepository, ColumnaKanbanRepository>();
        services.AddScoped<ITarjetaRepository, TarjetaRepository>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<ApplicationRole>>();
        await context.Database.MigrateAsync();
        await SecuritySeeder.SeedPermisosAsync(context);
        await SecuritySeeder.SeedRolesAsync(roleManager);
        await SecuritySeeder.SeedRolPermisosAsync(context, roleManager);
        await SecuritySeeder.MigrateLegacyGrantsAsync(context, roleManager);
        await SecuritySeeder.SeedDefaultUserAsync(userManager, roleManager);
        await DataSeeder.SeedAsync(context, userManager, roleManager);
        await KanbanSeeder.SeedAsync(context);
    }
}
