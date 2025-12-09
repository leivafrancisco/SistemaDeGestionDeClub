namespace SistemaDeGestionDeClub.Domain.Entities;

public class Membresia
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal CostoTotal { get; set; }
    public string Estado { get; set; } = MembresiaEstado.Activa; // Estado de la membresía

    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    
    // Relaciones
    public Socio Socio { get; set; } = null!;
    public ICollection<MembresiaActividad> MembresiaActividades { get; set; } = new List<MembresiaActividad>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    // Propiedades calculadas (no mapeadas a BD)
    public decimal TotalCargado => MembresiaActividades?.Sum(ma => ma.PrecioAlMomento) ?? 0;
    public decimal TotalPagado => Pagos?.Where(p => p.FechaEliminacion == null).Sum(p => p.Monto) ?? 0;
    public decimal Saldo => TotalCargado - TotalPagado;
}
