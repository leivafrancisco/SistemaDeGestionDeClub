using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IAsistenciaService
{
    Task<VerificarAsistenciaDto> VerificarEstadoSocioAsync(string dni);
    Task<AsistenciaDto> RegistrarAsistenciaAsync(string dni);
    Task<List<AsistenciaDto>> ObtenerAsistenciasAsync(DateTime? fecha = null, int? idSocio = null);
}

public class AsistenciaService : IAsistenciaService
{
    private readonly ClubDbContext _context;

    public AsistenciaService(ClubDbContext context)
    {
        _context = context;
    }

    public async Task<VerificarAsistenciaDto> VerificarEstadoSocioAsync(string dni)
    {
        // Buscar socio por DNI
        var socio = await _context.Socios
            .Include(s => s.Persona)
            .FirstOrDefaultAsync(s => s.Persona.Dni == dni && s.FechaEliminacion == null);

        if (socio == null)
        {
            throw new InvalidOperationException($"No se encontró un socio con DNI {dni}");
        }

        // Verificar si el socio está activo
        if (!socio.EstaActivo)
        {
            return new VerificarAsistenciaDto
            {
                IdSocio = socio.Id,
                NumeroSocio = socio.NumeroSocio,
                NombreSocio = $"{socio.Persona.Nombre} {socio.Persona.Apellido}",
                Dni = socio.Persona.Dni,
                TieneAcceso = false,
                Mensaje = "Socio inactivo. No tiene acceso a las instalaciones.",
                EstadoMembresia = "INACTIVO"
            };
        }

        // Buscar la membresía más reciente del socio
        var fechaActual = DateTime.Now;
        var membresiaActiva = await _context.Membresias
            .Include(m => m.MembresiaActividades)
                .ThenInclude(ma => ma.Actividad)
            .Include(m => m.Pagos)  // Incluir pagos para calcular el saldo correctamente
            .Where(m =>
                m.IdSocio == socio.Id &&
                m.FechaEliminacion == null &&
                m.FechaInicio <= fechaActual &&
                m.FechaFin >= fechaActual)
            .OrderByDescending(m => m.FechaInicio)
            .FirstOrDefaultAsync();

        if (membresiaActiva == null)
        {
            return new VerificarAsistenciaDto
            {
                IdSocio = socio.Id,
                NumeroSocio = socio.NumeroSocio,
                NombreSocio = $"{socio.Persona.Nombre} {socio.Persona.Apellido}",
                Dni = socio.Persona.Dni,
                TieneAcceso = false,
                Mensaje = "No tiene membresía activa vigente. Por favor, renueve su membresía.",
                EstadoMembresia = "SIN MEMBRESIA VIGENTE"
            };
        }

        // Verificar si tiene saldo pendiente
        var tieneSaldoPendiente = membresiaActiva.Saldo > 0;

        if (tieneSaldoPendiente)
        {
            return new VerificarAsistenciaDto
            {
                IdSocio = socio.Id,
                NumeroSocio = socio.NumeroSocio,
                NombreSocio = $"{socio.Persona.Nombre} {socio.Persona.Apellido}",
                Dni = socio.Persona.Dni,
                TieneAcceso = false,
                Mensaje = $"Tiene saldo pendiente de ${membresiaActiva.Saldo:N2}. Por favor, regularice su situación.",
                EstadoMembresia = "SALDO PENDIENTE",
                SaldoPendiente = membresiaActiva.Saldo,
                Actividades = membresiaActiva.MembresiaActividades
                    .Select(ma => ma.Actividad.Nombre)
                    .ToList()
            };
        }

        // Todo OK - tiene acceso
        return new VerificarAsistenciaDto
        {
            IdSocio = socio.Id,
            NumeroSocio = socio.NumeroSocio,
            NombreSocio = $"{socio.Persona.Nombre} {socio.Persona.Apellido}",
            Dni = socio.Persona.Dni,
            TieneAcceso = true,
            Mensaje = "✓ Socio con actividades al día. Acceso autorizado.",
            EstadoMembresia = "AL DIA",
            FechaVigenciaHasta = membresiaActiva.FechaFin,
            Actividades = membresiaActiva.MembresiaActividades
                .Select(ma => ma.Actividad.Nombre)
                .ToList()
        };
    }

    public async Task<AsistenciaDto> RegistrarAsistenciaAsync(string dni)
    {
        // Primero verificar el estado
        var verificacion = await VerificarEstadoSocioAsync(dni);

        if (!verificacion.TieneAcceso)
        {
            throw new InvalidOperationException(verificacion.Mensaje);
        }

        // Registrar asistencia
        var asistencia = new Asistencia
        {
            IdSocio = verificacion.IdSocio,
            FechaHoraIngreso = DateTime.Now,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Asistencias.Add(asistencia);
        await _context.SaveChangesAsync();

        return new AsistenciaDto
        {
            Id = asistencia.Id,
            IdSocio = asistencia.IdSocio,
            NumeroSocio = verificacion.NumeroSocio,
            NombreSocio = verificacion.NombreSocio,
            FechaHoraIngreso = asistencia.FechaHoraIngreso
        };
    }

    public async Task<List<AsistenciaDto>> ObtenerAsistenciasAsync(DateTime? fecha = null, int? idSocio = null)
    {
        var query = _context.Asistencias
            .Include(a => a.Socio)
                .ThenInclude(s => s.Persona)
            .Where(a => a.FechaEliminacion == null)
            .AsQueryable();

        if (fecha.HasValue)
        {
            var fechaInicio = fecha.Value.Date;
            var fechaFin = fechaInicio.AddDays(1);
            query = query.Where(a => a.FechaHoraIngreso >= fechaInicio && a.FechaHoraIngreso < fechaFin);
        }

        if (idSocio.HasValue)
        {
            query = query.Where(a => a.IdSocio == idSocio.Value);
        }

        var asistencias = await query
            .OrderByDescending(a => a.FechaHoraIngreso)
            .ToListAsync();

        return asistencias.Select(a => new AsistenciaDto
        {
            Id = a.Id,
            IdSocio = a.IdSocio,
            NumeroSocio = a.Socio.NumeroSocio,
            NombreSocio = $"{a.Socio.Persona.Nombre} {a.Socio.Persona.Apellido}",
            FechaHoraIngreso = a.FechaHoraIngreso
        }).ToList();
    }
}
