namespace SistemaDeGestionDeClub.Domain.Entities;

public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Dni { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    
    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    
    // Relaciones
    public Usuario? Usuario { get; set; }
    public Socio? Socio { get; set; }
    
    public string NombreCompleto => $"{Nombre} {Apellido}";
}
