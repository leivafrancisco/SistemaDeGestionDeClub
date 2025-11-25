using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembresiasController : ControllerBase
{
    private readonly IMembresiaService _membresiaService;

    public MembresiasController(IMembresiaService membresiaService)
    {
        _membresiaService = membresiaService;
    }

    /// <summary>
    /// Obtener todas las membresías con filtros opcionales
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MembresiaDto>>> ObtenerTodos(
        [FromQuery] int? idSocio = null,
        [FromQuery] short? periodoAnio = null,
        [FromQuery] byte? periodoMes = null,
        [FromQuery] bool? soloImpagas = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var filtros = new FiltrosMembresiasDto
            {
                IdSocio = idSocio,
                PeriodoAnio = periodoAnio,
                PeriodoMes = periodoMes,
                SoloImpagas = soloImpagas,
                Page = page,
                PageSize = pageSize
            };

            var membresias = await _membresiaService.ObtenerTodosAsync(filtros);
            return Ok(membresias);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener membresías", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener una membresía por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MembresiaDto>> ObtenerPorId(int id)
    {
        try
        {
            var membresia = await _membresiaService.ObtenerPorIdAsync(id);

            if (membresia == null)
            {
                return NotFound(new { message = "Membresía no encontrada" });
            }

            return Ok(membresia);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener membresía", error = ex.Message });
        }
    }

    /// <summary>
    /// Crear una nueva membresía
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<MembresiaDto>> Crear([FromBody] CrearMembresiaDto dto)
    {
        try
        {
            var membresia = await _membresiaService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = membresia.Id }, membresia);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear membresía", error = ex.Message });
        }
    }

    /// <summary>
    /// Actualizar una membresía existente (solo actividades)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult<MembresiaDto>> Actualizar(int id, [FromBody] ActualizarMembresiaDto dto)
    {
        try
        {
            var membresia = await _membresiaService.ActualizarAsync(id, dto);
            return Ok(membresia);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar membresía", error = ex.Message });
        }
    }

    /// <summary>
    /// Eliminar una membresía (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "superadmin,admin")]
    public async Task<ActionResult> Eliminar(int id)
    {
        try
        {
            var resultado = await _membresiaService.EliminarAsync(id);

            if (!resultado)
            {
                return NotFound(new { message = "Membresía no encontrada" });
            }

            return Ok(new { message = "Membresía eliminada exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar membresía", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener cantidad total de membresías
    /// </summary>
    [HttpGet("estadisticas/total")]
    public async Task<ActionResult<int>> ObtenerTotal()
    {
        try
        {
            var total = await _membresiaService.ContarTotalAsync();
            return Ok(new { total });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al contar membresías", error = ex.Message });
        }
    }

    /// <summary>
    /// Asignar actividad a membresía - Recepcionista da de alta un socio en una actividad
    /// </summary>
    [HttpPost("asignar-actividad")]
    [Authorize(Roles = "superadmin,admin,recepcionista")]
    public async Task<ActionResult<MembresiaDto>> AsignarActividad([FromBody] AsignarActividadDto dto)
    {
        try
        {
            var membresia = await _membresiaService.AsignarActividadAsync(dto);
            return Ok(membresia);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al asignar actividad", error = ex.Message });
        }
    }

    /// <summary>
    /// Remover actividad de membresía - Solo si no hay pagos registrados
    /// </summary>
    [HttpPost("remover-actividad")]
    [Authorize(Roles = "superadmin,admin,recepcionista")]
    public async Task<ActionResult<MembresiaDto>> RemoverActividad([FromBody] RemoverActividadDto dto)
    {
        try
        {
            var membresia = await _membresiaService.RemoverActividadAsync(dto);
            return Ok(membresia);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al remover actividad", error = ex.Message });
        }
    }
}
