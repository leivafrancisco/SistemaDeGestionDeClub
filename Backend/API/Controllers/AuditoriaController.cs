using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "superadmin")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    /// <summary>
    /// Obtener auditorías con filtros y paginación (Solo Superadmin)
    /// </summary>
    /// <param name="tabla">Filtrar por tabla</param>
    /// <param name="operacion">Filtrar por operación (INSERT/UPDATE/DELETE)</param>
    /// <param name="idUsuario">Filtrar por ID de usuario</param>
    /// <param name="fechaDesde">Filtrar desde fecha</param>
    /// <param name="fechaHasta">Filtrar hasta fecha</param>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Lista paginada de auditorías</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditoriaDto>>> ObtenerAuditorias(
        [FromQuery] string? tabla,
        [FromQuery] string? operacion,
        [FromQuery] int? idUsuario,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var filtros = new AuditoriaFiltrosDto
            {
                Tabla = tabla,
                Operacion = operacion,
                IdUsuario = idUsuario,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                Page = page,
                PageSize = pageSize
            };

            var resultado = await _auditoriaService.ObtenerAuditoriasAsync(filtros);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener auditorías", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener detalle de una auditoría específica (Solo Superadmin)
    /// </summary>
    /// <param name="id">ID de la auditoría</param>
    /// <returns>Detalle de la auditoría</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<AuditoriaDto>> ObtenerAuditoriaPorId(int id)
    {
        try
        {
            var auditoria = await _auditoriaService.ObtenerAuditoriaPorIdAsync(id);

            if (auditoria == null)
            {
                return NotFound(new { message = "Auditoría no encontrada" });
            }

            return Ok(auditoria);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener auditoría", error = ex.Message });
        }
    }
}
