using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IMembresiaService
{
    Task<List<MembresiaDto>> ObtenerTodosAsync(FiltrosMembresiasDto? filtros = null);
    Task<MembresiaDto?> ObtenerPorIdAsync(int id);
    Task<MembresiaDto> CrearAsync(CrearMembresiaDto dto);
    Task<MembresiaDto> ActualizarAsync(int id, ActualizarMembresiaDto dto);
    Task<bool> EliminarAsync(int id);
    Task<int> ContarTotalAsync();
    Task<MembresiaDto> AsignarActividadAsync(AsignarActividadDto dto);
    Task<MembresiaDto> RemoverActividadAsync(RemoverActividadDto dto);
}

public class MembresiaService : IMembresiaService
{
    private readonly ClubDbContext _context;

    public MembresiaService(ClubDbContext context)
    {
        _context = context;
    }

    public async Task<List<MembresiaDto>> ObtenerTodosAsync(FiltrosMembresiasDto? filtros = null)
    {
        filtros ??= new FiltrosMembresiasDto();

        var query = _context.Membresias
            .Include(m => m.Socio)
                .ThenInclude(s => s.Persona)
            .Include(m => m.MembresiaActividades)
                .ThenInclude(ma => ma.Actividad)
            .Include(m => m.Pagos)
            .Where(m => m.FechaEliminacion == null)
            .AsQueryable();

        // Filtros
        if (filtros.IdSocio.HasValue)
        {
            query = query.Where(m => m.IdSocio == filtros.IdSocio.Value);
        }

        if (filtros.FechaDesde.HasValue)
        {
            query = query.Where(m => m.FechaInicio >= filtros.FechaDesde.Value);
        }

        if (filtros.FechaHasta.HasValue)
        {
            query = query.Where(m => m.FechaFin <= filtros.FechaHasta.Value);
        }

        var membresias = await query
            .OrderByDescending(m => m.FechaInicio)
            .Skip((filtros.Page - 1) * filtros.PageSize)
            .Take(filtros.PageSize)
            .ToListAsync();

        var membresiasDtos = membresias.Select(m => MapearADto(m)).ToList();

        // Filtrar solo impagas si se solicitó
        if (filtros.SoloImpagas == true)
        {
            membresiasDtos = membresiasDtos.Where(m => !m.EstaPaga).ToList();
        }

        return membresiasDtos;
    }

    public async Task<MembresiaDto?> ObtenerPorIdAsync(int id)
    {
        var membresia = await _context.Membresias
            .Include(m => m.Socio)
                .ThenInclude(s => s.Persona)
            .Include(m => m.MembresiaActividades)
                .ThenInclude(ma => ma.Actividad)
            .Include(m => m.Pagos)
            .Where(m => m.Id == id && m.FechaEliminacion == null)
            .FirstOrDefaultAsync();

        return membresia == null ? null : MapearADto(membresia);
    }

    public async Task<MembresiaDto> CrearAsync(CrearMembresiaDto dto)
    {
        // Validar que el socio existe
        var socio = await _context.Socios
            .Include(s => s.Persona)
            .FirstOrDefaultAsync(s => s.Id == dto.IdSocio && s.FechaEliminacion == null);

        if (socio == null)
        {
            throw new InvalidOperationException("Socio no encontrado");
        }

        // Validar que la fecha de fin sea posterior a la fecha de inicio
        if (dto.FechaFin <= dto.FechaInicio)
        {
            throw new InvalidOperationException("La fecha de fin debe ser posterior a la fecha de inicio");
        }

        // Validar que no exista ya una membresía para este socio que se solape con las fechas
        var existeMembresia = await _context.Membresias.AnyAsync(m =>
            m.IdSocio == dto.IdSocio &&
            m.FechaEliminacion == null &&
            ((m.FechaInicio <= dto.FechaFin && m.FechaFin >= dto.FechaInicio) ||
             (dto.FechaInicio <= m.FechaFin && dto.FechaFin >= m.FechaInicio)));

        if (existeMembresia)
        {
            throw new InvalidOperationException($"Ya existe una membresía para este socio que se solapa con las fechas especificadas");
        }

        // Validar que las actividades existan
        if (dto.IdsActividades.Any())
        {
            var actividadesExistentes = await _context.Actividades
                .Where(a => dto.IdsActividades.Contains(a.Id) && a.FechaEliminacion == null)
                .CountAsync();

            if (actividadesExistentes != dto.IdsActividades.Count)
            {
                throw new InvalidOperationException("Una o más actividades no existen");
            }
        }

        // Validar que el costo total sea positivo
        if (dto.CostoTotal <= 0)
        {
            throw new InvalidOperationException("El costo total debe ser mayor a cero");
        }

        // Crear membresía
        var membresia = new Membresia
        {
            IdSocio = dto.IdSocio,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            CostoTotal = dto.CostoTotal,
            Estado = "AL DIA",
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Membresias.Add(membresia);
        await _context.SaveChangesAsync();

        Console.WriteLine($"[DEBUG] Membresía creada con ID: {membresia.Id}");
        Console.WriteLine($"[DEBUG] IdsActividades recibidos: {string.Join(", ", dto.IdsActividades)}");
        Console.WriteLine($"[DEBUG] Cantidad de actividades: {dto.IdsActividades.Count}");

        // Agregar actividades
        if (dto.IdsActividades.Any())
        {
            var actividades = await _context.Actividades
                .Where(a => dto.IdsActividades.Contains(a.Id) && a.FechaEliminacion == null)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Actividades encontradas en DB: {actividades.Count}");

            foreach (var actividad in actividades)
            {
                Console.WriteLine($"[DEBUG] Agregando actividad {actividad.Id} - {actividad.Nombre} - Precio: {actividad.Precio}");

                var membresiaActividad = new MembresiaActividad
                {
                    IdMembresia = membresia.Id,
                    IdActividad = actividad.Id,
                    PrecioAlMomento = actividad.Precio
                };

                _context.MembresiaActividades.Add(membresiaActividad);
            }

            Console.WriteLine($"[DEBUG] Guardando {actividades.Count} actividades en membresia_actividades...");
            await _context.SaveChangesAsync();
            Console.WriteLine($"[DEBUG] Actividades guardadas exitosamente");
        }
        else
        {
            Console.WriteLine("[DEBUG] No se recibieron IdsActividades, no se agregarán actividades");
        }

        // Recargar la membresía con todas sus relaciones
        return (await ObtenerPorIdAsync(membresia.Id))!;
    }

    public async Task<MembresiaDto> ActualizarAsync(int id, ActualizarMembresiaDto dto)
    {
        var membresia = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .FirstOrDefaultAsync(m => m.Id == id && m.FechaEliminacion == null);

        if (membresia == null)
        {
            throw new InvalidOperationException("Membresía no encontrada");
        }

        // Verificar que no haya pagos asociados
        var tienePagos = await _context.Pagos.AnyAsync(p => p.IdMembresia == id && p.FechaEliminacion == null);
        if (tienePagos)
        {
            throw new InvalidOperationException("No se puede modificar una membresía que ya tiene pagos registrados");
        }

        // Validar que las actividades existan
        if (dto.IdsActividades.Any())
        {
            var actividadesExistentes = await _context.Actividades
                .Where(a => dto.IdsActividades.Contains(a.Id) && a.FechaEliminacion == null)
                .CountAsync();

            if (actividadesExistentes != dto.IdsActividades.Count)
            {
                throw new InvalidOperationException("Una o más actividades no existen");
            }
        }

        // Eliminar actividades actuales
        _context.MembresiaActividades.RemoveRange(membresia.MembresiaActividades);

        // Agregar nuevas actividades
        if (dto.IdsActividades.Any())
        {
            var actividades = await _context.Actividades
                .Where(a => dto.IdsActividades.Contains(a.Id) && a.FechaEliminacion == null)
                .ToListAsync();

            foreach (var actividad in actividades)
            {
                var membresiaActividad = new MembresiaActividad
                {
                    IdMembresia = membresia.Id,
                    IdActividad = actividad.Id,
                    PrecioAlMomento = actividad.Precio
                };

                _context.MembresiaActividades.Add(membresiaActividad);
            }
        }

        await _context.SaveChangesAsync();

        // Recargar la membresía con todas sus relaciones
        return (await ObtenerPorIdAsync(membresia.Id))!;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var membresia = await _context.Membresias
            .Include(m => m.Pagos)
            .FirstOrDefaultAsync(m => m.Id == id && m.FechaEliminacion == null);

        if (membresia == null)
        {
            return false;
        }

        // Verificar que no haya pagos asociados
        var tienePagos = membresia.Pagos.Any(p => p.FechaEliminacion == null);
        if (tienePagos)
        {
            throw new InvalidOperationException("No se puede eliminar una membresía que tiene pagos registrados");
        }

        // Soft delete
        membresia.FechaEliminacion = DateTime.Now;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> ContarTotalAsync()
    {
        return await _context.Membresias
            .Where(m => m.FechaEliminacion == null)
            .CountAsync();
    }

    public async Task<MembresiaDto> AsignarActividadAsync(AsignarActividadDto dto)
    {
        // Validar que la membresía existe
        var membresia = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .FirstOrDefaultAsync(m => m.Id == dto.IdMembresia && m.FechaEliminacion == null);

        if (membresia == null)
        {
            throw new InvalidOperationException("Membresía no encontrada");
        }

        // Validar que la actividad existe
        var actividad = await _context.Actividades
            .FirstOrDefaultAsync(a => a.Id == dto.IdActividad && a.FechaEliminacion == null);

        if (actividad == null)
        {
            throw new InvalidOperationException("Actividad no encontrada");
        }

        // Verificar que la actividad no esté ya asignada
        var yaAsignada = membresia.MembresiaActividades
            .Any(ma => ma.IdActividad == dto.IdActividad);

        if (yaAsignada)
        {
            throw new InvalidOperationException("La actividad ya está asignada a esta membresía");
        }

        // Agregar la actividad
        var membresiaActividad = new MembresiaActividad
        {
            IdMembresia = membresia.Id,
            IdActividad = actividad.Id,
            PrecioAlMomento = actividad.Precio
        };

        _context.MembresiaActividades.Add(membresiaActividad);
        membresia.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();

        // Recargar la membresía con todas sus relaciones
        return (await ObtenerPorIdAsync(membresia.Id))!;
    }

    public async Task<MembresiaDto> RemoverActividadAsync(RemoverActividadDto dto)
    {
        // Validar que la membresía existe
        var membresia = await _context.Membresias
            .Include(m => m.MembresiaActividades)
            .Include(m => m.Pagos)
            .FirstOrDefaultAsync(m => m.Id == dto.IdMembresia && m.FechaEliminacion == null);

        if (membresia == null)
        {
            throw new InvalidOperationException("Membresía no encontrada");
        }

        // Validar que la membresía no tenga pagos
        var tienePagos = membresia.Pagos.Any(p => p.FechaEliminacion == null);
        if (tienePagos)
        {
            throw new InvalidOperationException("No se puede remover actividades de una membresía que ya tiene pagos registrados");
        }

        // Buscar la actividad en la membresía
        var membresiaActividad = membresia.MembresiaActividades
            .FirstOrDefault(ma => ma.IdActividad == dto.IdActividad);

        if (membresiaActividad == null)
        {
            throw new InvalidOperationException("La actividad no está asignada a esta membresía");
        }

        // Remover la actividad
        _context.MembresiaActividades.Remove(membresiaActividad);
        membresia.FechaActualizacion = DateTime.Now;
        await _context.SaveChangesAsync();

        // Recargar la membresía con todas sus relaciones
        return (await ObtenerPorIdAsync(membresia.Id))!;
    }

    private MembresiaDto MapearADto(Membresia membresia)
    {
        return new MembresiaDto
        {
            Id = membresia.Id,
            IdSocio = membresia.IdSocio,
            NumeroSocio = membresia.Socio.NumeroSocio,
            NombreSocio = membresia.Socio.Persona.NombreCompleto,
            FechaInicio = membresia.FechaInicio,
            FechaFin = membresia.FechaFin,
            CostoTotal = membresia.CostoTotal,
            Estado = membresia.Estado,
            TotalCargado = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento),
            TotalPagado = membresia.Pagos.Where(p => p.FechaEliminacion == null).Sum(p => p.Monto),
            Saldo = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento) -
                    membresia.Pagos.Where(p => p.FechaEliminacion == null).Sum(p => p.Monto),
            EstaPaga = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento) <=
                       membresia.Pagos.Where(p => p.FechaEliminacion == null).Sum(p => p.Monto),
            Actividades = membresia.MembresiaActividades.Select(ma => new ActividadEnMembresiaDto
            {
                IdActividad = ma.IdActividad,
                NombreActividad = ma.Actividad.Nombre,
                PrecioAlMomento = ma.PrecioAlMomento
            }).ToList()
        };
    }
}
