namespace SistemaDeGestionDeClub.Domain.Entities;

public class Membresia
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal CostoTotal { get; set; }
    public string EstadoPago { get; set; } = "PENDIENTE";

    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    
    // Relaciones
    public Socio Socio { get; set; } = null!;
    public ICollection<MembresiaActividad> MembresiaActividades { get; set; } = new List<MembresiaActividad>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    
    // Propiedades calculadas
    public decimal TotalCargado => MembresiaActividades.Sum(ma => ma.PrecioAlMomento);
    public decimal TotalPagado => Pagos.Sum(p => p.Monto);
    public decimal Saldo => TotalCargado - TotalPagado;
}
