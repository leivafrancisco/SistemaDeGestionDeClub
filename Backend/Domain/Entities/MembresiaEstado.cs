namespace SistemaDeGestionDeClub.Domain.Entities;

/// <summary>
/// Constantes para los estados posibles de una membresía
/// </summary>
public static class MembresiaEstado
{
    /// <summary>
    /// Membresía activa y vigente
    /// </summary>
    public const string Activa = "activa";

    /// <summary>
    /// Membresía vencida por fecha
    /// </summary>
    public const string Vencida = "vencida";

    /// <summary>
    /// Membresía con pago pendiente
    /// </summary>
    public const string PagoPendiente = "pago_pendiente";

    /// <summary>
    /// Membresía cancelada
    /// </summary>
    public const string Cancelada = "cancelada";
}
