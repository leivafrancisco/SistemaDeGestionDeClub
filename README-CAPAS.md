# Comunicación entre Capas — Clean Architecture

Este documento explica cómo fluye una solicitud a través de las 4 capas del sistema.

---

## Visión general

```
Cliente (HTTP)
     │
     ▼
┌─────────────────┐
│   API Layer      │  Controllers — recibe y responde solicitudes HTTP
└────────┬────────┘
         │ llama a interfaz del servicio
         ▼
┌─────────────────┐
│ Application Layer│  Services + DTOs — aplica lógica de negocio
└────────┬────────┘
         │ consulta/guarda usando DbContext
         ▼
┌─────────────────┐
│Infrastructure    │  ClubDbContext (EF Core) — accede a la base de datos
└────────┬────────┘
         │ trabaja con entidades del dominio
         ▼
┌─────────────────┐
│  Domain Layer    │  Entities — modelos puros sin dependencias externas
└─────────────────┘
```

---

## Flujo de una solicitud: Crear membresía

### 1. API Layer — `MembresiasController`

El cliente envía:

```http
POST /api/membresias
Authorization: Bearer <token>
Content-Type: application/json

{
  "idSocio": 42,
  "fechaInicio": "2026-04-16",
  "fechaFin": "2026-05-15",
  "idsActividades": [1, 2],
  "costoTotal": 14000.00,
  "monto": 10000.00,
  "idMetodoPago": 1
}
```

El controller:
- Valida el token JWT y extrae el rol del usuario (`[Authorize(Roles = "admin,superadmin")]`)
- Deserializa el body en un `CrearMembresiaDto`
- Delega al servicio: `await _membresiaService.CrearAsync(dto)`
- Devuelve `200 OK` con el `MembresiaDto` resultante

```csharp
// MembresiasController.cs
[HttpPost]
[Authorize(Roles = "admin,superadmin")]
public async Task<IActionResult> Crear([FromBody] CrearMembresiaDto dto)
{
    var resultado = await _membresiaService.CrearAsync(dto);
    return Ok(resultado);
}
```

---

### 2. Application Layer — `MembresiaService`

El servicio recibe el DTO y aplica todas las reglas de negocio:

```
MembresiaService.CrearAsync(dto)
  1. Valida que el socio exista (consulta a Infrastructure)
  2. Valida que fecha_fin > fecha_inicio
  3. Valida que no haya solapamiento de fechas para el mismo socio
  4. Valida que todas las actividades existan y estén activas
  5. Valida costoTotal > 0 y monto > 0
  6. Valida que el método de pago exista
  7. Crea la entidad Membresia (Domain)
  8. Crea las entidades MembresiaActividad con precio congelado (Domain)
  9. Crea la entidad Pago inicial (Domain)
 10. Persiste todo via DbContext (Infrastructure)
 11. Mapea las entidades resultantes a MembresiaDto y lo retorna
```

El servicio **nunca devuelve entidades de dominio al controller** — siempre mapea a DTOs:

```csharp
// MembresiaService.cs
private MembresiaDto MapearADto(Membresia membresia)
{
    return new MembresiaDto
    {
        Id          = membresia.Id,
        TotalCargado = membresia.MembresiaActividades.Sum(ma => ma.PrecioAlMomento),
        TotalPagado  = membresia.Pagos.Where(p => p.FechaEliminacion == null).Sum(p => p.Monto),
        Saldo        = /* TotalCargado - TotalPagado */,
        ...
    };
}
```

---

### 3. Infrastructure Layer — `ClubDbContext`

EF Core traduce las operaciones del servicio a SQL:

```
ClubDbContext
  ├── _context.Membresias.Add(membresia)
  │     → INSERT INTO membresias (id_socio, fecha_inicio, fecha_fin, costo_total, ...)
  │
  ├── _context.MembresiaActividades.AddRange(actividades)
  │     → INSERT INTO membresia_actividades (id_membresia, id_actividad, precio_mensual_congelado)
  │
  ├── _context.Pagos.Add(pago)
  │     → INSERT INTO pagos (id_membresia, id_metodo_pago, monto, ...)
  │
  └── await _context.SaveChangesAsync()
        → Ejecuta todo en una sola transacción
        → Auto-actualiza fecha_actualizacion en todas las entidades modificadas
```

**Convención de nombres:** EF Core traduce automáticamente PascalCase → snake_case:

| Propiedad C# | Columna SQL |
|---|---|
| `FechaInicio` | `fecha_inicio` |
| `IdSocio` | `id_socio` |
| `PrecioAlMomento` | `precio_mensual_congelado` |

---

### 4. Domain Layer — Entidades

Las entidades son POCOs (Plain Old C# Objects) sin dependencias externas. Solo definen estructura y propiedades calculadas:

```csharp
// Membresia.cs (Domain)
public class Membresia
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal CostoTotal { get; set; }

    // Navegación
    public Socio Socio { get; set; }
    public ICollection<MembresiaActividad> MembresiaActividades { get; set; }
    public ICollection<Pago> Pagos { get; set; }
}
```

Las propiedades calculadas (`Saldo`, `TotalPagado`) se computan en el **servicio**, no en la entidad, para mantener el dominio limpio.

---

## Reglas de comunicación entre capas

| Desde | Hacia | Qué pasa |
|---|---|---|
| Controller | Service | Pasa DTOs de entrada, recibe DTOs de salida |
| Service | Domain | Crea y manipula entidades |
| Service | Infrastructure | Consulta y persiste entidades vía DbContext |
| Infrastructure | Domain | Materializa entidades desde la BD |
| Controller | Domain | **Nunca** — el controller nunca toca entidades directamente |

---

## Inyección de dependencias

Todo se conecta a través de interfaces registradas en `Program.cs`:

```csharp
// Program.cs
builder.Services.AddScoped<IMembresiaService, MembresiaService>();
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddDbContext<ClubDbContext>(options =>
    options.UseSqlServer(connectionString));
```

El controller recibe `IMembresiaService` (interfaz), nunca la implementación concreta. Esto permite cambiar la implementación sin tocar el controller.

---

## Resumen del flujo completo

```
HTTP Request
    │
    ▼
Controller          → valida JWT, deserializa DTO, delega al servicio
    │
    ▼
Service             → aplica reglas de negocio, orquesta operaciones
    │
    ├──→ Domain     → crea/modifica entidades (sin lógica de BD)
    │
    └──→ DbContext  → persiste en SQL Server, traduce snake_case
    │
    ▼
Service             → mapea entidades a DTOs de respuesta
    │
    ▼
Controller          → retorna HTTP 200 con el DTO serializado como JSON
    │
    ▼
HTTP Response
```
