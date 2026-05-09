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
    public DbSet<Desarrollador> Desarrolladores => Set<Desarrollador>();
    public DbSet<Member> Members => Set<Member>();

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
            entity.HasOne(p => p.TipoSolucion)
                  .WithMany(t => t.Proyectos)
                  .HasForeignKey(p => p.TipoSolucionId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(p => p.Desarrolladores)
                  .WithOne(d => d.Proyecto)
                  .HasForeignKey(d => d.ProyectoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Desarrollador>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Rol).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Nombres).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Apellidos).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Correo).HasMaxLength(200);
            entity.Property(m => m.Telefono).HasMaxLength(20);
            entity.Property(m => m.Iniciales).HasMaxLength(2);
            entity.Property(m => m.Puesto).HasMaxLength(100);
        });
    }
}
