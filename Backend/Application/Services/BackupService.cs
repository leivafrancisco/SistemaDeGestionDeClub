using Microsoft.Data.SqlClient;
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
    private readonly string _backupDirectory;

    public BackupService(ClubDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _backupDirectory = configuration["BackupDirectory"] ?? "/var/backups";
    }

    public async Task<BackupResponseDto> CrearBackupAsync(BackupRequestDto request)
    {
        try
        {
            var directorio = string.IsNullOrWhiteSpace(request.RutaDestino)
                ? _backupDirectory
                : request.RutaDestino;

            var nombreArchivo = string.IsNullOrWhiteSpace(request.NombreArchivo)
                ? $"gestion_club_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
                : request.NombreArchivo.EndsWith(".bak") ? request.NombreArchivo : $"{request.NombreArchivo}.bak";

            var rutaCompleta = Path.Combine(directorio, nombreArchivo);

            var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(
                $"BACKUP DATABASE [gestion_club] TO DISK = N'{rutaCompleta}' WITH FORMAT, INIT, NAME = N'gestion_club-Full', STATS = 10",
                connection);
            command.CommandTimeout = 300;
            await command.ExecuteNonQueryAsync();

            return new BackupResponseDto
            {
                Exitoso = true,
                Mensaje = $"Backup creado exitosamente: {nombreArchivo}",
                RutaCompleta = rutaCompleta,
                FechaHoraBackup = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            return new BackupResponseDto
            {
                Exitoso = false,
                Mensaje = $"Error al crear el backup: {ex.Message}",
                FechaHoraBackup = DateTime.Now
            };
        }
    }

    public Task<List<string>> ObtenerBasesDatosDisponiblesAsync()
    {
        return Task.FromResult(new List<string> { "gestion_club" });
    }

    public async Task<(bool exito, byte[]? datos, string? nombreArchivo, string? mensajeError)> ObtenerArchivoBackupAsync(string rutaCompleta)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rutaCompleta))
                return (false, null, null, "La ruta del archivo es requerida");

            if (rutaCompleta.Contains(".."))
                return (false, null, null, "La ruta contiene caracteres no permitidos");

            if (!rutaCompleta.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                return (false, null, null, "Solo se pueden descargar archivos .bak");

            if (!File.Exists(rutaCompleta))
                return (false, null, null, $"El archivo no existe: {rutaCompleta}");

            var datos = await File.ReadAllBytesAsync(rutaCompleta);
            var nombreArchivo = Path.GetFileName(rutaCompleta);
            return (true, datos, nombreArchivo, null);
        }
        catch (UnauthorizedAccessException)
        {
            return (false, null, null, "Sin permisos para acceder al archivo");
        }
        catch (Exception ex)
        {
            return (false, null, null, $"Error al leer el archivo: {ex.Message}");
        }
    }

    public async Task<List<BackupArchivoDto>> ObtenerBackupsDisponiblesAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(_backupDirectory))
                    return new List<BackupArchivoDto>();

                return Directory.GetFiles(_backupDirectory, "*.bak")
                    .Select(ruta =>
                    {
                        var info = new FileInfo(ruta);
                        return new BackupArchivoDto
                        {
                            Nombre = info.Name,
                            RutaCompleta = ruta,
                            FechaCreacion = info.CreationTime,
                            TamanoBytes = info.Length,
                            TamanoFormateado = FormatearTamano(info.Length)
                        };
                    })
                    .OrderByDescending(b => b.FechaCreacion)
                    .ToList();
            }
            catch
            {
                return new List<BackupArchivoDto>();
            }
        });
    }

    public async Task<RestoreResponseDto> RestaurarBackupAsync(RestoreRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RutaBackup))
                return new RestoreResponseDto { Exitoso = false, Mensaje = "La ruta del archivo es requerida", FechaHoraRestore = DateTime.Now };

            if (!File.Exists(request.RutaBackup))
                return new RestoreResponseDto { Exitoso = false, Mensaje = $"El archivo no existe: {request.RutaBackup}", FechaHoraRestore = DateTime.Now };

            // Conectar a master para poder restaurar gestion_club
            var connectionString = _configuration.GetConnectionString("DefaultConnection")!
                .Replace("Database=gestion_club", "Database=master")
                .Replace("database=gestion_club", "Database=master");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Cerrar conexiones activas antes de restaurar
            using var killCmd = new SqlCommand(
                "ALTER DATABASE [gestion_club] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                connection);
            killCmd.CommandTimeout = 60;
            await killCmd.ExecuteNonQueryAsync();

            using var restoreCmd = new SqlCommand(
                $"RESTORE DATABASE [gestion_club] FROM DISK = N'{request.RutaBackup}' WITH REPLACE, STATS = 10",
                connection);
            restoreCmd.CommandTimeout = 600;
            await restoreCmd.ExecuteNonQueryAsync();

            using var multiCmd = new SqlCommand(
                "ALTER DATABASE [gestion_club] SET MULTI_USER",
                connection);
            await multiCmd.ExecuteNonQueryAsync();

            return new RestoreResponseDto
            {
                Exitoso = true,
                Mensaje = "Base de datos restaurada exitosamente",
                FechaHoraRestore = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            return new RestoreResponseDto
            {
                Exitoso = false,
                Mensaje = $"Error al restaurar: {ex.Message}",
                FechaHoraRestore = DateTime.Now
            };
        }
    }

    private static string FormatearTamano(long bytes)
    {
        string[] sufijos = { "B", "KB", "MB", "GB", "TB" };
        int orden = 0;
        double tamano = bytes;
        while (tamano >= 1024 && orden < sufijos.Length - 1) { orden++; tamano /= 1024; }
        return $"{tamano:0.##} {sufijos[orden]}";
    }
}
