namespace SistemaDeGestionDeClub.Domain.Entities;

public class MembresiaActividad
{
    public int IdMembresia { get; set; }
    public int IdActividad { get; set; }
    public decimal PrecioAlMomento { get; set; }
    
    // Relaciones
    public Membresia Membresia { get; set; } = null!;
    public Actividad Actividad { get; set; } = null!;
}
