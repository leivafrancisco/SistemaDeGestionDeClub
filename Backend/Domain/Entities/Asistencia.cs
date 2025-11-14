namespace SistemaDeGestionDeClub.Domain.Entities;

public class Asistencia
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public DateTime FechaHoraIngreso { get; set; }
    
    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    
    // Relaciones
    public Socio Socio { get; set; } = null!;
}
