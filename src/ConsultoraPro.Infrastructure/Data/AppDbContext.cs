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
    public DbSet<Ambiente> Ambientes => Set<Ambiente>();
    public DbSet<Credencial> Credenciales => Set<Credencial>();
    public DbSet<AuditoriaCredencial> AuditoriasCredenciales => Set<AuditoriaCredencial>();

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
            entity.HasIndex(u => u.Email);
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

        modelBuilder.Entity<Ambiente>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Nombre).IsRequired().HasMaxLength(160);
            entity.Property(a => a.Tipo).HasConversion<string>().HasMaxLength(30);
            entity.Property(a => a.Url).IsRequired().HasMaxLength(300);
            entity.Property(a => a.Tecnologia).IsRequired().HasMaxLength(120);
            entity.Property(a => a.Estado).HasConversion<string>().HasMaxLength(30);
            entity.Property(a => a.UptimePorcentaje).HasPrecision(5, 2);
            entity.Property(a => a.Activo).HasDefaultValue(true);
            entity.Property(a => a.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(a => new { a.ProyectoId, a.Activo });
            entity.HasIndex(a => a.Estado);
            entity.HasOne(a => a.Proyecto)
                  .WithMany(p => p.Ambientes)
                  .HasForeignKey(a => a.ProyectoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Credencial>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(160);
            entity.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(40);
            entity.Property(c => c.Servidor).IsRequired().HasMaxLength(220);
            entity.Property(c => c.ValorCifrado).IsRequired().HasMaxLength(7000);
            entity.Property(c => c.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(c => c.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(c => c.Activo).HasDefaultValue(true);
            entity.HasIndex(c => new { c.ProyectoId, c.Activo });
            entity.HasIndex(c => c.FechaVencimiento);
            entity.HasOne(c => c.Proyecto)
                  .WithMany()
                  .HasForeignKey(c => c.ProyectoId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.Ambiente)
                  .WithMany(a => a.Credenciales)
                  .HasForeignKey(c => c.AmbienteId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(c => c.Creador)
                  .WithMany()
                  .HasForeignKey(c => c.CreadoPor)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditoriaCredencial>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FechaRevelacion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(a => a.Ip).HasMaxLength(80);
            entity.Property(a => a.UserAgent).HasMaxLength(500);
            entity.HasIndex(a => new { a.CredencialId, a.FechaRevelacion });
            entity.HasOne(a => a.Credencial)
                  .WithMany(c => c.Auditorias)
                  .HasForeignKey(a => a.CredencialId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Usuario)
                  .WithMany()
                  .HasForeignKey(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        ApplyAutomaticTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAutomaticTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAutomaticTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Proyecto proyecto)
                {
                    if (proyecto.CreatedAt == default)
                        proyecto.CreatedAt = now;
                    proyecto.UpdatedAt = now;
                }

                if (entry.Entity is Cliente cliente && cliente.FechaAlta == default)
                    cliente.FechaAlta = now;

                if (entry.Entity is Credencial credencial)
                {
                    if (credencial.FechaCreacion == default)
                        credencial.FechaCreacion = now;
                    credencial.UpdatedAt = now;
                }

                if (entry.Entity is Ambiente ambiente && ambiente.FechaCreacion == default)
                    ambiente.FechaCreacion = now;

                if (entry.Entity is AuditoriaCredencial auditoria && auditoria.FechaRevelacion == default)
                    auditoria.FechaRevelacion = now;
            }

            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Proyecto proyecto)
                    proyecto.UpdatedAt = now;

                if (entry.Entity is Credencial credencial)
                    credencial.UpdatedAt = now;
            }
        }
    }
}
