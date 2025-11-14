using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    /// <summary>
    /// Login de usuario
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var resultado = await _authService.LoginAsync(dto);
            
            if (resultado == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }
            
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al iniciar sesión", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtener información del usuario actual
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> ObtenerUsuarioActual()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }
            
            var usuario = await _authService.ObtenerUsuarioActualAsync(userId);
            
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }
            
            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener usuario", error = ex.Message });
        }
    }
}
