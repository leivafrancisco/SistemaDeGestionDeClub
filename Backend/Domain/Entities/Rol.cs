namespace SistemaDeGestionDeClub.Domain.Entities;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    
    // Relaciones
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
