namespace SistemaDeGestionDeClub.Application.DTOs;

public class MembresiaDto
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public string NombreSocio { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaCreacion { get; set; }
    public decimal CostoTotal { get; set; }
    public string Estado { get; set; } = string.Empty; // AL DIA o VENCIDA
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
}

public class CrearMembresiaDto
{
    public int IdSocio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal CostoTotal { get; set; }
    public List<int> IdsActividades { get; set; } = new();

    // Datos del pago inicial
    public decimal Monto { get; set; } // Monto del pago inicial (puede incluir descuentos/bonificaciones)
    public int IdMetodoPago { get; set; } // Método de pago utilizado
    public int IdUsuarioProcesa { get; set; } // Usuario que procesa el pago
}

public class ActualizarMembresiaDto
{
    public DateTime? FechaInicio { get; set; } // Opcional - Nueva fecha de inicio
    public DateTime? FechaFin { get; set; } // Opcional - Nueva fecha de fin
    public List<int>? IdsActividades { get; set; } // Opcional - Nuevo array de IDs de actividades
}

public class FiltrosMembresiasDto
{
    public int? IdSocio { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public bool? SoloImpagas { get; set; }
    public string? Search { get; set; } // Búsqueda por nombre o número de socio
    public string? EstadoVigencia { get; set; } // Filtro por vigencia: todas, vigentes, vencidas, proximas_vencer
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
