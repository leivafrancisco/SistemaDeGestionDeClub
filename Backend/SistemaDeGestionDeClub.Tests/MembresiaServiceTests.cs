using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SistemaDeGestionDeClub.Application.DTOs;
using SistemaDeGestionDeClub.Application.Services;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;
using Xunit;

namespace SistemaDeGestionDeClub.Tests;

public class MembresiaServiceTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public MembresiaServiceTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    }

    private ClubDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ClubDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ClubDbContext(options, _mockHttpContextAccessor.Object);
    }

    private async Task SeedDatabaseAsync(ClubDbContext context)
    {
        // Agregar Roles
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Rol { Id = 1, Nombre = "superadmin" },
                new Rol { Id = 2, Nombre = "admin" }
            );
        }

        // Agregar Métodos de Pago
        if (!await context.MetodosPago.AnyAsync())
        {
            context.MetodosPago.AddRange(
                new MetodoPago { Id = 1, Nombre = "Efectivo" },
                new MetodoPago { Id = 2, Nombre = "Tarjeta" }
            );
        }

        // Agregar Actividades
        if (!await context.Actividades.AnyAsync())
        {
            context.Actividades.AddRange(
                new Actividad { Id = 1, Nombre = "Natacion", Precio = 1000m, EsCuotaBase = false },
                new Actividad { Id = 2, Nombre = "Gimnasio", Precio = 800m, EsCuotaBase = false },
                new Actividad { Id = 3, Nombre = "Cuota base", Precio = 0m, EsCuotaBase = true }
            );
        }

        // Agregar un Socio
        if (!await context.Socios.AnyAsync())
        {
            context.Socios.Add(new Socio
            {
                Id = 1,
                NumeroSocio = "SOC-001",
                EstaActivo = true,
                Persona = new Persona
                {
                    Id = 100,
                    Nombre = "Pedro",
                    Apellido = "Gomez",
                    Email = "pedro.gomez@test.com",
                    Dni = "35111222"
                }
            });
        }

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CrearMembresiaAsync_DatosValidos_GeneraMembresiaYCuotasYDistribuyePago()
    {
        // Arrange
        using var context = GetDbContext(nameof(CrearMembresiaAsync_DatosValidos_GeneraMembresiaYCuotasYDistribuyePago));
        await SeedDatabaseAsync(context);
        var service = new MembresiaService(context);

        var dto = new CrearMembresiaDto
        {
            IdSocio = 1,
            FechaInicio = new DateTime(2028, 1, 1),
            FechaFin = new DateTime(2028, 3, 31), // 3 meses
            CostoTotal = 3000m,
            Monto = 1000m, // Paga el primer mes (Cuota 1 de 1000m)
            IdMetodoPago = 1,
            IdUsuarioProcesa = 1,
            IdsActividades = new List<int> { 1 } // Natación
        };

        // Act
        var result = await service.CrearMembresiaAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3000m, result.CostoTotal);
        Assert.Equal(1000m, result.TotalPagado);
        Assert.Equal(2000m, result.Saldo);
        Assert.False(result.EstaPaga);
        Assert.Equal(MembresiaEstado.PagoPendiente, result.Estado);
        Assert.Single(result.Actividades);
        Assert.Equal("Natacion", result.Actividades.First().NombreActividad);

        // Validar cuotas en la base de datos
        var cuotas = await context.Cuotas.Where(c => c.IdMembresia == result.Id).OrderBy(c => c.NumeroCuota).ToListAsync();
        Assert.Equal(3, cuotas.Count);
        Assert.Equal(CuotaEstado.Pagada, cuotas[0].Estado);     // Cuota 1 pagada
        Assert.Equal(CuotaEstado.Pendiente, cuotas[1].Estado);  // Cuota 2 pendiente
        Assert.Equal(CuotaEstado.Pendiente, cuotas[2].Estado);  // Cuota 3 pendiente

        // Validar pagos registrados
        var pagos = await context.Pagos.Where(p => p.Cuota.IdMembresia == result.Id).ToListAsync();
        Assert.Single(pagos);
        Assert.Equal(1000m, pagos.First().Monto);
    }

    [Fact]
    public async Task CrearMembresiaAsync_FechasSolapadas_LanzaException()
    {
        // Arrange
        using var context = GetDbContext(nameof(CrearMembresiaAsync_FechasSolapadas_LanzaException));
        await SeedDatabaseAsync(context);
        var service = new MembresiaService(context);

        // Crear una membresía activa inicial
        var membresiaExistente = new Membresia
        {
            Id = 50,
            IdSocio = 1,
            FechaInicio = new DateTime(2025, 1, 1),
            FechaFin = new DateTime(2025, 1, 31),
            CostoTotal = 1000m,
            Estado = MembresiaEstado.Activa
        };
        context.Membresias.Add(membresiaExistente);
        await context.SaveChangesAsync();

        var dtoSolapado = new CrearMembresiaDto
        {
            IdSocio = 1,
            FechaInicio = new DateTime(2025, 1, 15), // Solapa con la existente
            FechaFin = new DateTime(2025, 2, 28),
            CostoTotal = 2000m,
            Monto = 1000m,
            IdMetodoPago = 1,
            IdsActividades = new List<int> { 1 }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CrearMembresiaAsync(dtoSolapado));
        Assert.Contains("solapa", ex.Message);
    }

    [Fact]
    public async Task CrearMembresiaAsync_FechasIncorrectas_LanzaException()
    {
        // Arrange
        using var context = GetDbContext(nameof(CrearMembresiaAsync_FechasIncorrectas_LanzaException));
        await SeedDatabaseAsync(context);
        var service = new MembresiaService(context);

        var dtoFechasMal = new CrearMembresiaDto
        {
            IdSocio = 1,
            FechaInicio = new DateTime(2025, 2, 1),
            FechaFin = new DateTime(2025, 1, 31), // Fin anterior a Inicio
            CostoTotal = 1000m,
            Monto = 1000m,
            IdMetodoPago = 1,
            IdsActividades = new List<int> { 1 }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CrearMembresiaAsync(dtoFechasMal));
        Assert.Contains("posterior", ex.Message);
    }
}
