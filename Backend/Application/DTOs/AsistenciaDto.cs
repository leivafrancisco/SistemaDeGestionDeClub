namespace SistemaDeGestionDeClub.Application.DTOs;

public class AsistenciaDto
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public string NombreSocio { get; set; } = string.Empty;
    public DateTime FechaHoraIngreso { get; set; }
}

public class RegistrarAsistenciaDto
{
    public int IdSocio { get; set; }
    public DateTime FechaHoraIngreso { get; set; }
}

public class VerificarAsistenciaDto
{
    public int IdSocio { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public string NombreSocio { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public bool TieneAcceso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string EstadoMembresia { get; set; } = string.Empty;
    public decimal? SaldoPendiente { get; set; }
    public DateTime? FechaVigenciaHasta { get; set; }
    public List<string> Actividades { get; set; } = new();
}
