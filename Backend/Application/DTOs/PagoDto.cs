namespace SistemaDeGestionDeClub.Application.DTOs;

public class PagoDto
{
    public int Id { get; set; }
    public int IdMembresia { get; set; }
    public int IdMetodoPago { get; set; }
    public string MetodoPagoNombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string? UsuarioProcesa { get; set; }
}

public class CrearPagoDto
{
    public int IdMembresia { get; set; }
    public int IdMetodoPago { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
}
