using ConsultoraPro.Domain.Enums;
using ConsultoraPro.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConsultoraPro.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<TipoSolucion> TiposSolucion => Set<TipoSolucion>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<ProyectoMiembro> ProyectoMiembros => Set<ProyectoMiembro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.Nombres).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Apellidos).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Telefono).HasMaxLength(20);
            entity.Property(u => u.Iniciales).HasMaxLength(2);
            entity.Property(u => u.Puesto).HasMaxLength(120);
            entity.Property(u => u.Activo).HasDefaultValue(true);
            entity.Property(u => u.FechaAlta).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(u => u.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(r => r.Descripcion).HasMaxLength(300);
            entity.Property(r => r.EsActivo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.Clave).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(120);
            entity.Property(p => p.Modulo).IsRequired().HasMaxLength(80);
            entity.Property(p => p.Descripcion).HasMaxLength(300);
            entity.HasIndex(p => p.Clave).IsUnique();
        });

        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.HasKey(rp => new { rp.RolId, rp.PermisoId });
            entity.Property(rp => rp.Concedido).HasDefaultValue(true);
            entity.HasOne(rp => rp.Rol)
                  .WithMany(r => r.Permisos)
                  .HasForeignKey(rp => rp.RolId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rp => rp.Permiso)
                  .WithMany(p => p.Roles)
                  .HasForeignKey(rp => rp.PermisoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProyectoMiembro>(entity =>
        {
            entity.HasKey(pm => pm.Id);
            entity.HasIndex(pm => new { pm.UsuarioId, pm.ProyectoId }).IsUnique();
            entity.Property(pm => pm.Rol).HasConversion<string>().HasMaxLength(20);
            entity.Property(pm => pm.FechaAsignacion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasOne(pm => pm.Usuario)
                  .WithMany(u => u.ProyectosMiembro)
                  .HasForeignKey(pm => pm.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pm => pm.Proyecto)
                  .WithMany(p => p.ProyectoMiembros)
                  .HasForeignKey(pm => pm.ProyectoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

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
        });
    }
}
