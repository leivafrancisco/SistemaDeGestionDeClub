namespace SistemaDeGestionDeClub.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public int IdPersona { get; set; }
    public int IdRol { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public bool EstaActivo { get; set; } = true;
    
    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    
    // Relaciones
    public Persona Persona { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
    public ICollection<Pago> PagosProcesados { get; set; } = new List<Pago>();
}
