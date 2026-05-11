using ConsultoraPro.Application.Interfaces;
using ConsultoraPro.Domain.Interfaces;
using ConsultoraPro.Domain.Models;
using ConsultoraPro.Infrastructure.Data;
using ConsultoraPro.Infrastructure.Data.Seed;
using ConsultoraPro.Infrastructure.Repositories;
using ConsultoraPro.Infrastructure.Security;
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
            options.Password.RequireDigit = true;
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
        services.AddScoped<IAmbienteRepository, AmbienteRepository>();
        services.AddScoped<IRepositorioRepository, RepositorioRepository>();
        services.AddScoped<IEncryptionService, EncryptionService>();


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
        await SecuritySeeder.SeedDefaultUserAsync(userManager, roleManager);
        await DataSeeder.SeedAsync(context, userManager, roleManager);
    }
}
