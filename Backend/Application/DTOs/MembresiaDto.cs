namespace SistemaDeGestionDeClub.Application.DTOs;

public class MembresiaDto
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public short PeriodoAnio { get; set; }
    public byte PeriodoMes { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalCargado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal Saldo { get; set; }
    public List<ActividadEnMembresiaDto> Actividades { get; set; } = new();
}

public class ActividadEnMembresiaDto
{
    public int IdActividad { get; set; }
    public string NombreActividad { get; set; } = string.Empty;
    public decimal PrecioAlMomento { get; set; }
    public bool EsCuotaBase { get; set; }
}

public class CrearMembresiaDto
{
    public int IdSocio { get; set; }
    public short PeriodoAnio { get; set; }
    public byte PeriodoMes { get; set; }
    public List<int> IdsActividades { get; set; } = new();
}
