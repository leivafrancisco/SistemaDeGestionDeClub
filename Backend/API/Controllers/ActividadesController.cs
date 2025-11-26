using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActividadesController : ControllerBase
{
    private readonly IActividadService _actividadService;

    public ActividadesController(IActividadService actividadService)
    {
        _actividadService = actividadService;
    }

    /// <summary>
    /// Obtener todas las actividades
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ActividadDto>>> ObtenerTodas()
    {
        try
        {
            var actividades = await _actividadService.ObtenerTodasAsync();
            return Ok(actividades);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener actividades", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener una actividad por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ActividadDto>> ObtenerPorId(int id)
    {
        try
        {
            var actividad = await _actividadService.ObtenerPorIdAsync(id);

            if (actividad == null)
            {
                return NotFound(new { message = "Actividad no encontrada" });
            }

            return Ok(actividad);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener actividad", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear una nueva actividad (solo superadmin y admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<ActividadDto>> Crear([FromBody] CrearActividadDto dto)
    {
        try
        {
            var actividad = await _actividadService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = actividad.Id }, actividad);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear actividad", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar una actividad existente (solo superadmin y admin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<ActividadDto>> Actualizar(int id, [FromBody] ActualizarActividadDto dto)
    {
        try
        {
            var actividad = await _actividadService.ActualizarAsync(id, dto);
            return Ok(actividad);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar actividad", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar (soft delete) una actividad (solo superadmin y admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult> Eliminar(int id)
    {
        try
        {
            var resultado = await _actividadService.EliminarAsync(id);

            if (!resultado)
            {
                return NotFound(new { message = "Actividad no encontrada" });
            }

            return Ok(new { message = "Actividad eliminada exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar actividad", error = ex.Message });
        }
    }
}
