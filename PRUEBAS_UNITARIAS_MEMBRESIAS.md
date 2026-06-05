# Pruebas Unitarias — GenerarCuotasAsync y CalcularMeses

**Carrera:** Lic. en Sistemas de Información
**Asignatura:** Ingeniería del Software II — FACENA UNNE

---

## Métodos bajo prueba

### CalcularMeses
```csharp
private static int CalcularMeses(DateTime inicio, DateTime fin)
{
    return (fin.Year - inicio.Year) * 12 + fin.Month - inicio.Month +
           (fin.Day >= inicio.Day ? 1 : 0);
}
```
Calcula cuántos meses hay entre dos fechas. Si el día de fin es mayor o igual al día de inicio, suma 1 mes adicional.

---

### GenerarCuotasAsync
```csharp
private async Task<List<Cuota>> GenerarCuotasAsync(Membresia membresia)
```
Genera una cuota por cada mes que dure la membresía. Divide el costo total en partes iguales y asigna como fecha de vencimiento el último día de cada mes.

---

## Tabla de Pruebas — CalcularMeses

| # | FechaInicio | FechaFin | Descripción | Resultado esperado |
|---|-------------|----------|-------------|-------------------|
| 1 | 01/01/2025 | 31/03/2025 | Membresía de 3 meses completos | 3 |
| 2 | 01/01/2025 | 31/01/2025 | Membresía de 1 solo mes | 1 |
| 3 | 01/11/2024 | 28/02/2025 | Membresía que cruza cambio de año | 4 |
| 4 | 15/01/2025 | 10/03/2025 | Día de fin menor al día de inicio | 2 |
| 5 | 01/01/2025 | 01/04/2025 | Día de fin igual al día de inicio | 4 |
| 6 | 01/01/2025 | 01/01/2025 | Misma fecha inicio y fin | 1 |

### Justificación

**Caso 1 — 3 meses normales**
```
(2025-2025)*12 + (3-1) + (31 >= 1 → 1) = 0 + 2 + 1 = 3
```

**Caso 2 — 1 mes**
```
(2025-2025)*12 + (1-1) + (31 >= 1 → 1) = 0 + 0 + 1 = 1
```

**Caso 3 — Cruza año**
```
(2025-2024)*12 + (2-11) + (28 >= 1 → 1) = 12 - 9 + 1 = 4
```
Verifica que el cálculo es correcto al pasar de un año al siguiente.

**Caso 4 — Día de fin MENOR al día de inicio**
```
(2025-2025)*12 + (3-1) + (10 >= 15 → 0) = 0 + 2 + 0 = 2
```
Cuando el día de fin es menor NO se suma el +1.

**Caso 5 — Día de fin IGUAL al día de inicio**
```
(2025-2025)*12 + (4-1) + (1 >= 1 → 1) = 0 + 3 + 1 = 4
```
Cuando el día coincide SÍ se suma el +1.

**Caso 6 — Misma fecha inicio y fin**
```
(2025-2025)*12 + (1-1) + (1 >= 1 → 1) = 0 + 0 + 1 = 1
```
Caso borde: mismo día cuenta como 1 mes.

---

## Tabla de Pruebas — GenerarCuotasAsync

| # | FechaInicio | FechaFin | CostoTotal | Descripción | Resultado esperado |
|---|-------------|----------|------------|-------------|-------------------|
| 1 | 01/01/2025 | 31/03/2025 | $3000 | División exacta en 3 meses | 3 cuotas de $1000 c/u |
| 2 | 01/01/2025 | 31/01/2025 | $1500 | Membresía de 1 mes | 1 cuota de $1500 |
| 3 | 01/01/2025 | 31/03/2025 | $1000 | Costo no divisible exactamente | 3 cuotas de $333.33 c/u |
| 4 | 01/01/2025 | 28/02/2025 | $2000 | Cuota de febrero en año normal | Cuota 2 vence 28/02/2025 |
| 5 | 01/01/2024 | 29/02/2024 | $2000 | Año bisiesto — febrero tiene 29 días | Cuota 2 vence 29/02/2024 |
| 6 | 01/01/2025 | 31/03/2025 | $3000 | Estado inicial de todas las cuotas | Todas en estado Pendiente |

### Justificación

**Caso 1 — División exacta**
```
$3000 / 3 = $1000 por cuota
Cuota 1 → vence 31/01 → Pendiente
Cuota 2 → vence 28/02 → Pendiente
Cuota 3 → vence 31/03 → Pendiente
```
Caso más común. Verifica cantidad y monto correcto de cuotas.

**Caso 2 — Un solo mes**
```
$1500 / 1 = $1500
Cuota 1 → vence 31/01 → Pendiente
```
Membresía mínima. Solo debe generarse una cuota.

**Caso 3 — Costo no divisible**
```
$1000 / 3 = $333.3333...
Math.Round(333.3333, 2) = $333.33
```
Verifica que el redondeo a 2 decimales funciona y no genera $333.333333.

**Caso 4 — Último día de febrero (año normal)**
```
DateTime.DaysInMonth(2025, 2) = 28
→ FechaVencimiento Cuota 2 = 28/02/2025
```
Verifica que no se intenta crear la fecha 30/02 o 31/02 que no existen.

**Caso 5 — Año bisiesto**
```
DateTime.DaysInMonth(2024, 2) = 29   (2024 es bisiesto)
→ FechaVencimiento Cuota 2 = 29/02/2024
```
Verifica que el sistema detecta correctamente los años bisiestos.

**Caso 6 — Estado inicial**
Todas las cuotas se crean con `CuotaEstado.Pendiente`.
Solo al registrar un pago una cuota pasa a estado `Pagada`.

---

## Resultado visual esperado (Caso 1)

```
Membresía: 01/01/2025 → 31/03/2025 | CostoTotal: $3000

Cuota #1  ──  $1000  ──  vence 31/01/2025  ──  Pendiente
Cuota #2  ──  $1000  ──  vence 28/02/2025  ──  Pendiente
Cuota #3  ──  $1000  ──  vence 31/03/2025  ──  Pendiente
```
