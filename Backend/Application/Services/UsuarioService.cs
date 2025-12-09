using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;
using BCrypt.Net;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IUsuarioService
{
    Task<UsuarioDetalleDto?> CrearAsync(CrearUsuarioDto dto, string? currentUserRole = null);
    Task<UsuarioDetalleDto?> ObtenerPorIdAsync(int id);
    Task<List<UsuarioDetalleDto>> ObtenerTodosAsync(string? rol = null, bool? estaActivo = null);
    Task<UsuarioDetalleDto?> ActualizarAsync(int id, ActualizarUsuarioDto dto);
    Task<bool> DesactivarAsync(int id);
}

public class UsuarioService : IUsuarioService
{
    private readonly ClubDbContext _context;

    public UsuarioService(ClubDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioDetalleDto?> CrearAsync(CrearUsuarioDto dto, string? currentUserRole = null)
    {
        // Validar permisos de creación según el rol del usuario actual
        var rolACrear = dto.Rol.ToLower();

        if (!string.IsNullOrEmpty(currentUserRole))
        {
            if (currentUserRole.ToLower() == "admin")
            {
                // Admin solo puede crear recepcionistas
                if (rolACrear != "recepcionista")
                {
                    throw new UnauthorizedAccessException("Los administradores solo pueden crear usuarios con rol 'recepcionista'");
                }
            }
            else if (currentUserRole.ToLower() == "superadmin")
            {
                // Superadmin puede crear admin o recepcionista
                var rolesPermitidos = new[] { "admin", "recepcionista" };
                if (!rolesPermitidos.Contains(rolACrear))
                {
                    throw new ArgumentException("El rol debe ser 'admin' o 'recepcionista'");
                }
            }
        }
        else
        {
            // Si no se proporciona rol actual, solo permitir admin o recepcionista
            var rolesPermitidos = new[] { "admin", "recepcionista" };
            if (!rolesPermitidos.Contains(rolACrear))
            {
                throw new ArgumentException("El rol debe ser 'admin' o 'recepcionista'");
            }
        }

        // Verificar que el nombre de usuario no exista
        var usuarioExiste = await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == dto.NombreUsuario && u.FechaEliminacion == null);

        if (usuarioExiste)
        {
            throw new InvalidOperationException("El nombre de usuario ya existe");
        }

        // Verificar que el email no exista
        var emailExiste = await _context.Personas
            .AnyAsync(p => p.Email == dto.Email && p.FechaEliminacion == null);

        if (emailExiste)
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        // Obtener el rol
        var rol = await _context.Roles
            .FirstOrDefaultAsync(r => r.Nombre.ToLower() == dto.Rol.ToLower());

        if (rol == null)
        {
            throw new InvalidOperationException($"El rol '{dto.Rol}' no existe en el sistema");
        }

        // Crear la persona
        var persona = new Persona
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email.ToLower(),
            Dni = dto.Dni,
            FechaNacimiento = dto.FechaNacimiento,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Personas.Add(persona);
        await _context.SaveChangesAsync();

        // DEBUG: Ver qué contraseña llega
        Console.WriteLine($"=== DEBUG CREAR USUARIO ===");
        Console.WriteLine($"Password recibido: '{dto.Password}'");
        Console.WriteLine($"Password length: {dto.Password?.Length ?? 0}");
        
        var hashGenerado = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        Console.WriteLine($"Hash generado: {hashGenerado}");
        Console.WriteLine($"===========================");

        // Crear el usuario
        var usuario = new Usuario
        {
            IdPersona = persona.Id,
            IdRol = rol.Id,
            NombreUsuario = dto.NombreUsuario,
            ContrasenaHash = hashGenerado,
            EstaActivo = true,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return new UsuarioDetalleDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Nombre = persona.Nombre,
            Apellido = persona.Apellido,
            NombreCompleto = persona.NombreCompleto,
            Email = persona.Email,
            Dni = persona.Dni,
            FechaNacimiento = persona.FechaNacimiento,
            Rol = rol.Nombre,
            EstaActivo = usuario.EstaActivo,
            FechaCreacion = usuario.FechaCreacion
        };
    }

    public async Task<UsuarioDetalleDto?> ObtenerPorIdAsync(int id)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id && u.FechaEliminacion == null);

        if (usuario == null)
        {
            return null;
        }

        return new UsuarioDetalleDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Nombre = usuario.Persona.Nombre,
            Apellido = usuario.Persona.Apellido,
            NombreCompleto = usuario.Persona.NombreCompleto,
            Email = usuario.Persona.Email,
            Dni = usuario.Persona.Dni,
            FechaNacimiento = usuario.Persona.FechaNacimiento,
            Rol = usuario.Rol.Nombre,
            EstaActivo = usuario.EstaActivo,
            FechaCreacion = usuario.FechaCreacion
        };
    }

    public async Task<List<UsuarioDetalleDto>> ObtenerTodosAsync(string? rol = null, bool? estaActivo = null)
    {
        var query = _context.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .Where(u => u.FechaEliminacion == null)
            .AsQueryable();

        if (!string.IsNullOrEmpty(rol))
        {
            query = query.Where(u => u.Rol.Nombre.ToLower() == rol.ToLower());
        }

        if (estaActivo.HasValue)
        {
            query = query.Where(u => u.EstaActivo == estaActivo.Value);
        }

        var usuarios = await query
            .OrderBy(u => u.Persona.Nombre)
            .ThenBy(u => u.Persona.Apellido)
            .ToListAsync();

        return usuarios.Select(u => new UsuarioDetalleDto
        {
            Id = u.Id,
            NombreUsuario = u.NombreUsuario,
            Nombre = u.Persona.Nombre,
            Apellido = u.Persona.Apellido,
            NombreCompleto = u.Persona.NombreCompleto,
            Email = u.Persona.Email,
            Dni = u.Persona.Dni,
            FechaNacimiento = u.Persona.FechaNacimiento,
            Rol = u.Rol.Nombre,
            EstaActivo = u.EstaActivo,
            FechaCreacion = u.FechaCreacion
        }).ToList();
    }

    public async Task<UsuarioDetalleDto?> ActualizarAsync(int id, ActualizarUsuarioDto dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id && u.FechaEliminacion == null);

        if (usuario == null)
        {
            return null;
        }

        // Actualizar persona
        if (!string.IsNullOrEmpty(dto.Nombre))
            usuario.Persona.Nombre = dto.Nombre;

        if (!string.IsNullOrEmpty(dto.Apellido))
            usuario.Persona.Apellido = dto.Apellido;

        if (!string.IsNullOrEmpty(dto.Email))
        {
            var emailExiste = await _context.Personas
                .AnyAsync(p => p.Email == dto.Email && p.Id != usuario.IdPersona && p.FechaEliminacion == null);

            if (emailExiste)
            {
                throw new InvalidOperationException("El email ya está registrado");
            }

            usuario.Persona.Email = dto.Email.ToLower();
        }

        if (dto.Dni != null)
            usuario.Persona.Dni = dto.Dni;

        if (dto.FechaNacimiento.HasValue)
            usuario.Persona.FechaNacimiento = dto.FechaNacimiento;

        if (dto.EstaActivo.HasValue)
            usuario.EstaActivo = dto.EstaActivo.Value;

        usuario.Persona.FechaActualizacion = DateTime.Now;
        usuario.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return new UsuarioDetalleDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Nombre = usuario.Persona.Nombre,
            Apellido = usuario.Persona.Apellido,
            NombreCompleto = usuario.Persona.NombreCompleto,
            Email = usuario.Persona.Email,
            Dni = usuario.Persona.Dni,
            FechaNacimiento = usuario.Persona.FechaNacimiento,
            Rol = usuario.Rol.Nombre,
            EstaActivo = usuario.EstaActivo,
            FechaCreacion = usuario.FechaCreacion
        };
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id && u.FechaEliminacion == null);

        if (usuario == null)
        {
            return false;
        }

        usuario.FechaEliminacion = DateTime.Now;
        usuario.FechaActualizacion = DateTime.Now;
        usuario.EstaActivo = false;

        await _context.SaveChangesAsync();

        return true;
    }
}
