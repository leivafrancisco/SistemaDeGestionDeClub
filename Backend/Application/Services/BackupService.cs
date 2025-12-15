using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

    public async Task<BackupResponseDto> CrearBackupAsync(BackupRequestDto request)
    {
        try
        {
            // NOTA: No validamos Directory.Exists porque la ruta está en el contenedor de SQL Server,
            // no en el filesystem local del backend. SQL Server validará la ruta al ejecutar BACKUP DATABASE.

            // Generar nombre completo del archivo con fecha, hora y extensión
            var fechaHora = DateTime.Now;
            var nombreArchivoCompleto = $"{request.NombreArchivo}_{fechaHora:yyyyMMdd_HHmmss}.bak";
            var rutaCompleta = Path.Combine(request.RutaDestino, nombreArchivoCompleto);

            // Construir el comando SQL para backup
            var sqlBackup = $@"
                BACKUP DATABASE [{request.NombreBaseDatos}]
                TO DISK = @rutaBackup
                WITH FORMAT,
                     INIT,
                     NAME = 'Backup completo - {fechaHora:yyyy-MM-dd HH:mm:ss}',
                     SKIP,
                     NOREWIND,
                     NOUNLOAD,
                     COMPRESSION,
                     STATS = 10";

            // Ejecutar el backup usando SQL directo
            await using var connection = _context.Database.GetDbConnection() as SqlConnection;
            if (connection == null)
            {
                return new BackupResponseDto
                {
                    Exitoso = false,
                    Mensaje = "No se pudo obtener la conexión a SQL Server",
                    FechaHoraBackup = fechaHora
                };
            }

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = sqlBackup;
            command.CommandTimeout = 300; // 5 minutos de timeout
            command.Parameters.Add(new SqlParameter("@rutaBackup", rutaCompleta));

            await command.ExecuteNonQueryAsync();

            // NOTA: No verificamos File.Exists ni obtenemos el tamaño porque el archivo está en el contenedor,
            // no en el filesystem local. Si ExecuteNonQueryAsync no lanzó excepción, el backup fue exitoso.

            return new BackupResponseDto
            {
                Exitoso = true,
                Mensaje = "Backup creado exitosamente",
                RutaCompleta = rutaCompleta,
                FechaHoraBackup = fechaHora,
                TamanoBytes = null // No podemos obtener el tamaño desde el backend
            };
        }
        catch (SqlException ex)
        {
            return new BackupResponseDto
            {
                Exitoso = false,
                Mensaje = $"Error de SQL Server al crear backup: {ex.Message}",
                FechaHoraBackup = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            return new BackupResponseDto
            {
                Exitoso = false,
                Mensaje = $"Error al crear backup: {ex.Message}",
                FechaHoraBackup = DateTime.Now
            };
        }
    }

    public async Task<List<string>> ObtenerBasesDatosDisponiblesAsync()
    {
        try
        {
            var basesDatos = new List<string>();

            await using var connection = _context.Database.GetDbConnection() as SqlConnection;
            if (connection == null)
            {
                return basesDatos;
            }

            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT name
                FROM sys.databases
                WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')
                AND state_desc = 'ONLINE'
                ORDER BY name";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                basesDatos.Add(reader.GetString(0));
            }

            return basesDatos;
        }
        catch
        {
            return new List<string>();
        }
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
}
