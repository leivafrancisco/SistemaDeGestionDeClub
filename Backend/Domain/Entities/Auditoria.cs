namespace SistemaDeGestionDeClub.Domain.Entities;

public class Auditoria
{
    public int Id { get; set; }
    public string Tabla { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public int? IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public DateTime FechaHora { get; set; }
    public string? ValoresAnteriores { get; set; } // JSON
    public string? ValoresNuevos { get; set; } // JSON
    public string? NombreEntidad { get; set; }
    public string? IdEntidad { get; set; }
    public string? Detalles { get; set; }
}
