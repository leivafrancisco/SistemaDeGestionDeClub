using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;
using System.Globalization;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IPagoService
{
    Task<List<PagoDto>> ObtenerTodosPagosAsync(FiltrosPagosDto? filtros = null);
    Task<PagoDto?> ObtenerPagoPorIdAsync(int id);
    Task<ComprobantePagoDto> RegistrarPagoAsync(RegistrarPagoDto dto, int idUsuario);
    Task<ComprobantePagoDto> GenerarComprobanteAsync(int idPago);
    Task<bool> AnularPagoAsync(int id);
    Task<decimal> ObtenerTotalRecaudadoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
    Task<List<MetodoPagoDto>> ObtenerMetodosPagoAsync();
    Task<EstadisticasPagosDto> ObtenerEstadisticasPagosAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null);
}

public class PagoService : IPagoService
{
    private readonly ClubDbContext _context;
    private readonly IMembresiaService _membresiaService;
    private readonly ICuotaService _cuotaService;

    public PagoService(ClubDbContext context, IMembresiaService membresiaService, ICuotaService cuotaService)
    {
        _context = context;
        _membresiaService = membresiaService;
        _cuotaService = cuotaService;
    }

    public async Task<List<PagoDto>> ObtenerTodosPagosAsync(FiltrosPagosDto? filtros = null)
    {
        filtros ??= new FiltrosPagosDto();

        var query = _context.Pagos
            .Include(p => p.Cuota)
                .ThenInclude(c => c.Membresia)
                    .ThenInclude(m => m.Socio)
                        .ThenInclude(s => s.Persona)
            .Include(p => p.MetodoPago)
            .Include(p => p.UsuarioProcesa)
                .ThenInclude(u => u!.Persona)
            .Where(p => p.FechaEliminacion == null)
            .AsQueryable();

        if (filtros.IdMembresia.HasValue)
            query = query.Where(p => p.Cuota.IdMembresia == filtros.IdMembresia.Value);

        if (filtros.IdSocio.HasValue)
            query = query.Where(p => p.Cuota.Membresia.IdSocio == filtros.IdSocio.Value);

        if (filtros.IdMetodoPago.HasValue)
            query = query.Where(p => p.IdMetodoPago == filtros.IdMetodoPago.Value);

        if (filtros.FechaDesde.HasValue)
            query = query.Where(p => p.FechaPago >= filtros.FechaDesde.Value);

        if (filtros.FechaHasta.HasValue)
            query = query.Where(p => p.FechaPago <= filtros.FechaHasta.Value);

        var pagos = await query
            .OrderByDescending(p => p.FechaPago)
            .ThenByDescending(p => p.FechaCreacion)
            .Skip((filtros.Page - 1) * filtros.PageSize)
            .Take(filtros.PageSize)
            .ToListAsync();

        return pagos.Select(MapearADto).ToList();
    }

    public async Task<PagoDto?> ObtenerPagoPorIdAsync(int id)
    {
        var pago = await _context.Pagos
            .Include(p => p.Cuota)
                .ThenInclude(c => c.Membresia)
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
        Cuota? cuota;

        if (dto.IdCuota > 0)
        {
            cuota = await _context.Cuotas
                .Include(c => c.Membresia)
                    .ThenInclude(m => m.Socio)
                        .ThenInclude(s => s.Persona)
                .Include(c => c.Membresia.MembresiaActividades)
                    .ThenInclude(ma => ma.Actividad)
                .Include(c => c.Pagos)
                .FirstOrDefaultAsync(c => c.Id == dto.IdCuota && c.FechaEliminacion == null);
        }
        else if (dto.IdMembresia > 0)
        {
            cuota = await _context.Cuotas
                .Include(c => c.Membresia)
                    .ThenInclude(m => m.Socio)
                        .ThenInclude(s => s.Persona)
                .Include(c => c.Membresia.MembresiaActividades)
                    .ThenInclude(ma => ma.Actividad)
                .Include(c => c.Pagos)
                .Where(c => c.IdMembresia == dto.IdMembresia && c.Estado != CuotaEstado.Pagada && c.FechaEliminacion == null)
                .OrderBy(c => c.NumeroCuota)
                .FirstOrDefaultAsync();
        }
        else
        {
            throw new InvalidOperationException("Debe especificar IdCuota o IdMembresia");
        }

        if (cuota == null)
            throw new InvalidOperationException("Cuota no encontrada");

        if (cuota.Estado == CuotaEstado.Pagada)
            throw new InvalidOperationException("La cuota ya está pagada");

        var metodoPago = await _context.MetodosPago.FindAsync(dto.IdMetodoPago);
        if (metodoPago == null)
            throw new InvalidOperationException("Método de pago no encontrado");

        if (dto.Monto <= 0)
            throw new InvalidOperationException("El monto debe ser mayor a cero");

        var pago = new Pago
        {
            IdCuota = cuota.Id,
            IdMetodoPago = dto.IdMetodoPago,
            IdUsuarioProcesa = idUsuario,
            Monto = dto.Monto,
            FechaPago = dto.FechaPago ?? DateTime.Now,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        await _cuotaService.MarcarCuotaPagadaAsync(cuota.Id);
        await _membresiaService.ActualizarEstadoDespuesDePagoAsync(cuota.IdMembresia);

        return await GenerarComprobanteAsync(pago.Id);
    }

    public async Task<ComprobantePagoDto> GenerarComprobanteAsync(int idPago)
    {
        var pago = await _context.Pagos
            .Include(p => p.Cuota)
                .ThenInclude(c => c.Membresia)
                    .ThenInclude(m => m.Socio)
                        .ThenInclude(s => s.Persona)
            .Include(p => p.Cuota.Membresia.MembresiaActividades)
                .ThenInclude(ma => ma.Actividad)
            .Include(p => p.Cuota.Membresia.Cuotas)
                .ThenInclude(c => c.Pagos)
            .Include(p => p.MetodoPago)
            .Include(p => p.UsuarioProcesa)
                .ThenInclude(u => u!.Persona)
            .FirstOrDefaultAsync(p => p.Id == idPago && p.FechaEliminacion == null);

        if (pago == null)
            throw new InvalidOperationException("Pago no encontrado");

        var membresia = pago.Cuota.Membresia;
        var totalCargado = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento);

        var totalPagadoAntes = membresia.Cuotas
            .Where(c => c.FechaEliminacion == null)
            .SelectMany(c => c.Pagos)
            .Where(p => p.FechaEliminacion == null && p.Id != idPago)
            .Sum(p => p.Monto);

        var totalPagadoDespues = membresia.Cuotas
            .Where(c => c.FechaEliminacion == null)
            .SelectMany(c => c.Pagos)
            .Where(p => p.FechaEliminacion == null)
            .Sum(p => p.Monto);

        var nuevoSaldo = totalCargado - totalPagadoDespues;
        var numeroComprobante = $"PAG-{idPago:D6}-{pago.FechaPago.Year}";
        var periodoMembresia = $"{membresia.FechaInicio:dd/MM/yyyy} - {membresia.FechaFin:dd/MM/yyyy}";

        return new ComprobantePagoDto
        {
            IdPago = pago.Id,
            NumeroComprobante = numeroComprobante,
            FechaEmision = pago.FechaCreacion,
            NumeroSocio = membresia.Socio.NumeroSocio,
            NombreSocio = membresia.Socio.Persona.NombreCompleto,
            PeriodoMembresia = periodoMembresia,
            TotalMembresia = totalCargado,
            TotalPagadoAntes = totalPagadoAntes,
            MontoPago = pago.Monto,
            NuevoSaldo = nuevoSaldo,
            EstaPaga = nuevoSaldo <= 0,
            MetodoPago = pago.MetodoPago.Nombre,
            FechaPago = pago.FechaPago,
            UsuarioProcesa = pago.UsuarioProcesa?.Persona?.NombreCompleto ?? "Sistema",
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
            return false;

        var idCuota = pago.IdCuota;

        pago.FechaEliminacion = DateTime.Now;
        await _context.SaveChangesAsync();

        await _cuotaService.RevertirCuotaPagadaAsync(idCuota);

        return true;
    }

    public async Task<decimal> ObtenerTotalRecaudadoAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var query = _context.Pagos
            .Where(p => p.FechaEliminacion == null)
            .AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(p => p.FechaPago >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(p => p.FechaPago <= fechaHasta.Value);

        return await query.SumAsync(p => p.Monto);
    }

    public async Task<List<MetodoPagoDto>> ObtenerMetodosPagoAsync()
    {
        var metodos = await _context.MetodosPago.ToListAsync();
        return metodos.Select(m => new MetodoPagoDto
        {
            Id = m.Id,
            Nombre = m.Nombre,
            EstaActivo = true
        }).ToList();
    }

    public async Task<EstadisticasPagosDto> ObtenerEstadisticasPagosAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        var hoy = DateTime.Today;
        var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
        var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

        var queryPeriodo = _context.Pagos
            .Include(p => p.MetodoPago)
            .Where(p => p.FechaEliminacion == null)
            .AsQueryable();

        if (fechaDesde.HasValue)
            queryPeriodo = queryPeriodo.Where(p => p.FechaPago >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            queryPeriodo = queryPeriodo.Where(p => p.FechaPago <= fechaHasta.Value);

        var pagosPeriodo = await queryPeriodo.ToListAsync();
        var totalRecaudado = pagosPeriodo.Sum(p => p.Monto);

        var totalPagosHoy = await _context.Pagos
            .Where(p => p.FechaEliminacion == null && p.FechaPago.Date == hoy)
            .SumAsync(p => p.Monto);

        var totalPagosMes = await _context.Pagos
            .Where(p => p.FechaEliminacion == null &&
                        p.FechaPago >= primerDiaMes &&
                        p.FechaPago <= ultimoDiaMes)
            .SumAsync(p => p.Monto);

        // Deuda pendiente: cuotas no pagadas
        var totalPendiente = await _context.Cuotas
            .Where(c => c.FechaEliminacion == null && c.Estado != CuotaEstado.Pagada)
            .SumAsync(c => c.Monto);

        var pagosPorMetodo = pagosPeriodo
            .GroupBy(p => p.MetodoPago.Nombre)
            .Select(g => new PagoPorMetodoDto
            {
                Metodo = g.Key,
                Total = g.Sum(p => p.Monto),
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var pagosPorDia = pagosPeriodo
            .GroupBy(p => p.FechaPago.Date)
            .Select(g => new PagoPorDiaDto
            {
                Fecha = g.Key,
                Total = g.Sum(p => p.Monto),
                Cantidad = g.Count()
            })
            .OrderBy(x => x.Fecha)
            .ToList();

        return new EstadisticasPagosDto
        {
            TotalRecaudado = totalRecaudado,
            TotalPagosHoy = totalPagosHoy,
            TotalPagosMes = totalPagosMes,
            TotalPendiente = totalPendiente,
            PagosPorMetodo = pagosPorMetodo,
            PagosPorDia = pagosPorDia
        };
    }

    private static PagoDto MapearADto(Pago pago)
    {
        var membresia = pago.Cuota.Membresia;
        var periodoMembresia = $"{membresia.FechaInicio:dd/MM/yyyy} - {membresia.FechaFin:dd/MM/yyyy}";

        return new PagoDto
        {
            Id = pago.Id,
            IdCuota = pago.IdCuota,
            NumeroCuota = pago.Cuota.NumeroCuota,
            IdMembresia = membresia.Id,
            PeriodoMembresia = periodoMembresia,
            IdSocio = membresia.IdSocio,
            NumeroSocio = membresia.Socio.NumeroSocio,
            NombreSocio = membresia.Socio.Persona.NombreCompleto,
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
