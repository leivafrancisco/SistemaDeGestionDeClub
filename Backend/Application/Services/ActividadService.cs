using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IActividadService
{
    Task<List<ActividadDto>> ObtenerTodasAsync();
    Task<ActividadDto?> ObtenerPorIdAsync(int id);
    Task<ActividadDto> CrearAsync(CrearActividadDto dto);
    Task<ActividadDto> ActualizarAsync(int id, ActualizarActividadDto dto);
    Task<bool> EliminarAsync(int id);
}

public class ActividadService : IActividadService
{
    private readonly ClubDbContext _context;

    public ActividadService(ClubDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActividadDto>> ObtenerTodasAsync()
    {
        var actividades = await _context.Actividades
            .Where(a => a.FechaEliminacion == null)
            .OrderBy(a => a.Nombre)
            .Select(a => new ActividadDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Precio = a.Precio,
                EsCuotaBase = a.EsCuotaBase,
                FechaCreacion = a.FechaCreacion
            })
            .ToListAsync();

        return actividades;
    }

    public async Task<ActividadDto?> ObtenerPorIdAsync(int id)
    {
        var actividad = await _context.Actividades
            .Where(a => a.Id == id && a.FechaEliminacion == null)
            .Select(a => new ActividadDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Precio = a.Precio,
                EsCuotaBase = a.EsCuotaBase,
                FechaCreacion = a.FechaCreacion
            })
            .FirstOrDefaultAsync();

        return actividad;
    }

    public async Task<ActividadDto> CrearAsync(CrearActividadDto dto)
    {
        // Validar que el nombre no exista
        if (await _context.Actividades.AnyAsync(a => a.Nombre == dto.Nombre && a.FechaEliminacion == null))
        {
            throw new InvalidOperationException("Ya existe una actividad con este nombre");
        }

        // Validar precio
        if (dto.Precio < 0)
        {
            throw new InvalidOperationException("El precio no puede ser negativo");
        }

        var actividad = new Actividad
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            EsCuotaBase = dto.EsCuotaBase,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        _context.Actividades.Add(actividad);
        await _context.SaveChangesAsync();

        return new ActividadDto
        {
            Id = actividad.Id,
            Nombre = actividad.Nombre,
            Descripcion = actividad.Descripcion,
            Precio = actividad.Precio,
            EsCuotaBase = actividad.EsCuotaBase,
            FechaCreacion = actividad.FechaCreacion
        };
    }

    public async Task<ActividadDto> ActualizarAsync(int id, ActualizarActividadDto dto)
    {
        var actividad = await _context.Actividades
            .FirstOrDefaultAsync(a => a.Id == id && a.FechaEliminacion == null);

        if (actividad == null)
        {
            throw new InvalidOperationException("Actividad no encontrada");
        }

        // Validar nombre único (excepto la misma actividad)
        if (await _context.Actividades.AnyAsync(a => a.Nombre == dto.Nombre && a.Id != id && a.FechaEliminacion == null))
        {
            throw new InvalidOperationException("Ya existe una actividad con este nombre");
        }

        // Validar precio
        if (dto.Precio < 0)
        {
            throw new InvalidOperationException("El precio no puede ser negativo");
        }

        actividad.Nombre = dto.Nombre;
        actividad.Descripcion = dto.Descripcion;
        actividad.Precio = dto.Precio;
        actividad.EsCuotaBase = dto.EsCuotaBase;

        await _context.SaveChangesAsync();

        return new ActividadDto
        {
            Id = actividad.Id,
            Nombre = actividad.Nombre,
            Descripcion = actividad.Descripcion,
            Precio = actividad.Precio,
            EsCuotaBase = actividad.EsCuotaBase,
            FechaCreacion = actividad.FechaCreacion
        };
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var actividad = await _context.Actividades.FindAsync(id);

        if (actividad == null || actividad.FechaEliminacion != null)
        {
            return false;
        }

        actividad.FechaEliminacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }
}
