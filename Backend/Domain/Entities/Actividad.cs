namespace SistemaDeGestionDeClub.Domain.Entities;

public class Actividad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool EsCuotaBase { get; set; }

    // Auditoría
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }

    // Relaciones
    public ICollection<MembresiaActividad> MembresiaActividades { get; set; } = new List<MembresiaActividad>();
}
