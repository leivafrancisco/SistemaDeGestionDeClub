namespace SistemaDeGestionDeClub.Domain.Entities;

public class MetodoPago
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    
    // Relaciones
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
