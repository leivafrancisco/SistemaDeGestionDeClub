using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface ISocioService
{
    Task<List<SocioDto>> ObtenerTodosSociosAsync(string? search = null, bool? estaActivo = null, int page = 1, int pageSize = 20);
    Task<SocioDto?> ObtenerSocioPorIdAsync(int id);
    Task<SocioDto?> ObtenerSocioPorNumeroAsync(string numeroSocio);
    Task<SocioDto> CrearSocioAsync(CrearSocioDto dto);
    Task<SocioDto> ActualizarSocioAsync(int id, ActualizarSocioDto dto);
    Task<bool> DesactivarSocioAsync(int id);
    Task<int> ContarTotalSociosAsync();

    // Invoca sp_ResumenSocio (consulta)
    Task<ResumenSocioDto?> ObtenerResumenAsync(int idSocio);

    // Invoca sp_CambiarEstadoSocio (actualización)
    Task<SocioDto?> CambiarEstadoAsync(int idSocio, bool estaActivo, int? idUsuarioProcesa);
}

public class SocioService : ISocioService
{
    private readonly ClubDbContext _context;
    
    public SocioService(ClubDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<SocioDto>> ObtenerTodosSociosAsync(string? search = null, bool? estaActivo = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Socios
            .Include(s => s.Persona)
            .Where(s => s.FechaEliminacion == null)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s => 
                s.NumeroSocio.Contains(search) ||
                s.Persona.Nombre.Contains(search) ||
                s.Persona.Apellido.Contains(search) ||
                s.Persona.Email.Contains(search) ||
                (s.Persona.Dni != null && s.Persona.Dni.Contains(search))
            );
        }
        
        if (estaActivo.HasValue)
        {
            query = query.Where(s => s.EstaActivo == estaActivo.Value);
        }
        
        var socios = await query
            .OrderByDescending(s => s.FechaAlta)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SocioDto
            {
                Id = s.Id,
                NumeroSocio = s.NumeroSocio,
                Nombre = s.Persona.Nombre,
                Apellido = s.Persona.Apellido,
                Email = s.Persona.Email,
                Dni = s.Persona.Dni,
                FechaNacimiento = s.Persona.FechaNacimiento,
                EstaActivo = s.EstaActivo,
                FechaAlta = s.FechaAlta,
                FechaBaja = s.FechaBaja
            })
            .ToListAsync();
        
        return socios;
    }
    
    public async Task<SocioDto?> ObtenerSocioPorIdAsync(int id)
    {
        var socio = await _context.Socios
            .Include(s => s.Persona)
            .Where(s => s.Id == id && s.FechaEliminacion == null)
            .Select(s => new SocioDto
            {
                Id = s.Id,
                NumeroSocio = s.NumeroSocio,
                Nombre = s.Persona.Nombre,
                Apellido = s.Persona.Apellido,
                Email = s.Persona.Email,
                Dni = s.Persona.Dni,
                FechaNacimiento = s.Persona.FechaNacimiento,
                EstaActivo = s.EstaActivo,
                FechaAlta = s.FechaAlta,
                FechaBaja = s.FechaBaja
            })
            .FirstOrDefaultAsync();
        
        return socio;
    }
    
    public async Task<SocioDto?> ObtenerSocioPorNumeroAsync(string numeroSocio)
    {
        var socio = await _context.Socios
            .Include(s => s.Persona)
            .Where(s => s.NumeroSocio == numeroSocio && s.FechaEliminacion == null)
            .Select(s => new SocioDto
            {
                Id = s.Id,
                NumeroSocio = s.NumeroSocio,
                Nombre = s.Persona.Nombre,
                Apellido = s.Persona.Apellido,
                Email = s.Persona.Email,
                Dni = s.Persona.Dni,
                FechaNacimiento = s.Persona.FechaNacimiento,
                EstaActivo = s.EstaActivo,
                FechaAlta = s.FechaAlta,
                FechaBaja = s.FechaBaja
            })
            .FirstOrDefaultAsync();
        
        return socio;
    }
    
    public async Task<SocioDto> CrearSocioAsync(CrearSocioDto dto)
    {
        // Validar que el email no exista
        if (await _context.Personas.AnyAsync(p => p.Email == dto.Email && p.FechaEliminacion == null))
        {
            throw new InvalidOperationException("Ya existe un socio con este email");
        }

        // Validar que el DNI no exista (si se proporciona)
        if (!string.IsNullOrEmpty(dto.Dni))
        {
            if (await _context.Personas.AnyAsync(p => p.Dni == dto.Dni && p.FechaEliminacion == null))
            {
                throw new InvalidOperationException("Ya existe un socio con este DNI");
            }
        }

        // Generar número de socio automáticamente
        var ultimoNumero = await _context.Socios
            .Where(s => s.NumeroSocio.StartsWith("SOC-"))
            .Select(s => s.NumeroSocio)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync();

        int siguienteNumero = 1;
        if (!string.IsNullOrEmpty(ultimoNumero) && ultimoNumero.Length > 4)
        {
            // Formato: SOC-XXXX
            if (int.TryParse(ultimoNumero.Substring(4), out int numero))
            {
                siguienteNumero = numero + 1;
            }
        }

        var numeroSocio = $"SOC-{siguienteNumero:D4}";
        
        // Crear persona
        var persona = new Persona
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            Dni = dto.Dni,
            FechaNacimiento = dto.FechaNacimiento,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        Console.WriteLine($"[DEBUG] Creando Persona - Nombre: {persona.Nombre}, DNI: {persona.Dni ?? "NULL"}");
        _context.Personas.Add(persona);
        await _context.SaveChangesAsync();
        Console.WriteLine($"[DEBUG] Persona creada con ID: {persona.Id}, DNI guardado: {persona.Dni ?? "NULL"}");
        
        // Crear socio
        var socio = new Socio
        {
            IdPersona = persona.Id,
            NumeroSocio = numeroSocio,
            EstaActivo = true,
            FechaAlta = DateTime.Now,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };
        
        _context.Socios.Add(socio);
        await _context.SaveChangesAsync();
        
        return new SocioDto
        {
            Id = socio.Id,
            NumeroSocio = socio.NumeroSocio,
            Nombre = persona.Nombre,
            Apellido = persona.Apellido,
            Email = persona.Email,
            Dni = persona.Dni,
            FechaNacimiento = persona.FechaNacimiento,
            EstaActivo = socio.EstaActivo,
            FechaAlta = socio.FechaAlta,
            FechaBaja = socio.FechaBaja
        };
    }
    
    public async Task<SocioDto> ActualizarSocioAsync(int id, ActualizarSocioDto dto)
    {
        var socio = await _context.Socios
            .Include(s => s.Persona)
            .FirstOrDefaultAsync(s => s.Id == id && s.FechaEliminacion == null);
        
        if (socio == null)
        {
            throw new InvalidOperationException("Socio no encontrado");
        }
        
        // Validar email único
        if (await _context.Personas.AnyAsync(p => p.Email == dto.Email && p.Id != socio.IdPersona && p.FechaEliminacion == null))
        {
            throw new InvalidOperationException("Ya existe una persona con este email");
        }

        // Validar que el DNI no exista (si se proporciona)
        if (!string.IsNullOrEmpty(dto.Dni))
        {
            if (await _context.Personas.AnyAsync(p => p.Dni == dto.Dni && p.Id != socio.IdPersona && p.FechaEliminacion == null))
            {
                throw new InvalidOperationException("Ya existe una persona con este DNI");
            }
        }

        // Actualizar persona
        socio.Persona.Nombre = dto.Nombre;
        socio.Persona.Apellido = dto.Apellido;
        socio.Persona.Email = dto.Email;
        socio.Persona.Dni = dto.Dni;
        socio.Persona.FechaNacimiento = dto.FechaNacimiento;
        
        await _context.SaveChangesAsync();
        
        return new SocioDto
        {
            Id = socio.Id,
            NumeroSocio = socio.NumeroSocio,
            Nombre = socio.Persona.Nombre,
            Apellido = socio.Persona.Apellido,
            Email = socio.Persona.Email,
            Dni = socio.Persona.Dni,
            FechaNacimiento = socio.Persona.FechaNacimiento,
            EstaActivo = socio.EstaActivo,
            FechaAlta = socio.FechaAlta,
            FechaBaja = socio.FechaBaja
        };
    }
    
    public async Task<bool> DesactivarSocioAsync(int id)
    {
        var existe = await _context.Socios.AnyAsync(s => s.Id == id && s.FechaEliminacion == null);
        if (!existe)
        {
            return false;
        }
        var socio = await CambiarEstadoAsync(id, false, null);
        return socio != null;
    }
    
    public async Task<int> ContarTotalSociosAsync()
    {
        return await _context.Socios
            .Where(s => s.FechaEliminacion == null && s.EstaActivo)
            .CountAsync();
    }

    /// <summary>
    /// Llama a sp_ResumenSocio y devuelve el resumen financiero del socio.
    /// </summary>
    public async Task<ResumenSocioDto?> ObtenerResumenAsync(int idSocio)
    {
        var connection = _context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText  = "sp_ResumenSocio";
        command.CommandType  = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@id_socio", idSocio));

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new ResumenSocioDto
        {
            IdSocio              = reader.GetInt32(reader.GetOrdinal("id_socio")),
            NombreCompleto       = reader.GetString(reader.GetOrdinal("nombre_completo")),
            NumeroSocio          = reader.GetString(reader.GetOrdinal("numero_socio")),
            Email                = reader.GetString(reader.GetOrdinal("email")),
            Dni                  = reader.IsDBNull(reader.GetOrdinal("dni"))
                                       ? null
                                       : reader.GetString(reader.GetOrdinal("dni")),
            EstaActivo           = reader.GetBoolean(reader.GetOrdinal("esta_activo")),
            FechaAlta            = reader.GetDateTime(reader.GetOrdinal("fecha_alta")),
            FechaBaja            = reader.IsDBNull(reader.GetOrdinal("fecha_baja"))
                                       ? null
                                       : reader.GetDateTime(reader.GetOrdinal("fecha_baja")),
            TotalMembresias      = reader.GetInt32(reader.GetOrdinal("total_membresias")),
            TotalCargado         = reader.GetDecimal(reader.GetOrdinal("total_cargado")),
            TotalPagado          = reader.GetDecimal(reader.GetOrdinal("total_pagado")),
            SaldoPendiente       = reader.GetDecimal(reader.GetOrdinal("saldo_pendiente")),
            UltimaFechaPago      = reader.IsDBNull(reader.GetOrdinal("ultima_fecha_pago"))
                                       ? null
                                       : reader.GetDateTime(reader.GetOrdinal("ultima_fecha_pago")),
            AsistenciasEsteMes   = reader.GetInt32(reader.GetOrdinal("asistencias_este_mes")),
        };
    }

    /// <summary>
    /// Llama a sp_CambiarEstadoSocio y devuelve el socio actualizado.
    /// </summary>
    public async Task<SocioDto?> CambiarEstadoAsync(int idSocio, bool estaActivo, int? idUsuarioProcesa)
    {
        var connection = _context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "sp_CambiarEstadoSocio";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(new SqlParameter("@id_socio",           idSocio));
        command.Parameters.Add(new SqlParameter("@esta_activo",        estaActivo ? 1 : 0));
        command.Parameters.Add(new SqlParameter("@id_usuario_procesa",
            idUsuarioProcesa.HasValue ? (object)idUsuarioProcesa.Value : DBNull.Value));

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new SocioDto
        {
            Id           = reader.GetInt32(reader.GetOrdinal("id")),
            NumeroSocio  = reader.GetString(reader.GetOrdinal("numero_socio")),
            Nombre       = reader.GetString(reader.GetOrdinal("nombre")),
            Apellido     = reader.GetString(reader.GetOrdinal("apellido")),
            Email        = reader.GetString(reader.GetOrdinal("email")),
            Dni          = reader.IsDBNull(reader.GetOrdinal("dni"))
                               ? null
                               : reader.GetString(reader.GetOrdinal("dni")),
            EstaActivo   = reader.GetBoolean(reader.GetOrdinal("esta_activo")),
            FechaAlta    = reader.GetDateTime(reader.GetOrdinal("fecha_alta")),
            FechaBaja    = reader.IsDBNull(reader.GetOrdinal("fecha_baja"))
                               ? null
                               : reader.GetDateTime(reader.GetOrdinal("fecha_baja")),
        };
    }
}
