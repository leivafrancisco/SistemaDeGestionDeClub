namespace SistemaDeGestionDeClub.Application.DTOs;

public class PagoDto
{
    public int Id { get; set; }
    public int IdMembresia { get; set; }
    public string PeriodoMembresia { get; set; } = string.Empty;
    public int IdSocio { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public string NombreSocio { get; set; } = string.Empty;
    public int IdMetodoPago { get; set; }
    public string MetodoPagoNombre { get; set; } = string.Empty;
    public int? IdUsuarioProcesa { get; set; }
    public string? NombreUsuarioProcesa { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class RegistrarPagoDto
{
    public int IdMembresia { get; set; }
    public int IdMetodoPago { get; set; }
    public decimal Monto { get; set; }
    public DateTime? FechaPago { get; set; } // Opcional, default: hoy
}

public class ComprobantePagoDto
{
    public int IdPago { get; set; }
    public string NumeroComprobante { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }

    // Datos del Socio
    public string NumeroSocio { get; set; } = string.Empty;
    public string NombreSocio { get; set; } = string.Empty;

    // Datos de la Membresía
    public string PeriodoMembresia { get; set; } = string.Empty;
    public decimal TotalMembresia { get; set; }
    public decimal TotalPagadoAntes { get; set; }
    public decimal MontoPago { get; set; }
    public decimal NuevoSaldo { get; set; }
    public bool EstaPaga { get; set; }

    // Datos del Pago
    public string MetodoPago { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
    public string UsuarioProcesa { get; set; } = string.Empty;

    // Detalle de Actividades
    public List<ActividadComprobanteDto> Actividades { get; set; } = new();
}

public class ActividadComprobanteDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

public class FiltrosPagosDto
{
    public int? IdMembresia { get; set; }
    public int? IdSocio { get; set; }
    public int? IdMetodoPago { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
