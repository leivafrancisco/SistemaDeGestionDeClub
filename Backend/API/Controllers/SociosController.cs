using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SociosController : ControllerBase
{
    private readonly ISocioService _socioService;
    
    public SociosController(ISocioService socioService)
    {
        _socioService = socioService;
    }
    
    /// <summary>
    /// Obtener todos los socios con paginación y filtros
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SocioDto>>> ObtenerTodos(
        [FromQuery] string? search = null,
        [FromQuery] bool? estaActivo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var socios = await _socioService.ObtenerTodosAsync(search, estaActivo, page, pageSize);
            return Ok(socios);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener socios", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtener un socio por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SocioDto>> ObtenerPorId(int id)
    {
        try
        {
            var socio = await _socioService.ObtenerPorIdAsync(id);
            
            if (socio == null)
            {
                return NotFound(new { message = "Socio no encontrado" });
            }
            
            return Ok(socio);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener socio", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtener un socio por número de socio
    /// </summary>
    [HttpGet("numero/{numeroSocio}")]
    public async Task<ActionResult<SocioDto>> ObtenerPorNumeroSocio(string numeroSocio)
    {
        try
        {
            var socio = await _socioService.ObtenerPorNumeroSocioAsync(numeroSocio);
            
            if (socio == null)
            {
                return NotFound(new { message = "Socio no encontrado" });
            }
            
            return Ok(socio);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener socio", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Crear un nuevo socio
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<SocioDto>> Crear([FromBody] CrearSocioDto dto)
    {
        try
        {
            var socio = await _socioService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = socio.Id }, socio);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear socio", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Actualizar un socio existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<SocioDto>> Actualizar(int id, [FromBody] ActualizarSocioDto dto)
    {
        try
        {
            var socio = await _socioService.ActualizarAsync(id, dto);
            return Ok(socio);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar socio", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Desactivar (dar de baja) un socio
    /// </summary>
    [HttpPut("{id}/desactivar")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _socioService.DesactivarAsync(id);
            
            if (!resultado)
            {
                return NotFound(new { message = "Socio no encontrado" });
            }
            
            return Ok(new { message = "Socio desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al desactivar socio", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Obtener cantidad total de socios activos
    /// </summary>
    [HttpGet("estadisticas/total")]
    public async Task<ActionResult<int>> ObtenerTotal()
    {
        try
        {
            var total = await _socioService.ContarTotalAsync();
            return Ok(new { total });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al contar socios", error = ex.Message });
        }
    }
}
