using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Domain.Entities;

namespace SistemaDeGestionDeClub.Infrastructure.Data;

public class ClubDbContext : DbContext
{
    public ClubDbContext(DbContextOptions<ClubDbContext> options) : base(options)
    {
    }
    
    public DbSet<Persona> Personas { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Socio> Socios { get; set; }
    public DbSet<Actividad> Actividades { get; set; }
    public DbSet<Membresia> Membresias { get; set; }
    public DbSet<MembresiaActividad> MembresiaActividades { get; set; }
    public DbSet<MetodoPago> MetodosPago { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<Asistencia> Asistencias { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuración de Persona
        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("personas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Apellido).HasColumnName("apellido").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Dni).HasColumnName("dni").HasMaxLength(20);
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Dni).IsUnique();
        });
        
        // Configuración de Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            
            entity.HasIndex(e => e.Nombre).IsUnique();
        });
        
        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdPersona).HasColumnName("id_persona");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.NombreUsuario).HasColumnName("nombre_usuario").HasMaxLength(50).IsRequired();
            entity.Property(e => e.ContrasenaHash).HasColumnName("contrasena_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.EstaActivo).HasColumnName("esta_activo").HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");
            
            entity.HasIndex(e => e.NombreUsuario).IsUnique();
            entity.HasIndex(e => e.IdPersona).IsUnique();
            
            entity.HasOne(e => e.Persona)
                .WithOne(p => p.Usuario)
                .HasForeignKey<Usuario>(e => e.IdPersona)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(e => e.IdRol)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuración de Socio
        modelBuilder.Entity<Socio>(entity =>
        {
            entity.ToTable("socios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdPersona).HasColumnName("id_persona");
            entity.Property(e => e.NumeroSocio).HasColumnName("numero_socio").HasMaxLength(20).IsRequired();
            entity.Property(e => e.EstaActivo).HasColumnName("esta_activo").HasDefaultValue(true);
            entity.Property(e => e.FechaAlta).HasColumnName("fecha_alta").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");
            
            entity.HasIndex(e => e.NumeroSocio).IsUnique();
            entity.HasIndex(e => e.IdPersona).IsUnique();
            
            entity.HasOne(e => e.Persona)
                .WithOne(p => p.Socio)
                .HasForeignKey<Socio>(e => e.IdPersona)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuración de Actividad
        modelBuilder.Entity<Actividad>(entity =>
        {
            entity.ToTable("actividades");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
            entity.Property(e => e.Precio).HasColumnName("precio").HasColumnType("decimal(10, 2)").IsRequired();
            entity.Property(e => e.EsCuotaBase).HasColumnName("es_cuota_base").HasDefaultValue(false);
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");
            
            entity.HasIndex(e => e.Nombre).IsUnique();
        });
        
        // Configuración de Membresia
        modelBuilder.Entity<Membresia>(entity =>
        {
            entity.ToTable("membresias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdSocio).HasColumnName("id_socio");
            entity.Property(e => e.PeriodoAnio).HasColumnName("periodo_anio");
            entity.Property(e => e.PeriodoMes).HasColumnName("periodo_mes");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");
            
            entity.HasIndex(e => new { e.IdSocio, e.PeriodoAnio, e.PeriodoMes }).IsUnique();
            
            entity.HasOne(e => e.Socio)
                .WithMany(s => s.Membresias)
                .HasForeignKey(e => e.IdSocio)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.Ignore(e => e.TotalCargado);
            entity.Ignore(e => e.TotalPagado);
            entity.Ignore(e => e.Saldo);
        });
        
        // Configuración de MembresiaActividad
        modelBuilder.Entity<MembresiaActividad>(entity =>
        {
            entity.ToTable("membresia_actividades");
            entity.HasKey(e => new { e.IdMembresia, e.IdActividad });
            entity.Property(e => e.IdMembresia).HasColumnName("id_membresia");
            entity.Property(e => e.IdActividad).HasColumnName("id_actividad");
            entity.Property(e => e.PrecioAlMomento).HasColumnName("precio_al_momento").HasColumnType("decimal(10, 2)").IsRequired();
            
            entity.HasOne(e => e.Membresia)
                .WithMany(m => m.MembresiaActividades)
                .HasForeignKey(e => e.IdMembresia)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Actividad)
                .WithMany(a => a.MembresiaActividades)
                .HasForeignKey(e => e.IdActividad)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configuración de MetodoPago
        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.ToTable("metodos_pago");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            
            entity.HasIndex(e => e.Nombre).IsUnique();
        });
        
        // Configuración de Pago
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("pagos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdMembresia).HasColumnName("id_membresia");
            entity.Property(e => e.IdMetodoPago).HasColumnName("id_metodo_pago");
            entity.Property(e => e.IdUsuarioProcesa).HasColumnName("id_usuario_procesa");
            entity.Property(e => e.Monto).HasColumnName("monto").HasColumnType("decimal(10, 2)").IsRequired();
            entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");
            
            entity.HasOne(e => e.Membresia)
                .WithMany(m => m.Pagos)
                .HasForeignKey(e => e.IdMembresia)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.MetodoPago)
                .WithMany(mp => mp.Pagos)
                .HasForeignKey(e => e.IdMetodoPago)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.UsuarioProcesa)
                .WithMany(u => u.PagosProcesados)
                .HasForeignKey(e => e.IdUsuarioProcesa)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Configuración de Asistencia
        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.ToTable("asistencias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdSocio).HasColumnName("id_socio");
            entity.Property(e => e.FechaHoraIngreso).HasColumnName("fecha_hora_ingreso").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");
            
            entity.HasIndex(e => new { e.IdSocio, e.FechaHoraIngreso });
            
            entity.HasOne(e => e.Socio)
                .WithMany(s => s.Asistencias)
                .HasForeignKey(e => e.IdSocio)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Actualizar automáticamente FechaActualizacion
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);
        
        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetProperty("FechaActualizacion") != null)
            {
                entry.Property("FechaActualizacion").CurrentValue = DateTime.Now;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}
