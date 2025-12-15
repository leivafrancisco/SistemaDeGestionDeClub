namespace SistemaDeGestionDeClub.Application.DTOs;

public class AuditoriaDto
{
    public int Id { get; set; }
    public string Tabla { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public int? IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public DateTime FechaHora { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public string? NombreEntidad { get; set; }
    public string? IdEntidad { get; set; }
    public string? Detalles { get; set; }
}

public class AuditoriaFiltrosDto
{
    public string? Tabla { get; set; }
    public string? Operacion { get; set; }
    public int? IdUsuario { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
