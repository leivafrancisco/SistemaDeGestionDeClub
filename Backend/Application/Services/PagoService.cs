using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;
using System.Globalization;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IPagoService
{
    Task<List<PagoDto>> ObtenerTodosAsync(FiltrosPagosDto? filtros = null);
    Task<PagoDto?> ObtenerPorIdAsync(int id);
    Task<ComprobantePagoDto> RegistrarPagoAsync(RegistrarPagoDto dto, int idUsuario);
    Task<ComprobantePagoDto> GenerarComprobanteAsync(int idPago);
    Task<bool> AnularPagoAsync(int id);
    Task<decimal> ObtenerTotalRecaudadoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
}

public class PagoService : IPagoService
{
    private readonly ClubDbContext _context;

    public PagoService(ClubDbContext context)
    {
        _context = context;
    }

    public async Task<List<PagoDto>> ObtenerTodosAsync(FiltrosPagosDto? filtros = null)
    {
        filtros ??= new FiltrosPagosDto();

        var query = _context.Pagos
            .Include(p => p.Membresia)
                .ThenInclude(m => m.Socio)
                    .ThenInclude(s => s.Persona)
            .Include(p => p.MetodoPago)
            .Include(p => p.UsuarioProcesa)
                .ThenInclude(u => u!.Persona)
            .Where(p => p.FechaEliminacion == null)
            .AsQueryable();

        // Filtros
        if (filtros.IdMembresia.HasValue)
        {
            query = query.Where(p => p.IdMembresia == filtros.IdMembresia.Value);
        }

        if (filtros.IdSocio.HasValue)
        {
            query = query.Where(p => p.Membresia.IdSocio == filtros.IdSocio.Value);
        }

        if (filtros.IdMetodoPago.HasValue)
        {
            query = query.Where(p => p.IdMetodoPago == filtros.IdMetodoPago.Value);
        }

        if (filtros.FechaDesde.HasValue)
        {
            query = query.Where(p => p.FechaPago >= filtros.FechaDesde.Value);
        }

        if (filtros.FechaHasta.HasValue)
        {
            query = query.Where(p => p.FechaPago <= filtros.FechaHasta.Value);
        }

        var pagos = await query
            .OrderByDescending(p => p.FechaPago)
            .ThenByDescending(p => p.FechaCreacion)
            .Skip((filtros.Page - 1) * filtros.PageSize)
            .Take(filtros.PageSize)
            .ToListAsync();

        return pagos.Select(p => MapearADto(p)).ToList();
    }

    public async Task<PagoDto?> ObtenerPorIdAsync(int id)
    {
        var pago = await _context.Pagos
            .Include(p => p.Membresia)
                .ThenInclude(m => m.Socio)
                    .ThenInclude(s => s.Persona)
            .Include(p => p.MetodoPago)
            .Include(p => p.UsuarioProcesa)
                .ThenInclude(u => u!.Persona)
            .Where(p => p.Id == id && p.FechaEliminacion == null)
            .FirstOrDefaultAsync();

        return pago == null ? null : MapearADto(pago);
    }

    public async Task<ComprobantePagoDto> RegistrarPagoAsync(RegistrarPagoDto dto, int idUsuario)
    {
        // Validar que la membresía existe
        var membresia = await _context.Membresias
            .Include(m => m.Socio)
                .ThenInclude(s => s.Persona)
            .Include(m => m.MembresiaActividades)
                .ThenInclude(ma => ma.Actividad)
            .Include(m => m.Pagos)
            .FirstOrDefaultAsync(m => m.Id == dto.IdMembresia && m.FechaEliminacion == null);

        if (membresia == null)
        {
            throw new InvalidOperationException("Membresía no encontrada");
        }

        // Validar que el método de pago existe
        var metodoPago = await _context.MetodosPago.FindAsync(dto.IdMetodoPago);
        if (metodoPago == null)
        {
            throw new InvalidOperationException("Método de pago no encontrado");
        }

        // Validar que el monto sea positivo
        if (dto.Monto <= 0)
        {
            throw new InvalidOperationException("El monto debe ser mayor a cero");
        }

        // Calcular saldo actual
        var totalCargado = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento);
        var totalPagado = membresia.Pagos.Where(p => p.FechaEliminacion == null).Sum(p => p.Monto);
        var saldoActual = totalCargado - totalPagado;

        // Validar que no se pague de más
        if (dto.Monto > saldoActual)
        {
            throw new InvalidOperationException($"El monto excede el saldo pendiente de ${saldoActual:N2}. No se permite sobrepago.");
        }

        // Crear el pago
        var pago = new Pago
        {
            IdMembresia = dto.IdMembresia,
            IdMetodoPago = dto.IdMetodoPago,
            IdUsuarioProcesa = idUsuario,
            Monto = dto.Monto,
            FechaPago = dto.FechaPago ?? DateTime.Now,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        // Generar y retornar el comprobante
        return await GenerarComprobanteAsync(pago.Id);
    }

    public async Task<ComprobantePagoDto> GenerarComprobanteAsync(int idPago)
    {
        var pago = await _context.Pagos
            .Include(p => p.Membresia)
                .ThenInclude(m => m.Socio)
                    .ThenInclude(s => s.Persona)
            .Include(p => p.Membresia.MembresiaActividades)
                .ThenInclude(ma => ma.Actividad)
            .Include(p => p.Membresia.Pagos)
            .Include(p => p.MetodoPago)
            .Include(p => p.UsuarioProcesa)
                .ThenInclude(u => u!.Persona)
            .FirstOrDefaultAsync(p => p.Id == idPago && p.FechaEliminacion == null);

        if (pago == null)
        {
            throw new InvalidOperationException("Pago no encontrado");
        }

        var membresia = pago.Membresia;
        var totalCargado = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento);

        // Calcular total pagado ANTES de este pago
        var totalPagadoAntes = membresia.Pagos
            .Where(p => p.FechaEliminacion == null && p.Id != idPago)
            .Sum(p => p.Monto);

        // Calcular total pagado DESPUÉS de este pago (incluyéndolo)
        var totalPagadoDespues = membresia.Pagos
            .Where(p => p.FechaEliminacion == null)
            .Sum(p => p.Monto);

        var nuevoSaldo = totalCargado - totalPagadoDespues;

        var nombresMeses = new CultureInfo("es-ES").DateTimeFormat.MonthNames;
        var nombreMes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombresMeses[membresia.PeriodoMes - 1]);

        // Generar número de comprobante: PAG-XXXXXX-AAAA
        var numeroComprobante = $"PAG-{idPago:D6}-{pago.FechaPago.Year}";

        return new ComprobantePagoDto
        {
            IdPago = pago.Id,
            NumeroComprobante = numeroComprobante,
            FechaEmision = pago.FechaCreacion,

            // Datos del Socio
            NumeroSocio = membresia.Socio.NumeroSocio,
            NombreSocio = membresia.Socio.Persona.NombreCompleto,

            // Datos de la Membresía
            PeriodoMembresia = $"{nombreMes} {membresia.PeriodoAnio}",
            TotalMembresia = totalCargado,
            TotalPagadoAntes = totalPagadoAntes,
            MontoPago = pago.Monto,
            NuevoSaldo = nuevoSaldo,
            EstaPaga = nuevoSaldo <= 0,

            // Datos del Pago
            MetodoPago = pago.MetodoPago.Nombre,
            FechaPago = pago.FechaPago,
            UsuarioProcesa = pago.UsuarioProcesa?.Persona?.NombreCompleto ?? "Sistema",

            // Detalle de Actividades
            Actividades = membresia.MembresiaActividades.Select(ma => new ActividadComprobanteDto
            {
                Nombre = ma.Actividad.Nombre,
                Precio = ma.PrecioAlMomento
            }).ToList()
        };
    }

    public async Task<bool> AnularPagoAsync(int id)
    {
        var pago = await _context.Pagos.FindAsync(id);

        if (pago == null || pago.FechaEliminacion != null)
        {
            return false;
        }

        // Soft delete
        pago.FechaEliminacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<decimal> ObtenerTotalRecaudadoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var query = _context.Pagos
            .Where(p => p.FechaEliminacion == null)
            .AsQueryable();

        if (fechaDesde.HasValue)
        {
            query = query.Where(p => p.FechaPago >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            query = query.Where(p => p.FechaPago <= fechaHasta.Value);
        }

        return await query.SumAsync(p => p.Monto);
    }

    private PagoDto MapearADto(Pago pago)
    {
        var nombresMeses = new CultureInfo("es-ES").DateTimeFormat.MonthNames;
        var nombreMes = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombresMeses[pago.Membresia.PeriodoMes - 1]);

        return new PagoDto
        {
            Id = pago.Id,
            IdMembresia = pago.IdMembresia,
            PeriodoMembresia = $"{nombreMes} {pago.Membresia.PeriodoAnio}",
            IdSocio = pago.Membresia.IdSocio,
            NumeroSocio = pago.Membresia.Socio.NumeroSocio,
            NombreSocio = pago.Membresia.Socio.Persona.NombreCompleto,
            IdMetodoPago = pago.IdMetodoPago,
            MetodoPagoNombre = pago.MetodoPago.Nombre,
            IdUsuarioProcesa = pago.IdUsuarioProcesa,
            NombreUsuarioProcesa = pago.UsuarioProcesa?.Persona?.NombreCompleto,
            Monto = pago.Monto,
            FechaPago = pago.FechaPago,
            FechaCreacion = pago.FechaCreacion
        };
    }
}
