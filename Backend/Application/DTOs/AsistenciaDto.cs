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
