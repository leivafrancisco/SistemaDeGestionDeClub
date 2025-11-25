using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Crear un nuevo usuario (admin o recepcionista) - Solo superadmin
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "superadmin")]
    public async Task<ActionResult<UsuarioDetalleDto>> Crear([FromBody] CrearUsuarioDto dto)
    {
        try
        {
            var usuario = await _usuarioService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario!.Id }, usuario);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear usuario", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener todos los usuarios - Solo superadmin y admin
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<List<UsuarioDetalleDto>>> ObtenerTodos(
        [FromQuery] string? rol = null,
        [FromQuery] bool? estaActivo = null)
    {
        try
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync(rol, estaActivo);
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener usuarios", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener usuario por ID - Solo superadmin y admin
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<UsuarioDetalleDto>> ObtenerPorId(int id)
    {
        try
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);

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

    /// <summary>
    /// Actualizar usuario - Solo superadmin
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "superadmin")]
    public async Task<ActionResult<UsuarioDetalleDto>> Actualizar(int id, [FromBody] ActualizarUsuarioDto dto)
    {
        try
        {
            var usuario = await _usuarioService.ActualizarAsync(id, dto);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar usuario", error = ex.Message });
        }
    }

    /// <summary>
    /// Desactivar usuario (soft delete) - Solo superadmin
    /// </summary>
    [HttpPut("{id}/desactivar")]
    [Authorize(Roles = "superadmin")]
    public async Task<ActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _usuarioService.DesactivarAsync(id);

            if (!resultado)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return Ok(new { message = "Usuario desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al desactivar usuario", error = ex.Message });
        }
    }
}
