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
    /// Obtener historial de asistencias con filtros opcionales
    /// </summary>
    /// <param name="fecha">Filtrar por fecha específica (formato: YYYY-MM-DD)</param>
    /// <param name="idSocio">Filtrar por ID de socio</param>
    /// <returns>Lista de asistencias ordenadas por fecha descendente</returns>
    [HttpGet]
    [Authorize(Roles = "superadmin,admin,recepcionista")]
    public async Task<ActionResult<List<AsistenciaDto>>> ObtenerAsistencias(
        [FromQuery] string? fecha = null,
        [FromQuery] int? idSocio = null)
    {
        try
        {
            // Validar y parsear la fecha si se proporciona
            DateTime? fechaParsed = null;
            if (!string.IsNullOrEmpty(fecha))
            {
                if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd", 
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, 
                    out var fechaResult))
                {
                    return BadRequest(new { message = "Formato de fecha inválido. Use el formato YYYY-MM-DD (ej: 2025-12-09)" });
                }
                fechaParsed = fechaResult;
            }

            var asistencias = await _asistenciaService.ObtenerAsistenciasAsync(fechaParsed, idSocio);
            return Ok(asistencias);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener asistencias", error = ex.Message });
        }
    }
}
