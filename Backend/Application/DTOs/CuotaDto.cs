namespace SistemaDeGestionDeClub.Application.DTOs;

public class CuotaDto
{
    public int Id { get; set; }
    public int IdMembresia { get; set; }
    public string PeriodoMembresia { get; set; } = string.Empty;
    public int IdSocio { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public string NombreSocio { get; set; } = string.Empty;
    public int NumeroCuota { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EsMorosa => Estado == "vencida";
    public int DiasVencida { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class MorosoDto
{
    public int IdSocio { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public string NombreSocio { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int CuotasVencidas { get; set; }
    public decimal DeudaTotal { get; set; }
    public DateTime FechaVencimientoMasTemprana { get; set; }
    public List<CuotaDto> Cuotas { get; set; } = new();
}

public class FiltrosCuotasDto
{
    public int? IdMembresia { get; set; }
    public int? IdSocio { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaVencimientoDesde { get; set; }
    public DateTime? FechaVencimientoHasta { get; set; }
    public bool? SoloVencidas { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ResumenCuotasDto
{
    public int TotalCuotas { get; set; }
    public int CuotasPendientes { get; set; }
    public int CuotasPagadas { get; set; }
    public int CuotasVencidas { get; set; }
    public decimal MontoTotalPendiente { get; set; }
    public decimal MontoTotalVencido { get; set; }
    public int TotalMorosos { get; set; }
}
