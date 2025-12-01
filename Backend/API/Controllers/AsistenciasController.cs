using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciasController : ControllerBase
{
    private readonly IAsistenciaService _asistenciaService;

    public AsistenciasController(IAsistenciaService asistenciaService)
    {
        _asistenciaService = asistenciaService;
    }

    /// <summary>
    /// Verificar estado de membresía de un socio por DNI
    /// </summary>
    [HttpGet("verificar/{dni}")]
    public async Task<ActionResult<VerificarAsistenciaDto>> VerificarEstadoSocio(string dni)
    {
        try
        {
            var resultado = await _asistenciaService.VerificarEstadoSocioAsync(dni);
            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al verificar estado del socio", error = ex.Message });
        }
    }

    /// <summary>
    /// Registrar asistencia de un socio por DNI
    /// </summary>
    [HttpPost("registrar/{dni}")]
    public async Task<ActionResult<AsistenciaDto>> RegistrarAsistencia(string dni)
    {
        try
        {
            var asistencia = await _asistenciaService.RegistrarAsistenciaAsync(dni);
            return CreatedAtAction(nameof(ObtenerAsistencias), new { idSocio = asistencia.IdSocio }, asistencia);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al registrar asistencia", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener historial de asistencias
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<List<AsistenciaDto>>> ObtenerAsistencias(
        [FromQuery] DateTime? fecha = null,
        [FromQuery] int? idSocio = null)
    {
        try
        {
            var asistencias = await _asistenciaService.ObtenerAsistenciasAsync(fecha, idSocio);
            return Ok(asistencias);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener asistencias", error = ex.Message });
        }
    }
}
