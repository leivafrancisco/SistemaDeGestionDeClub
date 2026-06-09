using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SistemaDeGestionDeClub.Application.DTOs; 
using SistemaDeGestionDeClub.Application.Services;
using SistemaDeGestionDeClub.Domain.Entities;
using SistemaDeGestionDeClub.Infrastructure.Data;

namespace SistemaDeGestionDeClub.TestConsole;

class Program
{
    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=========================================================================");
        Console.WriteLine("        SISTEMA DE GESTIÓN DE CLUB - CONSOLA DE EJECUCIÓN DE PRUEBAS      ");
        Console.WriteLine("=========================================================================");
        Console.ResetColor();

        try
        {
            // -----------------------------------------------------------------
            // PRUEBA 1: Función CalcularMeses (Lógica interna de Membresía)
            // -----------------------------------------------------------------
            EjecutarPruebasCalcularMeses();

            // -----------------------------------------------------------------
            // PRUEBA 2: Servicios de Negocio (Socio, Membresía, Cuotas y Pagos)
            // -----------------------------------------------------------------
            await EjecutarPruebasServiciosDeNegocio();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERROR CRÍTICO] Ocurrió un error no controlado: {ex.Message}");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n=========================================================================");
        Console.WriteLine("                      FIN DE LA EJECUCIÓN DE PRUEBAS                     ");
        Console.WriteLine("=========================================================================");
        Console.ResetColor();
    }

    private static void EjecutarPruebasCalcularMeses()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n>>> PRUEBA 1: Función CalcularMeses (Algoritmo de Duración de Membresía)");
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.ResetColor();

        var casos = new List<(DateTime inicio, DateTime fin, int esperado, string descripcion)>
        {
            (new DateTime(2025, 1, 1), new DateTime(2025, 3, 31), 3, "Caso 1: Membresía de 3 meses completos"),
            (new DateTime(2025, 1, 1), new DateTime(2025, 1, 31), 1, "Caso 2: Membresía de 1 solo mes"),
            (new DateTime(2024, 11, 1), new DateTime(2025, 2, 28), 4, "Caso 3: Membresía que cruza cambio de año"),
            (new DateTime(2025, 1, 15), new DateTime(2025, 3, 10), 2, "Caso 4: Día de fin menor al día de inicio (no suma +1)"),
            (new DateTime(2025, 1, 1), new DateTime(2025, 4, 1), 3, "Caso 5: Día de fin igual al día de inicio (exacto 3 meses)"),
            (new DateTime(2027, 1, 1), new DateTime(2027, 3, 1), 2, "Caso 6: Rango propuesto por el usuario (01/01/2027 a 01/03/2027)")
        };

        foreach (var caso in casos)
        {
            int calculado = CalcularMeses(caso.inicio, caso.fin);
            bool paso = calculado == caso.esperado;
            
            if (paso)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  [PASS] ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  [FAIL] ");
            }
            Console.ResetColor();

            Console.WriteLine($"{caso.descripcion,-60} | Rango: {caso.inicio:dd/MM/yyyy} a {caso.fin:dd/MM/yyyy} | Obtenido: {calculado} mes(es) | Esperado: {caso.esperado}");
        }
    }

    // Réplica exacta del método CalcularMeses que se encuentra en MembresiaService
    private static int CalcularMeses(DateTime inicio, DateTime fin)
    {
        return (fin.Year - inicio.Year) * 12 + fin.Month - inicio.Month +
               (fin.Day > inicio.Day ? 1 : 0);
    }

    private static async Task EjecutarPruebasServiciosDeNegocio()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n>>> PRUEBA 2: Servicios de Negocio (Socio, Membresía, Cuotas y Pagos)");
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.ResetColor();

        // 1. Inicializar base de datos en memoria
        var options = new DbContextOptionsBuilder<ClubDbContext>()
            .UseInMemoryDatabase(databaseName: "ClubTestDb_" + Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessor = new HttpContextAccessor();
        using var context = new ClubDbContext(options, httpContextAccessor);

        // Seeding de datos base (Métodos de pago, Actividades)
        await SeedBaseDataAsync(context);

        var socioService = new SocioService(context);
        var membresiaService = new MembresiaService(context);
        var cuotaService = new CuotaService(context);

        // ---------------------------------------------------------------------
        // CASO A: Creación de un nuevo Socio en el sistema
        // ---------------------------------------------------------------------
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  A. Creando un nuevo Socio...");
        Console.ResetColor();

        var nuevoSocioDto = new CrearSocioDto
        {
            Nombre = "Lionel",
            Apellido = "Messi",
            Email = "leo.messi@club.com",
            Dni = "10101010",
            FechaNacimiento = new DateTime(1987, 6, 24)
        };

        var socio = await socioService.CrearSocioAsync(nuevoSocioDto);
        Console.WriteLine($"     [Socio Creado] ID: {socio.Id} | Número Socio: {socio.NumeroSocio} | Nombre: {socio.Nombre} {socio.Apellido}");

        // ---------------------------------------------------------------------
        // CASO B: Crear Membresía válida y verificar autogeneración de cuotas
        // ---------------------------------------------------------------------
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n  B. Creando Membresía (3 meses, Costo: $3,000, Pago Inicial: $1,000)...");
        Console.ResetColor();

        var crearDto = new CrearMembresiaDto
        {
            IdSocio = socio.Id,
            FechaInicio = new DateTime(2028, 1, 1),
            FechaFin = new DateTime(2028, 3, 31),
            CostoTotal = 3000m,
            Monto = 1000m, // Pago inicial
            IdMetodoPago = 1, // Efectivo
            IdUsuarioProcesa = 1,
            IdsActividades = new List<int> { 1 } // Natación
        };

        var membresia = await membresiaService.CrearMembresiaAsync(crearDto);
        
        Console.WriteLine($"     [Membresía Creada] ID {membresia.Id} | Socio: {membresia.NombreSocio}");
        Console.WriteLine($"     Vigencia: {membresia.FechaInicio:dd/MM/yyyy} a {membresia.FechaFin:dd/MM/yyyy}");
        Console.WriteLine($"     Costo Total: ${membresia.CostoTotal} | Total Pagado: ${membresia.TotalPagado} | Saldo: ${membresia.Saldo}");
        Console.WriteLine($"     Estado Membresía: {membresia.Estado}");

        // Mostrar cuotas generadas automáticamente
        var cuotas = await cuotaService.ObtenerCuotasPorMembresiaAsync(membresia.Id);
        Console.WriteLine("\n     Plan de Cuotas Generadas y Estado del Pago:");
        Console.WriteLine("     ----------------------------------------------------------------");
        Console.WriteLine("     N° Cuota |      Vencimiento      |   Monto   |      Estado      ");
        Console.WriteLine("     ----------------------------------------------------------------");
        foreach (var c in cuotas)
        {
            string estadoColor = c.Estado.ToUpper();
            Console.WriteLine($"     Cuota {c.NumeroCuota,-2} |  {c.FechaVencimiento:dd/MM/yyyy}  |  ${c.Monto,-7:N2} |  {estadoColor}");
        }
        Console.WriteLine("     ----------------------------------------------------------------");

        // ---------------------------------------------------------------------
        // CASO C: Validación de solapamiento de fechas
        // ---------------------------------------------------------------------
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n  C. Intentando crear membresía solapada en fechas para el mismo socio...");
        Console.ResetColor();

        var crearDtoSolapado = new CrearMembresiaDto
        {
            IdSocio = socio.Id,
            FechaInicio = new DateTime(2028, 2, 1), // Se solapa con el periodo anterior (Ene-Mar)
            FechaFin = new DateTime(2028, 4, 30),
            CostoTotal = 3000m,
            Monto = 1000m,
            IdMetodoPago = 1,
            IdUsuarioProcesa = 1,
            IdsActividades = new List<int> { 1 }
        };

        try
        {
            await membresiaService.CrearMembresiaAsync(crearDtoSolapado);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("     [FALLÓ] Se esperaba un error de solapamiento pero se creó exitosamente.");
            Console.ResetColor();
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"     [PASÓ] Excepción capturada correctamente: \"{ex.Message}\"");
            Console.ResetColor();
        }

        // ---------------------------------------------------------------------
        // CASO D: Actualizar estados de cuotas vencidas (idempotencia y consistencia)
        // ---------------------------------------------------------------------
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n  D. Creando cuotas del pasado para validar el actualizador de vencimientos...");
        Console.ResetColor();

        // Creamos una membresía inyectándole manualmente una cuota con vencimiento pasado
        var membresiaHistorica = new Membresia
        {
            IdSocio = socio.Id,
            FechaInicio = DateTime.Today.AddMonths(-3),
            FechaFin = DateTime.Today.AddMonths(1),
            CostoTotal = 2000m,
            Estado = MembresiaEstado.PagoPendiente,
            FechaCreacion = DateTime.Now
        };
        context.Membresias.Add(membresiaHistorica);
        await context.SaveChangesAsync();

        var cuotaPasada = new Cuota
        {
            IdMembresia = membresiaHistorica.Id,
            NumeroCuota = 1,
            Monto = 1000m,
            FechaVencimiento = DateTime.Today.AddDays(-10), // VENCIDA
            Estado = CuotaEstado.Pendiente,
            FechaCreacion = DateTime.Now
        };
        var cuotaFutura = new Cuota
        {
            IdMembresia = membresiaHistorica.Id,
            NumeroCuota = 2,
            Monto = 1000m,
            FechaVencimiento = DateTime.Today.AddDays(20), // PENDIENTE
            Estado = CuotaEstado.Pendiente,
            FechaCreacion = DateTime.Now
        };
        context.Cuotas.AddRange(cuotaPasada, cuotaFutura);
        await context.SaveChangesAsync();

        Console.WriteLine($"     Cuotas antes de actualización:");
        Console.WriteLine($"       Cuota 1 Vence: {cuotaPasada.FechaVencimiento:dd/MM/yyyy} | Estado: {cuotaPasada.Estado}");
        Console.WriteLine($"       Cuota 2 Vence: {cuotaFutura.FechaVencimiento:dd/MM/yyyy} | Estado: {cuotaFutura.Estado}");

        // Primera actualización
        int modificadas1 = await cuotaService.ActualizarEstadosVencidosAsync();
        Console.WriteLine($"\n     >>> Ejecución 1 de ActualizarEstadosVencidosAsync(): se modificaron {modificadas1} cuota(s).");
        Console.WriteLine($"       Cuota 1 Estado Actualizado: {cuotaPasada.Estado} (Esperado: vencida)");
        Console.WriteLine($"       Cuota 2 Estado Actualizado: {cuotaFutura.Estado} (Esperado: pendiente)");

        // Segunda actualización (validando consistencia e idempotencia)
        int modificadas2 = await cuotaService.ActualizarEstadosVencidosAsync();
        Console.WriteLine($"     >>> Ejecución 2 de ActualizarEstadosVencidosAsync(): se modificaron {modificadas2} cuota(s).");
        
        if (modificadas2 == 0 && cuotaPasada.Estado == CuotaEstado.Vencida && cuotaFutura.Estado == CuotaEstado.Pendiente)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("     [PASÓ] El actualizador es consistente e idempotente. Mantiene los estados correctos.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("     [FALLÓ] Inconsistencia detectada en las ejecuciones repetidas.");
            Console.ResetColor();
        }
    }

    private static async Task SeedBaseDataAsync(ClubDbContext context)
    {
        // Métodos de pago
        context.MetodosPago.AddRange(
            new MetodoPago { Id = 1, Nombre = "Efectivo" },
            new MetodoPago { Id = 2, Nombre = "Tarjeta" }
        );

        // Actividades
        context.Actividades.AddRange(
            new Actividad { Id = 1, Nombre = "Natacion", Precio = 3000m, EsCuotaBase = false },
            new Actividad { Id = 2, Nombre = "Gimnasio", Precio = 2000m, EsCuotaBase = false }
        );

        await context.SaveChangesAsync();
    }
}
