using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;

namespace SistemaDeGestionDeClub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "superadmin")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;

    public BackupController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    /// <summary>
    /// Crear backup de la base de datos (Solo Superadmin)
    /// </summary>
    /// <param name="request">Datos del backup: nombre BD, ruta destino, nombre archivo</param>
    /// <returns>Resultado del backup con ruta completa y tamaño</returns>
    [HttpPost]
    public async Task<ActionResult<BackupResponseDto>> CrearBackup([FromBody] BackupRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NombreBaseDatos))
            {
                return BadRequest(new { message = "El nombre de la base de datos es requerido" });
            }

            if (string.IsNullOrWhiteSpace(request.RutaDestino))
            {
                return BadRequest(new { message = "La ruta de destino es requerida" });
            }

            if (string.IsNullOrWhiteSpace(request.NombreArchivo))
            {
                return BadRequest(new { message = "El nombre del archivo es requerido" });
            }

            var resultado = await _backupService.CrearBackupAsync(request);

            if (!resultado.Exitoso)
            {
                return BadRequest(resultado);
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear backup", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtener lista de bases de datos disponibles (Solo Superadmin)
    /// </summary>
    /// <returns>Lista de nombres de bases de datos</returns>
    [HttpGet("bases-datos")]
    public async Task<ActionResult<List<string>>> ObtenerBasesDatos()
    {
        try
        {
            var basesDatos = await _backupService.ObtenerBasesDatosDisponiblesAsync();
            return Ok(basesDatos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener bases de datos", error = ex.Message });
        }
    }

    /// <summary>
    /// Listar archivos de backup disponibles (Solo Superadmin)
    /// </summary>
    /// <returns>Lista de archivos de backup</returns>
    [HttpGet("archivos")]
    public async Task<ActionResult<List<BackupArchivoDto>>> ObtenerBackups()
    {
        try
        {
            var backups = await _backupService.ObtenerBackupsDisponiblesAsync();
            return Ok(backups);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener archivos de backup", error = ex.Message });
        }
    }

    /// <summary>
    /// Descargar archivo de backup (Solo Superadmin)
    /// </summary>
    /// <param name="request">Ruta completa del archivo de backup</param>
    /// <returns>Archivo de backup para descargar</returns>
    [HttpPost("descargar")]
    public async Task<IActionResult> DescargarBackup([FromBody] DescargarBackupRequestDto request)
    {
        try
        {
            var (exito, datos, nombreArchivo, mensajeError) = await _backupService.ObtenerArchivoBackupAsync(request.RutaCompleta);

            if (!exito)
            {
                if (mensajeError?.Contains("no existe") == true)
                {
                    return NotFound(new { mensaje = mensajeError });
                }
                return BadRequest(new { mensaje = mensajeError });
            }

            // Retornar el archivo para descarga
            return File(datos!, "application/octet-stream", nombreArchivo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al descargar el archivo de backup", error = ex.Message });
        }
    }
}
