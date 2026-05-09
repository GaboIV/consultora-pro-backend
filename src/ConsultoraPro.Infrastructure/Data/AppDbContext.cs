using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<TipoSolucion> TiposSolucion => Set<TipoSolucion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Industria).HasMaxLength(100);
            entity.Property(c => c.Iniciales).HasMaxLength(2);
            entity.Property(c => c.ColorClass).HasMaxLength(20);
            entity.HasMany(c => c.Proyectos)
                  .WithOne(p => p.Cliente)
                  .HasForeignKey(p => p.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TipoSolucion>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Nombre).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Nombre).IsUnique();
        });

        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Etapa).HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.TechLead).HasMaxLength(100);
            entity.Property(p => p.TechLeadIniciales).HasMaxLength(2);
            entity.HasOne(p => p.TipoSolucion)
                  .WithMany(t => t.Proyectos)
                  .HasForeignKey(p => p.TipoSolucionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
