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

public class CuotaServiceTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public CuotaServiceTests()
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

    [Fact]
    public async Task GenerarCuotasParaMembresiaAsync_DivisionExacta_GeneraCuotasCorrectas()
    {
        // Arrange
        using var context = GetDbContext(nameof(GenerarCuotasParaMembresiaAsync_DivisionExacta_GeneraCuotasCorrectas));
        var cuotaService = new CuotaService(context);

        var socio = new Socio
        {
            Id = 1,
            NumeroSocio = "SOC-001",
            Persona = new Persona { Nombre = "Juan", Apellido = "Perez", Email = "juan@test.com" }
        };
        context.Socios.Add(socio);

        var membresia = new Membresia
        {
            Id = 10,
            IdSocio = 1,
            FechaInicio = new DateTime(2025, 1, 1),
            FechaFin = new DateTime(2025, 3, 31),
            CostoTotal = 3000m,
            Estado = MembresiaEstado.Activa,
            MembresiaActividades = new List<MembresiaActividad>
            {
                new MembresiaActividad { IdActividad = 1, PrecioAlMomento = 3000m }
            }
        };
        context.Membresias.Add(membresia);
        await context.SaveChangesAsync();

        // Act
        var cuotas = await cuotaService.GenerarCuotasParaMembresiaAsync(10);

        // Assert
        Assert.Equal(3, cuotas.Count);
        Assert.All(cuotas, c => Assert.Equal(1000m, c.Monto));
        Assert.Equal(new DateTime(2025, 1, 31), cuotas[0].FechaVencimiento);
        Assert.Equal(new DateTime(2025, 2, 28), cuotas[1].FechaVencimiento);
        Assert.Equal(new DateTime(2025, 3, 31), cuotas[2].FechaVencimiento);
        Assert.All(cuotas, c => Assert.Equal(CuotaEstado.Pendiente, c.Estado));
    }

    [Fact]
    public async Task GenerarCuotasParaMembresiaAsync_UnSoloMes_GeneraUnaCuota()
    {
        // Arrange
        using var context = GetDbContext(nameof(GenerarCuotasParaMembresiaAsync_UnSoloMes_GeneraUnaCuota));
        var cuotaService = new CuotaService(context);

        var socio = new Socio
        {
            Id = 1,
            NumeroSocio = "SOC-001",
            Persona = new Persona { Nombre = "Juan", Apellido = "Perez", Email = "juan@test.com" }
        };
        context.Socios.Add(socio);

        var membresia = new Membresia
        {
            Id = 11,
            IdSocio = 1,
            FechaInicio = new DateTime(2025, 1, 1),
            FechaFin = new DateTime(2025, 1, 31),
            CostoTotal = 1500m,
            Estado = MembresiaEstado.Activa,
            MembresiaActividades = new List<MembresiaActividad>
            {
                new MembresiaActividad { IdActividad = 1, PrecioAlMomento = 1500m }
            }
        };
        context.Membresias.Add(membresia);
        await context.SaveChangesAsync();

        // Act
        var cuotas = await cuotaService.GenerarCuotasParaMembresiaAsync(11);

        // Assert
        Assert.Single(cuotas);
        Assert.Equal(1500m, cuotas[0].Monto);
        Assert.Equal(new DateTime(2025, 1, 31), cuotas[0].FechaVencimiento);
    }

    [Fact]
    public async Task GenerarCuotasParaMembresiaAsync_DivisionNoExacta_RedondeaCorrectamente()
    {
        // Arrange
        using var context = GetDbContext(nameof(GenerarCuotasParaMembresiaAsync_DivisionNoExacta_RedondeaCorrectamente));
        var cuotaService = new CuotaService(context);

        var socio = new Socio
        {
            Id = 1,
            NumeroSocio = "SOC-001",
            Persona = new Persona { Nombre = "Juan", Apellido = "Perez", Email = "juan@test.com" }
        };
        context.Socios.Add(socio);

        var membresia = new Membresia
        {
            Id = 12,
            IdSocio = 1,
            FechaInicio = new DateTime(2025, 1, 1),
            FechaFin = new DateTime(2025, 3, 31),
            CostoTotal = 1000m,
            Estado = MembresiaEstado.Activa,
            MembresiaActividades = new List<MembresiaActividad>
            {
                new MembresiaActividad { IdActividad = 1, PrecioAlMomento = 1000m }
            }
        };
        context.Membresias.Add(membresia);
        await context.SaveChangesAsync();

        // Act
        var cuotas = await cuotaService.GenerarCuotasParaMembresiaAsync(12);

        // Assert
        Assert.Equal(3, cuotas.Count);
        Assert.All(cuotas, c => Assert.Equal(333.33m, c.Monto));
    }

    [Fact]
    public async Task GenerarCuotasParaMembresiaAsync_AnioBisiestoFebrero_Asigna29DeFebrero()
    {
        // Arrange
        using var context = GetDbContext(nameof(GenerarCuotasParaMembresiaAsync_AnioBisiestoFebrero_Asigna29DeFebrero));
        var cuotaService = new CuotaService(context);

        var socio = new Socio
        {
            Id = 1,
            NumeroSocio = "SOC-001",
            Persona = new Persona { Nombre = "Juan", Apellido = "Perez", Email = "juan@test.com" }
        };
        context.Socios.Add(socio);

        var membresia = new Membresia
        {
            Id = 13,
            IdSocio = 1,
            FechaInicio = new DateTime(2024, 1, 1), // 2024 es bisiesto
            FechaFin = new DateTime(2024, 2, 29),
            CostoTotal = 2000m,
            Estado = MembresiaEstado.Activa,
            MembresiaActividades = new List<MembresiaActividad>
            {
                new MembresiaActividad { IdActividad = 1, PrecioAlMomento = 2000m }
            }
        };
        context.Membresias.Add(membresia);
        await context.SaveChangesAsync();

        // Act
        var cuotas = await cuotaService.GenerarCuotasParaMembresiaAsync(13);

        // Assert
        Assert.Equal(2, cuotas.Count);
        Assert.Equal(new DateTime(2024, 2, 29), cuotas[1].FechaVencimiento);
    }

    [Fact]
    public async Task ActualizarEstadosVencidosAsync_EjecutadoMultiplesVeces_EsConsistenteEIdempotente()
    {
        // Arrange
        using var context = GetDbContext(nameof(ActualizarEstadosVencidosAsync_EjecutadoMultiplesVeces_EsConsistenteEIdempotente));
        var cuotaService = new CuotaService(context);

        var socio = new Socio
        {
            Id = 1,
            NumeroSocio = "SOC-001",
            Persona = new Persona { Nombre = "Juan", Apellido = "Perez", Email = "juan@test.com" }
        };
        context.Socios.Add(socio);

        var membresia = new Membresia
        {
            Id = 14,
            IdSocio = 1,
            FechaInicio = new DateTime(2024, 1, 1),
            FechaFin = new DateTime(2024, 6, 30),
            CostoTotal = 6000m,
            Estado = MembresiaEstado.Activa
        };
        context.Membresias.Add(membresia);

        // Crear una cuota del pasado (vencida) y una del futuro (pendiente)
        var cuotaPasada = new Cuota
        {
            Id = 101,
            IdMembresia = 14,
            NumeroCuota = 1,
            Monto = 1000m,
            FechaVencimiento = DateTime.Today.AddDays(-5), // Vencida
            Estado = CuotaEstado.Pendiente
        };
        var cuotaFutura = new Cuota
        {
            Id = 102,
            IdMembresia = 14,
            NumeroCuota = 2,
            Monto = 1000m,
            FechaVencimiento = DateTime.Today.AddDays(15), // Pendiente del futuro
            Estado = CuotaEstado.Pendiente
        };
        context.Cuotas.AddRange(cuotaPasada, cuotaFutura);
        await context.SaveChangesAsync();

        // Act & Assert 1: Primera ejecución
        var modificados1 = await cuotaService.ActualizarEstadosVencidosAsync();
        Assert.Equal(1, modificados1); // Solo la cuota pasada debe modificarse
        
        // Refrescar entidad y verificar estado
        context.Entry(cuotaPasada).State = EntityState.Detached;
        var cuotaPasadaRefrescada1 = await context.Cuotas.FindAsync(101);
        Assert.Equal(CuotaEstado.Vencida, cuotaPasadaRefrescada1?.Estado);

        // Act & Assert 2: Segunda ejecución inmediata (no debería haber más cambios, garantizando consistencia)
        var modificados2 = await cuotaService.ActualizarEstadosVencidosAsync();
        Assert.Equal(0, modificados2);

        // Refrescar y validar que los estados siguen consistentes
        var cuotaPasadaRefrescada2 = await context.Cuotas.FindAsync(101);
        var cuotaFuturaRefrescada2 = await context.Cuotas.FindAsync(102);
        
        Assert.Equal(CuotaEstado.Vencida, cuotaPasadaRefrescada2?.Estado);
        Assert.Equal(CuotaEstado.Pendiente, cuotaFuturaRefrescada2?.Estado);
    }
}
