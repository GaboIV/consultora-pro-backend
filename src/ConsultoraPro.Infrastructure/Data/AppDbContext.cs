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
    public DbSet<Repositorio> Repositorios => Set<Repositorio>();
    public DbSet<Despliegue> Despliegues => Set<Despliegue>();
    public DbSet<AmbienteComponente> AmbienteComponentes => Set<AmbienteComponente>();
    public DbSet<AmbienteTestUser> AmbienteTestUsers => Set<AmbienteTestUser>();
    public DbSet<AmbienteCloudResource> AmbienteCloudResources => Set<AmbienteCloudResource>();

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
            entity.Property(a => a.HealthCheckUrl).HasMaxLength(300);
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
            entity.HasMany(a => a.Componentes)
                  .WithOne(c => c.Ambiente)
                  .HasForeignKey(c => c.AmbienteId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(a => a.TestUsers)
                  .WithOne(t => t.Ambiente)
                  .HasForeignKey(t => t.AmbienteId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(a => a.CloudResources)
                  .WithOne(r => r.Ambiente)
                  .HasForeignKey(r => r.AmbienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AmbienteComponente>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Rol).IsRequired().HasMaxLength(80);
            entity.Property(c => c.IpPublica).HasMaxLength(45);
            entity.Property(c => c.IpPrivada).HasMaxLength(45);
            entity.Property(c => c.Hostname).HasMaxLength(220);
            entity.Property(c => c.Tecnologia).HasMaxLength(120);
            entity.Property(c => c.Especificaciones).HasMaxLength(200);
            entity.Property(c => c.Activo).HasDefaultValue(true);
            entity.HasIndex(c => new { c.AmbienteId, c.Activo });
        });

        modelBuilder.Entity<AmbienteTestUser>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.RolAplicacion).IsRequired().HasMaxLength(80);
            entity.Property(t => t.Correo).IsRequired().HasMaxLength(200);
            entity.Property(t => t.PasswordCifrado).IsRequired().HasMaxLength(2000);
            entity.Property(t => t.Notas).HasMaxLength(500);
            entity.Property(t => t.Activo).HasDefaultValue(true);
            entity.HasIndex(t => new { t.AmbienteId, t.Activo });
        });

        modelBuilder.Entity<AmbienteCloudResource>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TipoRecurso).IsRequired().HasMaxLength(80);
            entity.Property(r => r.NombreRecurso).IsRequired().HasMaxLength(200);
            entity.Property(r => r.DeepLink).HasMaxLength(500);
            entity.Property(r => r.Plataforma).IsRequired().HasMaxLength(60).HasDefaultValue("Azure");
            entity.Property(r => r.Ubicacion).HasMaxLength(120);
            entity.Property(r => r.Nota).HasMaxLength(500);
            entity.Property(r => r.Activo).HasDefaultValue(true);
            entity.HasIndex(r => new { r.AmbienteId, r.Activo });
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

        modelBuilder.Entity<Repositorio>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Nombre).IsRequired().HasMaxLength(160);
            entity.Property(r => r.Proveedor).HasConversion<string>().HasMaxLength(30);
            entity.Property(r => r.RamaPrincipal).IsRequired().HasMaxLength(120);
            entity.Property(r => r.Url).IsRequired().HasMaxLength(500);
            entity.Property(r => r.EstadoPipeline).HasConversion<string>().HasMaxLength(30);
            entity.Property(r => r.Activo).HasDefaultValue(true);
            entity.Property(r => r.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(r => new { r.ProyectoId, r.Activo });
            entity.HasIndex(r => r.EstadoPipeline);
            entity.HasOne(r => r.Proyecto)
                  .WithMany(p => p.Repositorios)
                  .HasForeignKey(r => r.ProyectoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Despliegue>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Version).IsRequired().HasMaxLength(60);
            entity.Property(d => d.Notas).HasMaxLength(1000);
            entity.Property(d => d.Estado).HasConversion<string>().HasMaxLength(20);
            entity.Property(d => d.FechaHora).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(d => d.Activo).HasDefaultValue(true);
            entity.HasIndex(d => new { d.ProyectoId, d.Activo });
            entity.HasIndex(d => d.FechaHora);
            entity.HasOne(d => d.Proyecto)
                  .WithMany(p => p.Despliegues)
                  .HasForeignKey(d => d.ProyectoId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Ambiente)
                  .WithMany(a => a.Despliegues)
                  .HasForeignKey(d => d.AmbienteId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.EjecutadoPor)
                  .WithMany()
                  .HasForeignKey(d => d.EjecutadoPorId)
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
