using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IMembresiaService
{
    Task<List<MembresiaDto>> ObtenerTodasMembresiasAsync(FiltrosMembresiasDto? filtros = null);
    Task<MembresiaDto?> ObtenerMembresiaPorIdAsync(int id);
    Task<MembresiaDto> CrearMembresiaAsync(CrearMembresiaDto dto);
    Task<MembresiaDto> ActualizarMembresiaAsync(int id, ActualizarMembresiaDto dto);
    Task<bool> EliminarMembresiaAsync(int id);
    Task<int> ContarTotalMembresiasAsync();
    Task<MembresiaDto> AsignarActividadAsync(AsignarActividadDto dto);
    Task<MembresiaDto> RemoverActividadAsync(RemoverActividadDto dto);
    Task ActualizarEstadoDespuesDePagoAsync(int idMembresia);
}

public class MembresiaService : IMembresiaService
{
    private readonly ClubDbContext _context;

    public MembresiaService(ClubDbContext context)
    {
        _context = context;
    }

    public async Task<List<MembresiaDto>> ObtenerTodasMembresiasAsync(FiltrosMembresiasDto? filtros = null)
    {
        filtros ??= new FiltrosMembresiasDto();

        var query = _context.Membresias
            .Include(m => m.Socio)
                .ThenInclude(s => s.Persona)
            .Include(m => m.MembresiaActividades)
                .ThenInclude(ma => ma.Actividad)
            .Include(m => m.Cuotas)
                .ThenInclude(c => c.Pagos)
            .Where(m => m.FechaEliminacion == null)
            .AsQueryable();

        if (filtros.IdSocio.HasValue)
            query = query.Where(m => m.IdSocio == filtros.IdSocio.Value);

        if (filtros.FechaDesde.HasValue)
            query = query.Where(m => m.FechaInicio >= filtros.FechaDesde.Value);

        if (filtros.FechaHasta.HasValue)
            query = query.Where(m => m.FechaFin <= filtros.FechaHasta.Value);

        if (!string.IsNullOrWhiteSpace(filtros.Search))
        {
            var searchLower = filtros.Search.Trim().ToLower();
            query = query.Where(m =>
                m.Socio.Persona.Nombre.ToLower().Contains(searchLower) ||
                m.Socio.Persona.Apellido.ToLower().Contains(searchLower) ||
                m.Socio.NumeroSocio.ToLower().Contains(searchLower) ||
                (m.Socio.Persona.Nombre + " " + m.Socio.Persona.Apellido).ToLower().Contains(searchLower)
            );
        }

        if (!string.IsNullOrWhiteSpace(filtros.EstadoVigencia))
        {
            var hoy = DateTime.Today;
            var proximosSieteDias = hoy.AddDays(7);

            switch (filtros.EstadoVigencia.ToLower())
            {
                case "vigentes":
                    query = query.Where(m => m.FechaFin > proximosSieteDias);
                    break;
                case "vencidas":
                    query = query.Where(m => m.FechaFin < hoy);
                    break;
                case "proximas_vencer":
                    query = query.Where(m => m.FechaFin >= hoy && m.FechaFin <= proximosSieteDias);
                    break;
            }
        }

        var membresias = await query
            .OrderByDescending(m => m.FechaInicio)
            .Skip((filtros.Page - 1) * filtros.PageSize)
            .Take(filtros.PageSize)
            .ToListAsync();

        var membresiasDtos = membresias.Select(MapearADto).ToList();

        if (filtros.SoloImpagas == true)
            membresiasDtos = membresiasDtos.Where(m => !m.EstaPaga).ToList();

        return membresiasDtos;
    }

    public async Task<MembresiaDto?> ObtenerMembresiaPorIdAsync(int id)
    {
        var membresia = await _context.Membresias
            .Include(m => m.Socio)
                .ThenInclude(s => s.Persona)
            .Include(m => m.MembresiaActividades)
                .ThenInclude(ma => ma.Actividad)
            .Include(m => m.Cuotas)
                .ThenInclude(c => c.Pagos)
            .Where(m => m.Id == id && m.FechaEliminacion == null)
            .FirstOrDefaultAsync();

        return membresia == null ? null : MapearADto(membresia);
    }

    public async Task<MembresiaDto> CrearMembresiaAsync(CrearMembresiaDto dto)
    {
        var socio = await _context.Socios
            .Include(s => s.Persona)
            .FirstOrDefaultAsync(s => s.Id == dto.IdSocio && s.FechaEliminacion == null);

        if (socio == null)
            throw new InvalidOperationException("Socio no encontrado");

        if (dto.FechaFin <= dto.FechaInicio)
            throw new InvalidOperationException("La fecha de fin debe ser posterior a la fecha de inicio");

        var existeMembresia = await _context.Membresias.AnyAsync(m =>
            m.IdSocio == dto.IdSocio &&
            m.FechaEliminacion == null &&
            ((m.FechaInicio <= dto.FechaFin && m.FechaFin >= dto.FechaInicio) ||
             (dto.FechaInicio <= m.FechaFin && dto.FechaFin >= m.FechaInicio)));

        if (existeMembresia)
            throw new InvalidOperationException("Ya existe una membresía para este socio que se solapa con las fechas especificadas");

        if (dto.IdsActividades.Any())
        {
            var actividadesExistentes = await _context.Actividades
                .Where(a => dto.IdsActividades.Contains(a.Id) && a.FechaEliminacion == null)
                .CountAsync();

            if (actividadesExistentes != dto.IdsActividades.Count)
                throw new InvalidOperationException("Una o más actividades no existen");
        }

        if (dto.CostoTotal <= 0)
            throw new InvalidOperationException("El costo total debe ser mayor a cero");

        if (dto.Monto <= 0)
            throw new InvalidOperationException("El monto del pago debe ser mayor a cero");

        var metodoPago = await _context.MetodosPago.FindAsync(dto.IdMetodoPago);
        if (metodoPago == null)
            throw new InvalidOperationException("El método de pago no existe");

        var membresia = new Membresia
        {
            IdSocio = dto.IdSocio,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            CostoTotal = dto.CostoTotal,
            Estado = MembresiaEstado.Activa,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Membresias.Add(membresia);
        await _context.SaveChangesAsync();

        Console.WriteLine($"[DEBUG] Membresía creada con ID: {membresia.Id}");

        // Agregar actividades
        if (dto.IdsActividades.Any())
        {
            var actividades = await _context.Actividades
                .Where(a => dto.IdsActividades.Contains(a.Id) && a.FechaEliminacion == null)
                .ToListAsync();

            foreach (var actividad in actividades)
            {
                _context.MembresiaActividades.Add(new MembresiaActividad
                {
                    IdMembresia = membresia.Id,
                    IdActividad = actividad.Id,
                    PrecioAlMomento = actividad.Precio
                });
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"[DEBUG] {actividades.Count} actividades guardadas");
        }

        // Generar cuotas ANTES del pago inicial (el pago se vincula a la cuota #1)
        var cuotas = await GenerarCuotasAsync(membresia);

        // Crear el pago inicial vinculado a la primera cuota
        if (cuotas.Any())
        {
            var primeraCuota = cuotas.OrderBy(c => c.NumeroCuota).First();

            var pago = new Pago
            {
                IdCuota = primeraCuota.Id,
                IdMetodoPago = dto.IdMetodoPago,
                IdUsuarioProcesa = dto.IdUsuarioProcesa,
                Monto = dto.Monto,
                FechaPago = DateTime.Now,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            primeraCuota.Estado = CuotaEstado.Pagada;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[DEBUG] Pago inicial ID: {pago.Id} vinculado a cuota #1 (ID: {primeraCuota.Id})");
        }

        return (await ObtenerMembresiaPorIdAsync(membresia.Id))!;
    }

    private async Task<List<Cuota>> GenerarCuotasAsync(Membresia membresia)
    {
        var meses = CalcularMeses(membresia.FechaInicio, membresia.FechaFin);
        if (meses == 0) return new List<Cuota>();

        var membresiaConActividades = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .FirstOrDefaultAsync(m => m.Id == membresia.Id);

        var totalCargado = membresiaConActividades?.MembresiaActividades.Sum(ma => ma.PrecioAlMomento) ?? membresia.CostoTotal;
        var montoPorCuota = Math.Round(totalCargado / meses, 2);

        var cuotas = new List<Cuota>();
        for (int i = 0; i < meses; i++)
        {
            var mesActual = membresia.FechaInicio.AddMonths(i);
            var ultimoDiaMes = new DateTime(mesActual.Year, mesActual.Month,
                DateTime.DaysInMonth(mesActual.Year, mesActual.Month));

            var cuota = new Cuota
            {
                IdMembresia = membresia.Id,
                NumeroCuota = i + 1,
                Monto = montoPorCuota,
                FechaVencimiento = ultimoDiaMes,
                Estado = CuotaEstado.Pendiente,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };

            _context.Cuotas.Add(cuota);
            cuotas.Add(cuota);
        }

        await _context.SaveChangesAsync();
        Console.WriteLine($"[DEBUG] Generadas {meses} cuotas para membresía {membresia.Id}");

        return cuotas;
    }

    private static int CalcularMeses(DateTime inicio, DateTime fin)
    {
        return (fin.Year - inicio.Year) * 12 + fin.Month - inicio.Month +
               (fin.Day >= inicio.Day ? 1 : 0);
    }

    public async Task<MembresiaDto> ActualizarMembresiaAsync(int id, ActualizarMembresiaDto dto)
    {
        var membresia = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .Include(m => m.Cuotas)
                .ThenInclude(c => c.Pagos)
            .Include(m => m.Socio)
                .ThenInclude(s => s.Persona)
            .FirstOrDefaultAsync(m => m.Id == id && m.FechaEliminacion == null);

        if (membresia == null)
            throw new InvalidOperationException("Membresía no encontrada");

        Console.WriteLine($"[DEBUG ActualizarMembresia] ID: {id}");

        DateTime nuevaFechaInicio = dto.FechaInicio ?? membresia.FechaInicio;
        DateTime nuevaFechaFin = dto.FechaFin ?? membresia.FechaFin;

        if (nuevaFechaFin <= nuevaFechaInicio)
            throw new InvalidOperationException("La fecha de fin debe ser posterior a la fecha de inicio");

        var haySolapamiento = await _context.Membresias
            .Where(m => m.IdSocio == membresia.IdSocio && m.Id != id && m.FechaEliminacion == null)
            .AnyAsync(m => (nuevaFechaInicio >= m.FechaInicio && nuevaFechaInicio < m.FechaFin) ||
                           (nuevaFechaFin > m.FechaInicio && nuevaFechaFin <= m.FechaFin) ||
                           (nuevaFechaInicio <= m.FechaInicio && nuevaFechaFin >= m.FechaFin));

        if (haySolapamiento)
            throw new InvalidOperationException("Las fechas se solapan con otra membresía existente del socio");

        membresia.FechaInicio = nuevaFechaInicio;
        membresia.FechaFin = nuevaFechaFin;

        if (dto.IdsActividades != null)
        {
            if (dto.IdsActividades.Any())
            {
                var actividadesExistentes = await _context.Actividades
                    .Where(a => dto.IdsActividades.Contains(a.Id) && a.FechaEliminacion == null)
                    .CountAsync();

                if (actividadesExistentes != dto.IdsActividades.Count)
                    throw new InvalidOperationException("Una o más actividades no existen o están inactivas");
            }

            var totalPagado = membresia.Cuotas
                .Where(c => c.FechaEliminacion == null)
                .SelectMany(c => c.Pagos)
                .Where(p => p.FechaEliminacion == null)
                .Sum(p => p.Monto);

            Console.WriteLine($"[DEBUG ActualizarMembresia] Total pagado: {totalPagado}");

            var actividadesAnteriores = membresia.MembresiaActividades.ToList();
            _context.MembresiaActividades.RemoveRange(actividadesAnteriores);

            if (dto.IdsActividades.Any())
            {
                var actividades = await _context.Actividades
                    .Where(a => dto.IdsActividades.Contains(a.Id) && a.FechaEliminacion == null)
                    .ToListAsync();

                foreach (var actividad in actividades)
                {
                    _context.MembresiaActividades.Add(new MembresiaActividad
                    {
                        IdMembresia = membresia.Id,
                        IdActividad = actividad.Id,
                        PrecioAlMomento = actividad.Precio
                    });
                }

                membresia.CostoTotal = actividades.Sum(a => a.Precio);
            }
        }

        membresia.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return (await ObtenerMembresiaPorIdAsync(membresia.Id))!;
    }

    public async Task<bool> EliminarMembresiaAsync(int id)
    {
        var membresia = await _context.Membresias
            .Include(m => m.Cuotas)
                .ThenInclude(c => c.Pagos)
            .FirstOrDefaultAsync(m => m.Id == id && m.FechaEliminacion == null);

        if (membresia == null)
            return false;

        var tienePagos = membresia.Cuotas
            .SelectMany(c => c.Pagos)
            .Any(p => p.FechaEliminacion == null);

        if (tienePagos)
            throw new InvalidOperationException("No se puede eliminar una membresía que tiene pagos registrados");

        membresia.FechaEliminacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> ContarTotalMembresiasAsync()
    {
        return await _context.Membresias
            .Where(m => m.FechaEliminacion == null)
            .CountAsync();
    }

    public async Task<MembresiaDto> AsignarActividadAsync(AsignarActividadDto dto)
    {
        var membresia = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .FirstOrDefaultAsync(m => m.Id == dto.IdMembresia && m.FechaEliminacion == null);

        if (membresia == null)
            throw new InvalidOperationException("Membresía no encontrada");

        var actividad = await _context.Actividades
            .FirstOrDefaultAsync(a => a.Id == dto.IdActividad && a.FechaEliminacion == null);

        if (actividad == null)
            throw new InvalidOperationException("Actividad no encontrada");

        var yaAsignada = membresia.MembresiaActividades.Any(ma => ma.IdActividad == dto.IdActividad);
        if (yaAsignada)
            throw new InvalidOperationException("La actividad ya está asignada a esta membresía");

        _context.MembresiaActividades.Add(new MembresiaActividad
        {
            IdMembresia = membresia.Id,
            IdActividad = actividad.Id,
            PrecioAlMomento = actividad.Precio
        });

        membresia.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return (await ObtenerMembresiaPorIdAsync(membresia.Id))!;
    }

    public async Task<MembresiaDto> RemoverActividadAsync(RemoverActividadDto dto)
    {
        var membresia = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .Include(m => m.Cuotas)
                .ThenInclude(c => c.Pagos)
            .FirstOrDefaultAsync(m => m.Id == dto.IdMembresia && m.FechaEliminacion == null);

        if (membresia == null)
            throw new InvalidOperationException("Membresía no encontrada");

        var tienePagos = membresia.Cuotas
            .SelectMany(c => c.Pagos)
            .Any(p => p.FechaEliminacion == null);

        if (tienePagos)
            throw new InvalidOperationException("No se puede remover actividades de una membresía que ya tiene pagos registrados");

        var membresiaActividad = membresia.MembresiaActividades
            .FirstOrDefault(ma => ma.IdActividad == dto.IdActividad);

        if (membresiaActividad == null)
            throw new InvalidOperationException("La actividad no está asignada a esta membresía");

        _context.MembresiaActividades.Remove(membresiaActividad);
        membresia.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return (await ObtenerMembresiaPorIdAsync(membresia.Id))!;
    }

    public async Task ActualizarEstadoDespuesDePagoAsync(int idMembresia)
    {
        var membresia = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .Include(m => m.Cuotas)
                .ThenInclude(c => c.Pagos)
            .FirstOrDefaultAsync(m => m.Id == idMembresia && m.FechaEliminacion == null);

        if (membresia == null) return;

        if (membresia.FechaFin < DateTime.Now)
        {
            membresia.Estado = MembresiaEstado.Vencida;
        }
        else
        {
            var totalCargado = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento);
            var totalPagado = membresia.Cuotas
                .Where(c => c.FechaEliminacion == null)
                .SelectMany(c => c.Pagos)
                .Where(p => p.FechaEliminacion == null)
                .Sum(p => p.Monto);

            membresia.Estado = totalPagado >= totalCargado
                ? MembresiaEstado.Activa
                : MembresiaEstado.PagoPendiente;
        }

        membresia.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    private static MembresiaDto MapearADto(Membresia membresia)
    {
        var totalCargado = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento);
        var totalPagado = membresia.Cuotas
            .Where(c => c.FechaEliminacion == null)
            .SelectMany(c => c.Pagos)
            .Where(p => p.FechaEliminacion == null)
            .Sum(p => p.Monto);

        return new MembresiaDto
        {
            Id = membresia.Id,
            IdSocio = membresia.IdSocio,
            NumeroSocio = membresia.Socio.NumeroSocio,
            NombreSocio = membresia.Socio.Persona.NombreCompleto,
            FechaInicio = membresia.FechaInicio,
            FechaFin = membresia.FechaFin,
            FechaCreacion = membresia.FechaCreacion,
            CostoTotal = membresia.CostoTotal,
            Estado = membresia.Estado,
            TotalCargado = totalCargado,
            TotalPagado = totalPagado,
            Saldo = totalCargado - totalPagado,
            EstaPaga = totalPagado >= totalCargado,
            Actividades = membresia.MembresiaActividades.Select(ma => new ActividadEnMembresiaDto
            {
                IdActividad = ma.IdActividad,
                NombreActividad = ma.Actividad.Nombre,
                PrecioAlMomento = ma.PrecioAlMomento
            }).ToList()
        };
    }
}
