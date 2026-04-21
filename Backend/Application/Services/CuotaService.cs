using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface ICuotaService
{
    Task<List<CuotaDto>> ObtenerCuotasAsync(FiltrosCuotasDto? filtros = null);
    Task<CuotaDto?> ObtenerCuotaPorIdAsync(int id);
    Task<List<CuotaDto>> ObtenerCuotasPorMembresiaAsync(int idMembresia);
    Task<List<CuotaDto>> ObtenerCuotasPorSocioAsync(int idSocio);
    Task<List<MorosoDto>> ObtenerMorososAsync();
    Task<ResumenCuotasDto> ObtenerResumenAsync();
    Task<List<CuotaDto>> GenerarCuotasParaMembresiaAsync(int idMembresia);
    Task<int> ActualizarEstadosVencidosAsync();
    Task MarcarCuotaPagadaAsync(int idCuota);
    Task RevertirCuotaPagadaAsync(int idCuota);
}

public class CuotaService : ICuotaService
{
    private readonly ClubDbContext _context;

    public CuotaService(ClubDbContext context)
    {
        _context = context;
    }

    public async Task<List<CuotaDto>> ObtenerCuotasAsync(FiltrosCuotasDto? filtros = null)
    {
        filtros ??= new FiltrosCuotasDto();

        var query = _context.Cuotas
            .Include(c => c.Membresia)
                .ThenInclude(m => m.Socio)
                    .ThenInclude(s => s.Persona)
            .Include(c => c.Pagos)
            .Where(c => c.FechaEliminacion == null)
            .AsQueryable();

        if (filtros.IdMembresia.HasValue)
            query = query.Where(c => c.IdMembresia == filtros.IdMembresia.Value);

        if (filtros.IdSocio.HasValue)
            query = query.Where(c => c.Membresia.IdSocio == filtros.IdSocio.Value);

        if (!string.IsNullOrWhiteSpace(filtros.Estado))
            query = query.Where(c => c.Estado == filtros.Estado);

        if (filtros.SoloVencidas == true)
            query = query.Where(c => c.Estado == CuotaEstado.Vencida);

        if (filtros.FechaVencimientoDesde.HasValue)
            query = query.Where(c => c.FechaVencimiento >= filtros.FechaVencimientoDesde.Value);

        if (filtros.FechaVencimientoHasta.HasValue)
            query = query.Where(c => c.FechaVencimiento <= filtros.FechaVencimientoHasta.Value);

        var cuotas = await query
            .OrderBy(c => c.FechaVencimiento)
            .ThenBy(c => c.NumeroCuota)
            .Skip((filtros.Page - 1) * filtros.PageSize)
            .Take(filtros.PageSize)
            .ToListAsync();

        return cuotas.Select(MapearADto).ToList();
    }

    public async Task<CuotaDto?> ObtenerCuotaPorIdAsync(int id)
    {
        var cuota = await _context.Cuotas
            .Include(c => c.Membresia)
                .ThenInclude(m => m.Socio)
                    .ThenInclude(s => s.Persona)
            .Include(c => c.Pagos)
            .Where(c => c.Id == id && c.FechaEliminacion == null)
            .FirstOrDefaultAsync();

        return cuota == null ? null : MapearADto(cuota);
    }

    public async Task<List<CuotaDto>> ObtenerCuotasPorMembresiaAsync(int idMembresia)
    {
        var cuotas = await _context.Cuotas
            .Include(c => c.Membresia)
                .ThenInclude(m => m.Socio)
                    .ThenInclude(s => s.Persona)
            .Include(c => c.Pagos)
            .Where(c => c.IdMembresia == idMembresia && c.FechaEliminacion == null)
            .OrderBy(c => c.NumeroCuota)
            .ToListAsync();

        return cuotas.Select(MapearADto).ToList();
    }

    public async Task<List<CuotaDto>> ObtenerCuotasPorSocioAsync(int idSocio)
    {
        var cuotas = await _context.Cuotas
            .Include(c => c.Membresia)
                .ThenInclude(m => m.Socio)
                    .ThenInclude(s => s.Persona)
            .Include(c => c.Pagos)
            .Where(c => c.Membresia.IdSocio == idSocio && c.FechaEliminacion == null)
            .OrderByDescending(c => c.FechaVencimiento)
            .ToListAsync();

        return cuotas.Select(MapearADto).ToList();
    }

    public async Task<List<MorosoDto>> ObtenerMorososAsync()
    {
        var cuotasVencidas = await _context.Cuotas
            .Include(c => c.Membresia)
                .ThenInclude(m => m.Socio)
                    .ThenInclude(s => s.Persona)
            .Include(c => c.Pagos)
            .Where(c => c.Estado == CuotaEstado.Vencida && c.FechaEliminacion == null)
            .ToListAsync();

        var morosos = cuotasVencidas
            .GroupBy(c => c.Membresia.IdSocio)
            .Select(g =>
            {
                var socio = g.First().Membresia.Socio;
                var cuotasDto = g.Select(MapearADto).OrderBy(c => c.FechaVencimiento).ToList();
                return new MorosoDto
                {
                    IdSocio = socio.Id,
                    NumeroSocio = socio.NumeroSocio,
                    NombreSocio = socio.Persona.NombreCompleto,
                    Email = socio.Persona.Email,
                    CuotasVencidas = g.Count(),
                    DeudaTotal = g.Sum(c => c.Monto),
                    FechaVencimientoMasTemprana = g.Min(c => c.FechaVencimiento),
                    Cuotas = cuotasDto
                };
            })
            .OrderByDescending(m => m.DeudaTotal)
            .ToList();

        return morosos;
    }

    public async Task<ResumenCuotasDto> ObtenerResumenAsync()
    {
        var cuotas = await _context.Cuotas
            .Where(c => c.FechaEliminacion == null)
            .GroupBy(c => c.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count(), Monto = g.Sum(c => c.Monto) })
            .ToListAsync();

        var pendientes = cuotas.FirstOrDefault(c => c.Estado == CuotaEstado.Pendiente);
        var pagadas = cuotas.FirstOrDefault(c => c.Estado == CuotaEstado.Pagada);
        var vencidas = cuotas.FirstOrDefault(c => c.Estado == CuotaEstado.Vencida);

        var totalMorosos = await _context.Cuotas
            .Where(c => c.Estado == CuotaEstado.Vencida && c.FechaEliminacion == null)
            .Select(c => c.Membresia.IdSocio)
            .Distinct()
            .CountAsync();

        return new ResumenCuotasDto
        {
            TotalCuotas = cuotas.Sum(c => c.Cantidad),
            CuotasPendientes = pendientes?.Cantidad ?? 0,
            CuotasPagadas = pagadas?.Cantidad ?? 0,
            CuotasVencidas = vencidas?.Cantidad ?? 0,
            MontoTotalPendiente = (pendientes?.Monto ?? 0) + (vencidas?.Monto ?? 0),
            MontoTotalVencido = vencidas?.Monto ?? 0,
            TotalMorosos = totalMorosos
        };
    }

    public async Task<List<CuotaDto>> GenerarCuotasParaMembresiaAsync(int idMembresia)
    {
        var membresia = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .Include(m => m.Cuotas)
            .Include(m => m.Socio)
                .ThenInclude(s => s.Persona)
            .FirstOrDefaultAsync(m => m.Id == idMembresia && m.FechaEliminacion == null);

        if (membresia == null)
            throw new InvalidOperationException("Membresía no encontrada");

        if (membresia.Cuotas.Any(c => c.FechaEliminacion == null))
            throw new InvalidOperationException("La membresía ya tiene cuotas generadas. Elimínelas antes de regenerar.");

        var meses = CalcularMeses(membresia.FechaInicio, membresia.FechaFin);
        if (meses == 0)
            throw new InvalidOperationException("El período de la membresía no permite generar cuotas mensuales.");

        var totalCargado = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento);
        var montoPorCuota = Math.Round(totalCargado / meses, 2);

        var cuotas = new List<Cuota>();
        for (int i = 0; i < meses; i++)
        {
            var mesActual = membresia.FechaInicio.AddMonths(i);
            var ultimoDiaMes = new DateTime(mesActual.Year, mesActual.Month,
                DateTime.DaysInMonth(mesActual.Year, mesActual.Month));

            cuotas.Add(new Cuota
            {
                IdMembresia = idMembresia,
                NumeroCuota = i + 1,
                Monto = montoPorCuota,
                FechaVencimiento = ultimoDiaMes,
                Estado = CuotaEstado.Pendiente,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            });
        }

        _context.Cuotas.AddRange(cuotas);
        await _context.SaveChangesAsync();

        return (await ObtenerCuotasPorMembresiaAsync(idMembresia));
    }

    public async Task<int> ActualizarEstadosVencidosAsync()
    {
        var hoy = DateTime.Today;
        var cuotasAVencer = await _context.Cuotas
            .Where(c => c.Estado == CuotaEstado.Pendiente &&
                        c.FechaVencimiento < hoy &&
                        c.FechaEliminacion == null)
            .ToListAsync();

        foreach (var cuota in cuotasAVencer)
        {
            cuota.Estado = CuotaEstado.Vencida;
        }

        if (cuotasAVencer.Any())
            await _context.SaveChangesAsync();

        return cuotasAVencer.Count;
    }

    public async Task MarcarCuotaPagadaAsync(int idCuota)
    {
        var cuota = await _context.Cuotas.FindAsync(idCuota);
        if (cuota == null || cuota.FechaEliminacion != null)
            throw new InvalidOperationException("Cuota no encontrada");

        cuota.Estado = CuotaEstado.Pagada;
        await _context.SaveChangesAsync();
    }

    public async Task RevertirCuotaPagadaAsync(int idCuota)
    {
        var cuota = await _context.Cuotas.FindAsync(idCuota);
        if (cuota == null || cuota.FechaEliminacion != null)
            throw new InvalidOperationException("Cuota no encontrada");

        var hoy = DateTime.Today;
        cuota.Estado = cuota.FechaVencimiento < hoy ? CuotaEstado.Vencida : CuotaEstado.Pendiente;
        await _context.SaveChangesAsync();
    }

    private static int CalcularMeses(DateTime inicio, DateTime fin)
    {
        return (fin.Year - inicio.Year) * 12 + fin.Month - inicio.Month +
               (fin.Day >= inicio.Day ? 1 : 0);
    }

    private static CuotaDto MapearADto(Cuota cuota)
    {
        var hoy = DateTime.Today;
        var diasVencida = cuota.Estado == CuotaEstado.Vencida
            ? (int)(hoy - cuota.FechaVencimiento.Date).TotalDays
            : 0;

        return new CuotaDto
        {
            Id = cuota.Id,
            IdMembresia = cuota.IdMembresia,
            PeriodoMembresia = $"{cuota.Membresia.FechaInicio:dd/MM/yyyy} - {cuota.Membresia.FechaFin:dd/MM/yyyy}",
            IdSocio = cuota.Membresia.IdSocio,
            NumeroSocio = cuota.Membresia.Socio.NumeroSocio,
            NombreSocio = cuota.Membresia.Socio.Persona.NombreCompleto,
            NumeroCuota = cuota.NumeroCuota,
            Monto = cuota.Monto,
            FechaVencimiento = cuota.FechaVencimiento,
            Estado = cuota.Estado,
            DiasVencida = diasVencida,
            FechaCreacion = cuota.FechaCreacion
        };
    }
}
