namespace SistemaDeGestionDeClub.Application.DTOs;

public class SocioDto
{
    public int Id { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
}

public class CrearSocioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
}

public class ActualizarSocioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
}

// DTO de salida de sp_ResumenSocio
public class ResumenSocioDto
{
    public int IdSocio { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NumeroSocio { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public int TotalMembresias { get; set; }
    public decimal TotalCargado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public DateTime? UltimaFechaPago { get; set; }
    public int AsistenciasEsteMes { get; set; }
}

// DTO de entrada para sp_CambiarEstadoSocio
public class CambiarEstadoSocioDto
{
    public bool EstaActivo { get; set; }
}
