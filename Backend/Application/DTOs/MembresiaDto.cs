namespace SistemaDeGestionDeClub.Application.DTOs;

public class MembresiaDto
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public string NombreSocio { get; set; } = string.Empty;
    public short PeriodoAnio { get; set; }
    public byte PeriodoMes { get; set; }
    public string PeriodoTexto { get; set; } = string.Empty; // Ej: "Noviembre 2025"
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalCargado { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal Saldo { get; set; }
    public bool EstaPaga { get; set; }
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

public class ActualizarMembresiaDto
{
    public List<int> IdsActividades { get; set; } = new();
}

public class FiltrosMembresiasDto
{
    public int? IdSocio { get; set; }
    public short? PeriodoAnio { get; set; }
    public byte? PeriodoMes { get; set; }
    public bool? SoloImpagas { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AsignarActividadDto
{
    public int IdMembresia { get; set; }
    public int IdActividad { get; set; }
}

public class RemoverActividadDto
{
    public int IdMembresia { get; set; }
    public int IdActividad { get; set; }
}
