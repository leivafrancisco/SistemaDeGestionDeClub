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
