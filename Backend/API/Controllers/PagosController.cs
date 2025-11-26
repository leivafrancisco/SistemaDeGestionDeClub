using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;
using System.Security.Claims;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;

    public PagosController(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    /// <summary>
    /// Obtener todos los pagos con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PagoDto>>> ObtenerTodos(
        [FromQuery] int? idMembresia = null,
        [FromQuery] int? idSocio = null,
        [FromQuery] int? idMetodoPago = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var filtros = new FiltrosPagosDto
            {
                IdMembresia = idMembresia,
                IdSocio = idSocio,
                IdMetodoPago = idMetodoPago,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Page = page,
                PageSize = pageSize
            };

            var pagos = await _pagoService.ObtenerTodosAsync(filtros);
            return Ok(pagos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener pagos", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener un pago por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PagoDto>> ObtenerPorId(int id)
    {
        try
        {
            var pago = await _pagoService.ObtenerPorIdAsync(id);

            if (pago == null)
            {
                return NotFound(new { message = "Pago no encontrado" });
            }

            return Ok(pago);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener pago", error = ex.Message });
        }
    }

    /// <summary>
    /// Registrar un nuevo pago y generar comprobante
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<ComprobantePagoDto>> RegistrarPago([FromBody] RegistrarPagoDto dto)
    {
        try
        {
            // Obtener ID del usuario autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "ID de usuario inválido" });
            }

            var comprobante = await _pagoService.RegistrarPagoAsync(dto, userId);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = comprobante.IdPago }, comprobante);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al registrar pago", error = ex.Message });
        }
    }

    /// <summary>
    /// Generar comprobante de un pago existente
    /// </summary>
    [HttpGet("{id}/comprobante")]
    public async Task<ActionResult<ComprobantePagoDto>> GenerarComprobante(int id)
    {
        try
        {
            var comprobante = await _pagoService.GenerarComprobanteAsync(id);
            return Ok(comprobante);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al generar comprobante", error = ex.Message });
        }
    }

    /// <summary>
    /// Anular un pago (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult> AnularPago(int id)
    {
        try
        {
            var resultado = await _pagoService.AnularPagoAsync(id);

            if (!resultado)
            {
                return NotFound(new { message = "Pago no encontrado" });
            }

            return Ok(new { message = "Pago anulado exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al anular pago", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener métodos de pago disponibles
    /// </summary>
    [HttpGet("metodos")]
    public async Task<ActionResult> ObtenerMetodosPago()
    {
        try
        {
            var metodos = await _pagoService.ObtenerMetodosPagoAsync();
            return Ok(metodos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener métodos de pago", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener estadísticas completas de pagos
    /// </summary>
    [HttpGet("estadisticas")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<EstadisticasPagosDto>> ObtenerEstadisticas(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        try
        {
            var estadisticas = await _pagoService.ObtenerEstadisticasAsync(fechaDesde, fechaHasta);
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener estadísticas", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener total recaudado en un rango de fechas
    /// </summary>
    [HttpGet("estadisticas/recaudacion")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult> ObtenerRecaudacion(
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        try
        {
            var total = await _pagoService.ObtenerTotalRecaudadoAsync(fechaDesde, fechaHasta);
            return Ok(new { total, fechaDesde, fechaHasta });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener recaudación", error = ex.Message });
        }
    }
}
