using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface ISocioService
{
    Task<List<SocioDto>> ObtenerTodosAsync(string? search = null, bool? estaActivo = null, int page = 1, int pageSize = 20);
    Task<SocioDto?> ObtenerPorIdAsync(int id);
    Task<SocioDto?> ObtenerPorNumeroSocioAsync(string numeroSocio);
    Task<SocioDto> CrearAsync(CrearSocioDto dto);
    Task<SocioDto> ActualizarAsync(int id, ActualizarSocioDto dto);
    Task<bool> DesactivarAsync(int id);
    Task<int> ContarTotalAsync();
}

public class SocioService : ISocioService
{
    private readonly ClubDbContext _context;
    
    public SocioService(ClubDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<SocioDto>> ObtenerTodosAsync(string? search = null, bool? estaActivo = null, int page = 1, int pageSize = 20)
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
    
    public async Task<SocioDto?> ObtenerPorIdAsync(int id)
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
    
    public async Task<SocioDto?> ObtenerPorNumeroSocioAsync(string numeroSocio)
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
    
    public async Task<SocioDto> CrearAsync(CrearSocioDto dto)
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
    
    public async Task<SocioDto> ActualizarAsync(int id, ActualizarSocioDto dto)
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
    
    public async Task<bool> DesactivarAsync(int id)
    {
        var socio = await _context.Socios.FindAsync(id);
        
        if (socio == null || socio.FechaEliminacion != null)
        {
            return false;
        }
        
        socio.EstaActivo = false;
        socio.FechaBaja = DateTime.Now;
        
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<int> ContarTotalAsync()
    {
        return await _context.Socios
            .Where(s => s.FechaEliminacion == null && s.EstaActivo)
            .CountAsync();
    }
}
