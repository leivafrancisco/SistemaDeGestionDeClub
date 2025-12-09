using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;
using BCrypt.Net;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task<UsuarioDto?> ObtenerUsuarioActualAsync(int userId);
    Task<UsuarioDto?> ActualizarPerfilAsync(int userId, ActualizarPerfilDto dto);
}

public class AuthService : IAuthService
{
    private readonly ClubDbContext _context;
    private readonly IConfiguration _configuration;
    
    public AuthService(ClubDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        // Debug: Primero buscar sin filtros para ver qué hay
        var usuarioDebug = await _context.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == dto.NombreUsuario);
        
        if (usuarioDebug != null)
        {
            Console.WriteLine($"=== DEBUG LOGIN ===");
            Console.WriteLine($"Usuario encontrado: {usuarioDebug.NombreUsuario}");
            Console.WriteLine($"EstaActivo: {usuarioDebug.EstaActivo}");
            Console.WriteLine($"FechaEliminacion: {usuarioDebug.FechaEliminacion}");
            Console.WriteLine($"Rol: {usuarioDebug.Rol?.Nombre ?? "NULL"}");
            Console.WriteLine($"IdRol: {usuarioDebug.IdRol}");
            Console.WriteLine($"Persona: {usuarioDebug.Persona?.NombreCompleto ?? "NULL"}");
            Console.WriteLine($"===================");
        }
        else
        {
            Console.WriteLine($"=== DEBUG: Usuario '{dto.NombreUsuario}' NO encontrado en BD ===");
        }
        
        var usuario = await _context.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => 
                u.NombreUsuario == dto.NombreUsuario && 
                u.EstaActivo && 
                u.FechaEliminacion == null);
        
        if (usuario == null)
        {
            Console.WriteLine("=== DEBUG: Usuario null después de filtros (EstaActivo o FechaEliminacion) ===");
            return null;
        }
        
        // Validar contraseña con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.ContrasenaHash))
        {
            Console.WriteLine("=== DEBUG: Contraseña incorrecta (BCrypt.Verify falló) ===");
            return null;
        }
        
        // Generar token JWT
        var token = GenerarTokenJwt(usuario);
        
        return new LoginResponseDto
        {
            Token = token,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.Persona.NombreCompleto,
                Email = usuario.Persona.Email,
                Rol = usuario.Rol.Nombre,
                EstaActivo = usuario.EstaActivo
            }
        };
    }
    
    public async Task<UsuarioDto?> ObtenerUsuarioActualAsync(int userId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId && u.FechaEliminacion == null);

        if (usuario == null)
        {
            return null;
        }

        return new UsuarioDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.Persona.NombreCompleto,
            Email = usuario.Persona.Email,
            Dni = usuario.Persona.Dni,
            FechaNacimiento = usuario.Persona.FechaNacimiento,
            Rol = usuario.Rol.Nombre,
            EstaActivo = usuario.EstaActivo
        };
    }
    
    public async Task<UsuarioDto?> ActualizarPerfilAsync(int userId, ActualizarPerfilDto dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == userId && u.FechaEliminacion == null);

        if (usuario == null)
        {
            return null;
        }

        // Validar y actualizar nombre
        if (!string.IsNullOrEmpty(dto.Nombre))
        {
            if (dto.Nombre.Length > 50)
            {
                throw new InvalidOperationException("El nombre no puede tener más de 50 caracteres");
            }
            usuario.Persona.Nombre = dto.Nombre.Trim();
        }

        // Validar y actualizar apellido
        if (!string.IsNullOrEmpty(dto.Apellido))
        {
            if (dto.Apellido.Length > 50)
            {
                throw new InvalidOperationException("El apellido no puede tener más de 50 caracteres");
            }
            usuario.Persona.Apellido = dto.Apellido.Trim();
        }

        // Validar y actualizar email
        if (!string.IsNullOrEmpty(dto.Email))
        {
            var emailNormalizado = dto.Email.ToLower();

            // Verificar que el email no esté en uso por otro usuario
            var emailExiste = await _context.Personas
                .AnyAsync(p => p.Email == emailNormalizado && p.Id != usuario.IdPersona && p.FechaEliminacion == null);

            if (emailExiste)
            {
                throw new InvalidOperationException("El email ya está en uso por otro usuario");
            }

            usuario.Persona.Email = emailNormalizado;
        }

        // Validar y actualizar DNI
        if (!string.IsNullOrEmpty(dto.Dni))
        {
            // Validar que solo contenga números
            if (!dto.Dni.All(char.IsDigit))
            {
                throw new InvalidOperationException("El DNI solo puede contener números");
            }

            // Validar longitud (7-8 dígitos)
            if (dto.Dni.Length < 7 || dto.Dni.Length > 8)
            {
                throw new InvalidOperationException("El DNI debe tener entre 7 y 8 dígitos");
            }

            usuario.Persona.Dni = dto.Dni;
        }

        // Validar y actualizar fecha de nacimiento
        if (dto.FechaNacimiento.HasValue)
        {
            // Validar que la fecha sea anterior a hoy
            if (dto.FechaNacimiento.Value.Date >= DateTime.Now.Date)
            {
                throw new InvalidOperationException("La fecha de nacimiento debe ser anterior a la fecha actual");
            }

            usuario.Persona.FechaNacimiento = dto.FechaNacimiento.Value;
        }

        // Validar y actualizar contraseña
        if (!string.IsNullOrEmpty(dto.Password))
        {
            // Si se quiere cambiar la contraseña, passwordActual es obligatorio
            if (string.IsNullOrEmpty(dto.PasswordActual))
            {
                throw new InvalidOperationException("La contraseña actual es requerida para cambiar la contraseña");
            }

            // Validar que la contraseña actual sea correcta
            if (!BCrypt.Net.BCrypt.Verify(dto.PasswordActual, usuario.ContrasenaHash))
            {
                throw new InvalidOperationException("La contraseña actual es incorrecta");
            }

            // Validar longitud mínima de la nueva contraseña
            if (dto.Password.Length < 6)
            {
                throw new InvalidOperationException("La nueva contraseña debe tener al menos 6 caracteres");
            }

            // Hashear y actualizar la nueva contraseña
            usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        // Actualizar fechas
        usuario.Persona.FechaActualizacion = DateTime.Now;
        usuario.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return new UsuarioDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.Persona.NombreCompleto,
            Email = usuario.Persona.Email,
            Dni = usuario.Persona.Dni,
            FechaNacimiento = usuario.Persona.FechaNacimiento,
            Rol = usuario.Rol.Nombre,
            EstaActivo = usuario.EstaActivo
        };
    }

    private string GenerarTokenJwt(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, usuario.Persona.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
            new Claim("NombreCompleto", usuario.Persona.NombreCompleto)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "TuClaveSecretaSuperSeguraDeAlMenos32Caracteres!"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24), // Token expira en 24 horas
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
