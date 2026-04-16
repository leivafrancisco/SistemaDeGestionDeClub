using Microsoft.Extensions.Configuration;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IBackupService
{
    Task<BackupResponseDto> CrearBackupAsync(BackupRequestDto request);
    Task<List<string>> ObtenerBasesDatosDisponiblesAsync();
    Task<(bool exito, byte[]? datos, string? nombreArchivo, string? mensajeError)> ObtenerArchivoBackupAsync(string rutaCompleta);
    Task<List<BackupArchivoDto>> ObtenerBackupsDisponiblesAsync();
    Task<RestoreResponseDto> RestaurarBackupAsync(RestoreRequestDto request);
}

public class BackupService : IBackupService
{
    private readonly ClubDbContext _context;
    private readonly IConfiguration _configuration;

    public BackupService(ClubDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public Task<BackupResponseDto> CrearBackupAsync(BackupRequestDto request)
    {
        return Task.FromResult(new BackupResponseDto
        {
            Exitoso = false,
            Mensaje = "El backup manual no está disponible en modo cloud (Supabase). Los backups son gestionados automáticamente por Supabase.",
            FechaHoraBackup = DateTime.Now
        });
    }

    public Task<List<string>> ObtenerBasesDatosDisponiblesAsync()
    {
        return Task.FromResult(new List<string> { "postgres" });
    }

    public async Task<(bool exito, byte[]? datos, string? nombreArchivo, string? mensajeError)> ObtenerArchivoBackupAsync(string rutaCompleta)
    {
        try
        {
            // Validación de seguridad: evitar path traversal
            if (string.IsNullOrWhiteSpace(rutaCompleta))
            {
                return (false, null, null, "La ruta del archivo es requerida");
            }

            if (rutaCompleta.Contains(".."))
            {
                return (false, null, null, "La ruta contiene caracteres no permitidos");
            }

            // Validar que sea un archivo .bak
            if (!rutaCompleta.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
            {
                return (false, null, null, "Solo se pueden descargar archivos .bak");
            }

            // NOTA: Traducir ruta del contenedor Docker a ruta del host Mac
            // El backup se crea en /backups dentro del contenedor SQL Server,
            // pero el backend corre en Mac donde ese path está mapeado a /Users/Shared/Backups/
            string rutaEnHost = rutaCompleta;
            if (rutaCompleta.StartsWith("/backups/"))
            {
                rutaEnHost = rutaCompleta.Replace("/backups/", "/Users/Shared/Backups/");
            }

            // Verificar que el archivo existe en el host
            if (!File.Exists(rutaEnHost))
            {
                return (false, null, null, $"El archivo de backup no existe en la ruta especificada: {rutaEnHost}");
            }

            // Leer el archivo
            byte[] archivoBytes = await File.ReadAllBytesAsync(rutaEnHost);
            string nombreArchivo = Path.GetFileName(rutaCompleta);

            return (true, archivoBytes, nombreArchivo, null);
        }
        catch (UnauthorizedAccessException)
        {
            return (false, null, null, "No tiene permisos para acceder al archivo");
        }
        catch (IOException ex)
        {
            return (false, null, null, $"Error al leer el archivo: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, null, $"Error al obtener el archivo de backup: {ex.Message}");
        }
    }

    public async Task<List<BackupArchivoDto>> ObtenerBackupsDisponiblesAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                // Directorio donde están almacenados los backups en el host Mac
                string directorioBackups = "/Users/Shared/Backups";

                if (!Directory.Exists(directorioBackups))
                {
                    return new List<BackupArchivoDto>();
                }

                var archivos = Directory.GetFiles(directorioBackups, "*.bak")
                    .Select(rutaArchivo =>
                    {
                        var fileInfo = new FileInfo(rutaArchivo);
                        // Convertir la ruta del host Mac a la ruta del contenedor Docker
                        var rutaDocker = rutaArchivo.Replace("/Users/Shared/Backups/", "/backups/");

                        return new BackupArchivoDto
                        {
                            Nombre = fileInfo.Name,
                            RutaCompleta = rutaDocker,
                            FechaCreacion = fileInfo.CreationTime,
                            TamanoBytes = fileInfo.Length,
                            TamanoFormateado = FormatearTamano(fileInfo.Length)
                        };
                    })
                    .OrderByDescending(b => b.FechaCreacion)
                    .ToList();

                return archivos;
            }
            catch
            {
                return new List<BackupArchivoDto>();
            }
        });
    }

    private string FormatearTamano(long bytes)
    {
        string[] sufijos = { "B", "KB", "MB", "GB", "TB" };
        int orden = 0;
        double tamano = bytes;

        while (tamano >= 1024 && orden < sufijos.Length - 1)
        {
            orden++;
            tamano /= 1024;
        }

        return $"{tamano:0.##} {sufijos[orden]}";
    }

    public Task<RestoreResponseDto> RestaurarBackupAsync(RestoreRequestDto request)
    {
        return Task.FromResult(new RestoreResponseDto
        {
            Exitoso = false,
            Mensaje = "La restauración manual no está disponible en modo cloud (Supabase). Los restores deben realizarse desde el panel de Supabase.",
            FechaHoraRestore = DateTime.Now
        });
    }
}
