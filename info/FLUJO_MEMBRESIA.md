# Flujo completo de Membresía

Este documento explica cómo funciona el módulo de membresías del sistema, desde que el usuario completa el formulario hasta que los datos quedan guardados en la base de datos.

---

## ¿Qué es una Membresía?

Una membresía representa la inscripción de un socio al club durante un período de tiempo determinado. Incluye:
- Las fechas de inicio y fin
- Las actividades a las que accede (natación, gimnasio, etc.)
- El costo total dividido en cuotas mensuales
- Los pagos realizados

---

## 1. Crear una Membresía

### Flujo general

```
[Formulario del front]
        │
        │  POST /api/membresias
        ↓
[MembresiasController] ── verifica rol admin ──→ si no tiene rol: 403 Forbidden
        │
        ↓
[MembresiaService.CrearMembresiaAsync]
        │
        ├── Validaciones (6 controles)
        │       ¿Existe el socio?
        │       ¿FechaFin > FechaInicio?
        │       ¿Fechas se solapan con otra membresía?
        │       ¿Las actividades existen?
        │       ¿CostoTotal > 0?
        │       ¿El método de pago existe?
        │
        ├── Guarda la Membresía en la BD
        │
        ├── Guarda las Actividades (tabla MembresiaActividades)
        │
        ├── Genera las Cuotas mensuales
        │
        └── Registra el pago de la Cuota #1
                │
                ↓
        [Base de datos]   ✔ Todo guardado
                │
                ↓
        Retorna la membresía completa → 201 Created
```

### Ejemplo concreto

Un socio se inscribe del 01/01/2025 al 31/03/2025, con costo $3000, elige Natación y paga con tarjeta.

**Lo que se crea en la base de datos:**

```
Membresia
  ├── id: 5
  ├── idSocio: 12
  ├── fechaInicio: 01/01/2025
  ├── fechaFin: 31/03/2025
  ├── costoTotal: $3000
  └── estado: Activa

MembresiaActividades
  └── idMembresia: 5  →  idActividad: "Natación"  →  precioAlMomento: $1000

Cuotas
  ├── Cuota 1: $1000 — vence 31/01 — estado: Pagada
  ├── Cuota 2: $1000 — vence 28/02 — estado: Pendiente
  └── Cuota 3: $1000 — vence 31/03 — estado: Pendiente

Pagos
  └── Pago vinculado a Cuota 1: $1000 — método: Tarjeta
```

### ¿Cómo se generan las cuotas?

El método `GenerarCuotasAsync` divide el costo total en partes iguales, una por mes:

```
costoTotal / cantidadMeses = montoPorCuota
$3000 / 3 = $1000 por cuota
```

Cada cuota tiene como fecha de vencimiento el **último día del mes** correspondiente.

---

## 2. Actualizar una Membresía

Permite modificar las fechas y/o las actividades de una membresía existente.

```
[PUT /api/membresias/{id}]
        │
        ├── Verifica que la membresía exista
        ├── Valida que las nuevas fechas no se solapen con otras membresías
        ├── Reemplaza las actividades anteriores por las nuevas
        ├── Recalcula el costo total según las nuevas actividades
        └── Guarda los cambios
```

**Regla importante:** Los pagos ya registrados se mantienen intactos. Si el costo cambia, el saldo se recalcula automáticamente.

---

## 3. Eliminar una Membresía

La eliminación es **lógica** (soft delete): no borra el registro de la base de datos, solo le pone una `FechaEliminacion`.

```
[DELETE /api/membresias/{id}]
        │
        ├── ¿Tiene pagos registrados?
        │       SÍ → Error: "No se puede eliminar" (protege la integridad)
        │       NO → FechaEliminacion = DateTime.Now ✔
        └── La membresía ya no aparece en las consultas
```

---

## 4. Estados de una Membresía

Una membresía puede estar en 3 estados distintos. El sistema los actualiza automáticamente cada vez que se registra un pago.

| Estado | Cuándo ocurre |
|--------|--------------|
| `Activa` | El total pagado cubre el costo total |
| `PagoPendiente` | Hay cuotas sin pagar y la membresía no venció |
| `Vencida` | La fecha de fin ya pasó |

```
Al registrar un pago:
  ¿FechaFin ya pasó?       → Vencida
  ¿totalPagado >= costoTotal? → Activa
  Si no                    → PagoPendiente
```

---

## 5. Asignar / Remover Actividades

Luego de crear la membresía, es posible agregar o quitar actividades de forma individual.

**Asignar actividad:**
```
¿La membresía existe? → ¿La actividad existe? → ¿Ya está asignada?
        NO →  Error          NO → Error              SÍ → Error
        SÍ                   SÍ                       NO → Se agrega ✔
```

**Remover actividad:**
```
¿La membresía tiene pagos registrados?
        SÍ → Error: no se puede modificar (integridad de datos)
        NO → Se elimina la actividad ✔
```

---

## 6. Capas del sistema involucradas

```
Frontend (React/TypeScript)
    └── membresiaService.ts → apiClient.post('/membresias', datos)
            │
            ↓
API Layer (.NET)
    └── MembresiasController.cs → recibe el request HTTP
            │
            ↓
Application Layer (.NET)
    └── MembresiaService.cs → toda la lógica de negocio
            │
            ↓
Infrastructure Layer (.NET)
    └── ClubDbContext (Entity Framework) → accede a la base de datos
            │
            ↓
Base de Datos (SQL Server)
    └── Tablas: Membresias, MembresiaActividades, Cuotas, Pagos
```

---

## 7. Reglas de negocio principales

| Regla | Motivo |
|-------|--------|
| No se pueden solapar membresías del mismo socio | Un socio no puede tener dos membresías activas en el mismo período |
| No se puede eliminar si tiene pagos | Protege el historial de pagos |
| No se puede remover actividades si hay pagos | Mantiene la consistencia entre lo cobrado y lo contratado |
| La primera cuota se marca como pagada al crear | El pago inicial se registra en el momento de la inscripción |
| El precio de la actividad se guarda al momento | Si el precio cambia en el futuro, el historial queda intacto |
