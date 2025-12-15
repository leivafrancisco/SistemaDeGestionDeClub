using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.Application.Services;

public interface IAuditoriaService
{
    Task<PagedResult<AuditoriaDto>> ObtenerAuditoriasAsync(AuditoriaFiltrosDto filtros);
    Task<AuditoriaDto?> ObtenerAuditoriaPorIdAsync(int id);
}

public class AuditoriaService : IAuditoriaService
{
    private readonly ClubDbContext _context;

    public AuditoriaService(ClubDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditoriaDto>> ObtenerAuditoriasAsync(AuditoriaFiltrosDto filtros)
    {
        var query = _context.Auditorias.AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(filtros.Tabla))
        {
            query = query.Where(a => a.Tabla == filtros.Tabla);
        }

        if (!string.IsNullOrEmpty(filtros.Operacion))
        {
            query = query.Where(a => a.Operacion == filtros.Operacion);
        }

        if (filtros.IdUsuario.HasValue)
        {
            query = query.Where(a => a.IdUsuario == filtros.IdUsuario.Value);
        }

        if (filtros.FechaDesde.HasValue)
        {
            query = query.Where(a => a.FechaHora >= filtros.FechaDesde.Value);
        }

        if (filtros.FechaHasta.HasValue)
        {
            var fechaHastaFinal = filtros.FechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(a => a.FechaHora <= fechaHastaFinal);
        }

        // Contar total
        var totalCount = await query.CountAsync();

        // Aplicar paginación
        var items = await query
            .OrderByDescending(a => a.FechaHora)
            .Skip((filtros.Page - 1) * filtros.PageSize)
            .Take(filtros.PageSize)
            .Select(a => new AuditoriaDto
            {
                Id = a.Id,
                Tabla = a.Tabla,
                Operacion = a.Operacion,
                IdUsuario = a.IdUsuario,
                NombreUsuario = a.NombreUsuario,
                FechaHora = a.FechaHora,
                ValoresAnteriores = a.ValoresAnteriores,
                ValoresNuevos = a.ValoresNuevos,
                NombreEntidad = a.NombreEntidad,
                IdEntidad = a.IdEntidad,
                Detalles = a.Detalles
            })
            .ToListAsync();

        return new PagedResult<AuditoriaDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filtros.Page,
            PageSize = filtros.PageSize
        };
    }

    public async Task<AuditoriaDto?> ObtenerAuditoriaPorIdAsync(int id)
    {
        var auditoria = await _context.Auditorias
            .Where(a => a.Id == id)
            .Select(a => new AuditoriaDto
            {
                Id = a.Id,
                Tabla = a.Tabla,
                Operacion = a.Operacion,
                IdUsuario = a.IdUsuario,
                NombreUsuario = a.NombreUsuario,
                FechaHora = a.FechaHora,
                ValoresAnteriores = a.ValoresAnteriores,
                ValoresNuevos = a.ValoresNuevos,
                NombreEntidad = a.NombreEntidad,
                IdEntidad = a.IdEntidad,
                Detalles = a.Detalles
            })
            .FirstOrDefaultAsync();

        return auditoria;
    }
}
