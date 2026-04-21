namespace SistemaDeGestionDeClub.Domain.Entities;

public class Cuota
{
    public int Id { get; set; }
    public int IdMembresia { get; set; }
    public int NumeroCuota { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; } = CuotaEstado.Pendiente;

    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }

    // Relaciones
    public Membresia Membresia { get; set; } = null!;
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    // Propiedad calculada (no mapeada)
    public bool EstaPagada => Pagos.Any(p => p.FechaEliminacion == null);
}
