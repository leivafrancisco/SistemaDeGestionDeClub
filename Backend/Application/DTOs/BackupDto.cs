namespace SistemaDeGestionDeClub.Application.DTOs;

public class BackupRequestDto
{
    public string NombreBaseDatos { get; set; } = string.Empty;
    public string RutaDestino { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
}

public class BackupResponseDto
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? RutaCompleta { get; set; }
    public DateTime FechaHoraBackup { get; set; }
    public long? TamanoBytes { get; set; }
}

public class DescargarBackupRequestDto
{
    public string RutaCompleta { get; set; } = string.Empty;
}

public class BackupArchivoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string RutaCompleta { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public long TamanoBytes { get; set; }
    public string TamanoFormateado { get; set; } = string.Empty;
}

public class RestoreRequestDto
{
    public string RutaBackup { get; set; } = string.Empty;
    public string NombreBaseDatos { get; set; } = string.Empty;
}

public class RestoreResponseDto
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaHoraRestore { get; set; }
}
