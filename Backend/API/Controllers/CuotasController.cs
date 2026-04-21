using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CuotasController : ControllerBase
{
    private readonly ICuotaService _cuotaService;

    public CuotasController(ICuotaService cuotaService)
    {
        _cuotaService = cuotaService;
    }

    /// <summary>
    /// Obtener cuotas con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CuotaDto>>> ObtenerTodas(
        [FromQuery] int? idMembresia = null,
        [FromQuery] int? idSocio = null,
        [FromQuery] string? estado = null,
        [FromQuery] bool? soloVencidas = null,
        [FromQuery] DateTime? fechaVencimientoDesde = null,
        [FromQuery] DateTime? fechaVencimientoHasta = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var filtros = new FiltrosCuotasDto
            {
                IdMembresia = idMembresia,
                IdSocio = idSocio,
                Estado = estado,
                SoloVencidas = soloVencidas,
                FechaVencimientoDesde = fechaVencimientoDesde,
                FechaVencimientoHasta = fechaVencimientoHasta,
                Page = page,
                PageSize = pageSize
            };

            var cuotas = await _cuotaService.ObtenerCuotasAsync(filtros);
            return Ok(cuotas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener cuotas", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener resumen general de cuotas y morosos
    /// </summary>
    [HttpGet("resumen")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<ResumenCuotasDto>> ObtenerResumen()
    {
        try
        {
            var resumen = await _cuotaService.ObtenerResumenAsync();
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener resumen", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener listado de socios morosos (con cuotas vencidas)
    /// </summary>
    [HttpGet("morosos")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<List<MorosoDto>>> ObtenerMorosos()
    {
        try
        {
            var morosos = await _cuotaService.ObtenerMorososAsync();
            return Ok(morosos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener morosos", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener una cuota por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CuotaDto>> ObtenerPorId(int id)
    {
        try
        {
            var cuota = await _cuotaService.ObtenerCuotaPorIdAsync(id);
            if (cuota == null)
                return NotFound(new { message = "Cuota no encontrada" });

            return Ok(cuota);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener cuota", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener cuotas de una membresía
    /// </summary>
    [HttpGet("membresia/{idMembresia}")]
    public async Task<ActionResult<List<CuotaDto>>> ObtenerPorMembresia(int idMembresia)
    {
        try
        {
            var cuotas = await _cuotaService.ObtenerCuotasPorMembresiaAsync(idMembresia);
            return Ok(cuotas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener cuotas de la membresía", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener cuotas de un socio
    /// </summary>
    [HttpGet("socio/{idSocio}")]
    public async Task<ActionResult<List<CuotaDto>>> ObtenerPorSocio(int idSocio)
    {
        try
        {
            var cuotas = await _cuotaService.ObtenerCuotasPorSocioAsync(idSocio);
            return Ok(cuotas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener cuotas del socio", error = ex.Message });
        }
    }

    /// <summary>
    /// Generar cuotas mensuales para una membresía existente
    /// </summary>
    [HttpPost("generar/{idMembresia}")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<List<CuotaDto>>> GenerarCuotas(int idMembresia)
    {
        try
        {
            var cuotas = await _cuotaService.GenerarCuotasParaMembresiaAsync(idMembresia);
            return CreatedAtAction(nameof(ObtenerPorMembresia), new { idMembresia }, cuotas);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar cuotas", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar estados: marca como vencidas las cuotas pendientes cuya fecha ya pasó
    /// </summary>
    [HttpPost("actualizar-vencidas")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult> ActualizarVencidas()
    {
        try
        {
            var cantidad = await _cuotaService.ActualizarEstadosVencidosAsync();
            return Ok(new { message = $"Se marcaron {cantidad} cuota(s) como vencidas", cantidad });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar cuotas vencidas", error = ex.Message });
        }
    }
}
