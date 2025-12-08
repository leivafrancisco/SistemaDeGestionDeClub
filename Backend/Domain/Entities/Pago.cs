namespace SistemaDeGestionDeClub.Domain.Entities;

public class Pago
{
    public int Id { get; set; }
    public int IdMembresia { get; set; }
    public int IdMetodoPago { get; set; }
    public int? IdUsuarioProcesa { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string EstadoPago { get; set; } = "COMPLETADO"; // COMPLETADO, PENDIENTE, ANULADO, etc.
    
    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    
    // Relaciones
    public Membresia Membresia { get; set; } = null!;
    public MetodoPago MetodoPago { get; set; } = null!;
    public Usuario? UsuarioProcesa { get; set; }
}
