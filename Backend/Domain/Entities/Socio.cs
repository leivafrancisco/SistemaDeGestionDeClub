namespace SistemaDeGestionDeClub.Domain.Entities;

public class Socio
{
    public int Id { get; set; }
    public int IdPersona { get; set; }
    public string NumeroSocio { get; set; } = string.Empty;
    public bool EstaActivo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    
    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    
    // Relaciones
    public Persona Persona { get; set; } = null!;
    public ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();
    public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
}
