using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using SistemaDeGestionDeClub.Domain.Entities;

namespace SistemaDeGestionDeClub.Infrastructure.Data;

public class ClubDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClubDbContext(DbContextOptions<ClubDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
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
    public DbSet<Auditoria> Auditorias { get; set; }
    
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
            entity.Property(e => e.Precio).HasColumnName("precio_actual").HasColumnType("decimal(10, 2)").IsRequired();
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
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.CostoTotal).HasColumnName("costo_total").HasColumnType("decimal(12, 2)").IsRequired();
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(45).HasDefaultValue("activa");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.FechaEliminacion).HasColumnName("fecha_eliminacion");

            entity.HasOne(e => e.Socio)
                .WithMany(s => s.Membresias)
                .HasForeignKey(e => e.IdSocio)
                .OnDelete(DeleteBehavior.Restrict);

            // Ignorar propiedades calculadas (no mapeadas a BD)
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
            entity.Property(e => e.PrecioAlMomento).HasColumnName("precio_mensual_congelado").HasColumnType("decimal(10, 2)").IsRequired();

            entity.HasOne(e => e.Membresia)
                .WithMany(m => m.MembresiaActividades)
                .HasForeignKey(e => e.IdMembresia)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Actividad)
                .WithMany(a => a.MembresiaActividades)
                .HasForeignKey(e => e.IdActividad)
                .OnDelete(DeleteBehavior.Cascade);
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

        // Configuración de Auditoria
        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.ToTable("auditoria");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Tabla).HasColumnName("tabla").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Operacion).HasColumnName("operacion").HasMaxLength(20).IsRequired();
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NombreUsuario).HasColumnName("nombre_usuario").HasMaxLength(100);
            entity.Property(e => e.FechaHora).HasColumnName("fecha_hora").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.ValoresAnteriores).HasColumnName("valores_anteriores");
            entity.Property(e => e.ValoresNuevos).HasColumnName("valores_nuevos");
            entity.Property(e => e.NombreEntidad).HasColumnName("nombre_entidad").HasMaxLength(100);
            entity.Property(e => e.IdEntidad).HasColumnName("id_entidad").HasMaxLength(50);
            entity.Property(e => e.Detalles).HasColumnName("detalles").HasMaxLength(500);

            entity.HasIndex(e => e.Tabla);
            entity.HasIndex(e => e.Operacion);
            entity.HasIndex(e => e.IdUsuario);
            entity.HasIndex(e => e.FechaHora);
        });
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Obtener información del usuario actual
        var usuarioId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var nombreUsuario = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

        // Capturar cambios para auditoría ANTES de guardar
        var auditEntries = new List<AuditoriaEntry>();
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Where(e => e.Entity.GetType() != typeof(Auditoria)) // No auditar la tabla de auditoría
            .ToList();

        foreach (var entry in entries)
        {
            // Actualizar FechaActualizacion automáticamente
            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity.GetType().GetProperty("FechaActualizacion") != null)
                {
                    entry.Property("FechaActualizacion").CurrentValue = DateTime.Now;
                }
            }

            // Capturar valores ANTES de guardar
            var valoresAnteriores = new Dictionary<string, object?>();
            var valoresNuevos = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                // Omitir propiedades de auditoría y timestamps
                if (propertyName == "FechaActualizacion" || propertyName == "FechaCreacion")
                    continue;

                if (entry.State == EntityState.Added)
                {
                    valoresNuevos[propertyName] = property.CurrentValue;
                }
                else if (entry.State == EntityState.Modified && property.IsModified)
                {
                    valoresAnteriores[propertyName] = property.OriginalValue;
                    valoresNuevos[propertyName] = property.CurrentValue;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    valoresAnteriores[propertyName] = property.OriginalValue;
                }
            }

            // Guardar información de auditoría temporal (sin ID aún para INSERT)
            auditEntries.Add(new AuditoriaEntry
            {
                Entry = entry,
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Estado = entry.State,
                ValoresAnteriores = valoresAnteriores,
                ValoresNuevos = valoresNuevos
            });
        }

        // Guardar cambios originales
        var result = await base.SaveChangesAsync(cancellationToken);

        // Ahora crear auditorías con IDs reales (después del SaveChanges)
        if (auditEntries.Any())
        {
            var auditorias = auditEntries.Select(ae => CrearAuditoriaDesdeEntry(ae)).ToList();
            await Auditorias.AddRangeAsync(auditorias, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    // Clase auxiliar para guardar temporalmente la información de auditoría
    private class AuditoriaEntry
    {
        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; set; } = null!;
        public string? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public EntityState Estado { get; set; }
        public Dictionary<string, object?> ValoresAnteriores { get; set; } = new();
        public Dictionary<string, object?> ValoresNuevos { get; set; } = new();
    }

    private Auditoria CrearAuditoriaDesdeEntry(AuditoriaEntry auditEntry)
    {
        var entry = auditEntry.Entry;
        var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
        var operacion = auditEntry.Estado switch
        {
            EntityState.Added => "INSERT",
            EntityState.Modified => "UPDATE",
            EntityState.Deleted => "DELETE",
            _ => "UNKNOWN"
        };

        // Obtener ID de la entidad DESPUÉS de guardar (ahora tiene el ID real)
        string? idEntidad = null;
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        if (idProperty != null)
        {
            idEntidad = idProperty.CurrentValue?.ToString();
        }

        // Generar detalles legibles
        string? detalles = GenerarDetalles(entry.Entity.GetType().Name, idEntidad, operacion);

        return new Auditoria
        {
            Tabla = tableName,
            Operacion = operacion,
            IdUsuario = int.TryParse(auditEntry.UsuarioId, out var uid) ? uid : null,
            NombreUsuario = auditEntry.NombreUsuario,
            FechaHora = DateTime.Now,
            ValoresAnteriores = auditEntry.ValoresAnteriores.Any() ? JsonSerializer.Serialize(auditEntry.ValoresAnteriores) : null,
            ValoresNuevos = auditEntry.ValoresNuevos.Any() ? JsonSerializer.Serialize(auditEntry.ValoresNuevos) : null,
            NombreEntidad = entry.Entity.GetType().Name,
            IdEntidad = idEntidad,
            Detalles = detalles
        };
    }

    private Auditoria CrearAuditoria(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string? usuarioId, string? nombreUsuario)
    {
        var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
        var operacion = entry.State switch
        {
            EntityState.Added => "INSERT",
            EntityState.Modified => "UPDATE",
            EntityState.Deleted => "DELETE",
            _ => "UNKNOWN"
        };

        // Obtener ID de la entidad si existe
        string? idEntidad = null;
        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
        if (idProperty != null)
        {
            idEntidad = idProperty.CurrentValue?.ToString();
        }

        // Capturar valores anteriores y nuevos
        var valoresAnteriores = new Dictionary<string, object?>();
        var valoresNuevos = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            // Omitir propiedades de auditoría y timestamps
            if (propertyName == "FechaActualizacion" || propertyName == "FechaCreacion")
                continue;

            if (entry.State == EntityState.Added)
            {
                valoresNuevos[propertyName] = property.CurrentValue;
            }
            else if (entry.State == EntityState.Modified && property.IsModified)
            {
                valoresAnteriores[propertyName] = property.OriginalValue;
                valoresNuevos[propertyName] = property.CurrentValue;
            }
            else if (entry.State == EntityState.Deleted)
            {
                valoresAnteriores[propertyName] = property.OriginalValue;
            }
        }

        // Generar detalles legibles
        string? detalles = GenerarDetalles(entry, operacion);

        return new Auditoria
        {
            Tabla = tableName,
            Operacion = operacion,
            IdUsuario = int.TryParse(usuarioId, out var uid) ? uid : null,
            NombreUsuario = nombreUsuario,
            FechaHora = DateTime.Now,
            ValoresAnteriores = valoresAnteriores.Any() ? JsonSerializer.Serialize(valoresAnteriores) : null,
            ValoresNuevos = valoresNuevos.Any() ? JsonSerializer.Serialize(valoresNuevos) : null,
            NombreEntidad = entry.Entity.GetType().Name,
            IdEntidad = idEntidad,
            Detalles = detalles
        };
    }

    private string? GenerarDetalles(string entityType, string? id, string operacion)
    {
        try
        {
            return operacion switch
            {
                "INSERT" => $"Creó {entityType} (ID: {id ?? "N/A"})",
                "UPDATE" => $"Modificó {entityType} (ID: {id ?? "N/A"})",
                "DELETE" => $"Eliminó {entityType} (ID: {id ?? "N/A"})",
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private string? GenerarDetalles(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string operacion)
    {
        try
        {
            var entityType = entry.Entity.GetType().Name;
            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            var id = idProperty?.CurrentValue?.ToString();

            return GenerarDetalles(entityType, id, operacion);
        }
        catch
        {
            return null;
        }
    }
}
