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
                FechaCreacion = a.FechaCreacion
            })
            .FirstOrDefaultAsync();

        return actividad;
    }

    public async Task<ActividadDto> CrearAsync(CrearActividadDto dto)
    {
        // Validar nombre
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            throw new InvalidOperationException("El nombre es requerido");
        }

        if (dto.Nombre.Length < 3)
        {
            throw new InvalidOperationException("El nombre debe tener al menos 3 caracteres");
        }

        if (dto.Nombre.Length > 100)
        {
            throw new InvalidOperationException("El nombre no puede tener más de 100 caracteres");
        }

        // Validar que el nombre no exista
        if (await _context.Actividades.AnyAsync(a => a.Nombre == dto.Nombre && a.FechaEliminacion == null))
        {
            throw new InvalidOperationException("Ya existe una actividad con este nombre");
        }

        // Validar descripción
        if (!string.IsNullOrWhiteSpace(dto.Descripcion) && dto.Descripcion.Length > 500)
        {
            throw new InvalidOperationException("La descripción no puede tener más de 500 caracteres");
        }

        // Validar precio
        if (dto.Precio < 0)
        {
            throw new InvalidOperationException("El precio no puede ser negativo");
        }

        var actividad = new Actividad
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Precio = dto.Precio,
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

        // Validar nombre
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            throw new InvalidOperationException("El nombre es requerido");
        }

        if (dto.Nombre.Length < 3)
        {
            throw new InvalidOperationException("El nombre debe tener al menos 3 caracteres");
        }

        if (dto.Nombre.Length > 100)
        {
            throw new InvalidOperationException("El nombre no puede tener más de 100 caracteres");
        }

        // Validar nombre único (excepto la misma actividad)
        if (await _context.Actividades.AnyAsync(a => a.Nombre == dto.Nombre && a.Id != id && a.FechaEliminacion == null))
        {
            throw new InvalidOperationException("Ya existe una actividad con este nombre");
        }

        // Validar descripción
        if (!string.IsNullOrWhiteSpace(dto.Descripcion) && dto.Descripcion.Length > 500)
        {
            throw new InvalidOperationException("La descripción no puede tener más de 500 caracteres");
        }

        // Validar precio
        if (dto.Precio < 0)
        {
            throw new InvalidOperationException("El precio no puede ser negativo");
        }

        actividad.Nombre = dto.Nombre.Trim();
        actividad.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        actividad.Precio = dto.Precio;
        actividad.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return new ActividadDto
        {
            Id = actividad.Id,
            Nombre = actividad.Nombre,
            Descripcion = actividad.Descripcion,
            Precio = actividad.Precio,
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
