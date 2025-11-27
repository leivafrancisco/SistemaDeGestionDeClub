using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task<UsuarioDto?> ObtenerUsuarioActualAsync(int userId);
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
        var usuario = await _context.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => 
                u.NombreUsuario == dto.NombreUsuario && 
                u.EstaActivo && 
                u.FechaEliminacion == null);
        
        if (usuario == null)
        {
            return null;
        }
        
        // Validar contraseña (en producción usar BCrypt)
        if (!VerificarContrasena(dto.Password, usuario.ContrasenaHash))
        {
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
    
    private bool VerificarContrasena(string contrasena, string hash)
    {
        // En producción, usar BCrypt.Net-Next:
        // return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        
        // Por ahora, comparación simple (SOLO PARA DESARROLLO)
        return contrasena == hash;
    }
}
