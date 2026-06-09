# Invocación y Propósito de Procedimientos Almacenados

Este documento explica el propósito de la implementación de los procedimientos almacenados (Stored Procedures) en la base de datos de la aplicación, su integración en el backend de .NET y cómo son invocados desde la API y el Frontend de Next.js.

---

## 🎯 1. Propósito de los Procedimientos Almacenados

En lugar de delegar toda la lógica al mapeador objeto-relacional (ORM) en el backend, se optó por implementar procedimientos almacenados para operaciones críticas por las siguientes razones:

1.  **Consistencia de Datos:** Aseguran la atomicidad de las operaciones. En actualizaciones complejas que tocan múltiples entidades, el motor de base de datos ejecuta toda la lógica en un único bloque transaccional.
2.  **Rendimiento en Consultas de Agregación:** El cálculo de balances financieros y asistencia requiere cruzar múltiples tablas (`socios`, `personas`, `membresias`, `pagos`, `cuotas`, `asistencias`). Hacer esto mediante el ORM suele generar consultas ineficientes (problema de consultas N+1). Un procedimiento almacenado realiza estas agrupaciones en el servidor de base de datos eficientemente.
3.  **Seguridad y Auditoría:** Se encapsula la lógica de auditoría directamente en el procedimiento de actualización, garantizando que cualquier cambio de estado a nivel de BD quede auditado, sin importar desde dónde se invoque.

---

## 🛠️ 2. Procedimientos Implementados y su Lógica

### A. Consulta: `sp_ResumenSocio`
*   **Propósito:** Retorna un informe consolidado del perfil de un socio en una sola llamada.
*   **Lógica:** Cruza la tabla `socios` y `personas`, cuenta sus membresías, calcula la sumatoria de costos cargados, calcula el monto total pagado acumulando los pagos asociados a sus cuotas, deduce el saldo pendiente neto, extrae la fecha del último pago y cuenta las asistencias registradas para el mes calendario actual.

### B. Actualización: `sp_CambiarEstadoSocio`
*   **Propósito:** Realiza una desactivación (baja) o reactivación (alta) de un socio en el sistema de manera segura.
*   **Lógica:**
    *   Si `@esta_activo = 0`, cambia el estado a inactivo y asigna la fecha actual como `fecha_baja`.
    *   Si `@esta_activo = 1`, reactiva al socio y limpia la `fecha_baja` a `NULL`.
    *   Inserta de manera atómica un registro en la tabla `auditoria` guardando los detalles del cambio, la fecha/hora, el usuario que procesó el cambio y los estados anterior y nuevo en formato serializado JSON.

---

## 💻 3. Implementación en el Código C# (Backend)

En el backend (.NET 8), la invocación se realiza en la capa de aplicación dentro del servicio [[SocioService.cs](file:///Users/franciscoleiva/GitHub/SistemaDeGestionDeClub/Backend/Application/Services/SocioService.cs)] utilizando la conexión subyacente de Entity Framework Core (`GetDbConnection`) para ejecutar los procedimientos almacenados mediante ADO.NET.

### Invocación de Consulta (`sp_ResumenSocio`)

```csharp
public async Task<ResumenSocioDto?> ObtenerResumenAsync(int idSocio)
{
    var connection = _context.Database.GetDbConnection();
    await using var command = connection.CreateCommand();
    command.CommandText  = "sp_ResumenSocio";
    command.CommandType  = CommandType.StoredProcedure;
    command.Parameters.Add(new SqlParameter("@id_socio", idSocio));

    if (connection.State != ConnectionState.Open)
        await connection.OpenAsync();

    await using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return null;

    return new ResumenSocioDto
    {
        IdSocio              = reader.GetInt32(reader.GetOrdinal("id_socio")),
        NombreCompleto       = reader.GetString(reader.GetOrdinal("nombre_completo")),
        NumeroSocio          = reader.GetString(reader.GetOrdinal("numero_socio")),
        Email                = reader.GetString(reader.GetOrdinal("email")),
        Dni                  = reader.IsDBNull(reader.GetOrdinal("dni")) ? null : reader.GetString(reader.GetOrdinal("dni")),
        EstaActivo           = reader.GetBoolean(reader.GetOrdinal("esta_activo")),
        FechaAlta            = reader.GetDateTime(reader.GetOrdinal("fecha_alta")),
        FechaBaja            = reader.IsDBNull(reader.GetOrdinal("fecha_baja")) ? null : reader.GetDateTime(reader.GetOrdinal("fecha_baja")),
        TotalMembresias      = reader.GetInt32(reader.GetOrdinal("total_membresias")),
        TotalCargado         = reader.GetDecimal(reader.GetOrdinal("total_cargado")),
        TotalPagado          = reader.GetDecimal(reader.GetOrdinal("total_pagado")),
        SaldoPendiente       = reader.GetDecimal(reader.GetOrdinal("saldo_pendiente")),
        UltimaFechaPago      = reader.IsDBNull(reader.GetOrdinal("ultima_fecha_pago")) ? null : reader.GetDateTime(reader.GetOrdinal("ultima_fecha_pago")),
        AsistenciasEsteMes   = reader.GetInt32(reader.GetOrdinal("asistencias_este_mes")),
    };
}
```

### Invocación de Actualización (`sp_CambiarEstadoSocio`)

```csharp
public async Task<SocioDto?> CambiarEstadoAsync(int idSocio, bool estaActivo, int? idUsuarioProcesa)
{
    var connection = _context.Database.GetDbConnection();
    await using var command = connection.CreateCommand();
    command.CommandText = "sp_CambiarEstadoSocio";
    command.CommandType = CommandType.StoredProcedure;
    command.Parameters.Add(new SqlParameter("@id_socio", idSocio));
    command.Parameters.Add(new SqlParameter("@esta_activo", estaActivo ? 1 : 0));
    command.Parameters.Add(new SqlParameter("@id_usuario_procesa", 
        idUsuarioProcesa.HasValue ? (object)idUsuarioProcesa.Value : DBNull.Value));

    if (connection.State != ConnectionState.Open)
        await connection.OpenAsync();

    await using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return null;

    return new SocioDto
    {
        Id           = reader.GetInt32(reader.GetOrdinal("id")),
        NumeroSocio  = reader.GetString(reader.GetOrdinal("numero_socio")),
        Nombre       = reader.GetString(reader.GetOrdinal("nombre")),
        Apellido     = reader.GetString(reader.GetOrdinal("apellido")),
        Email        = reader.GetString(reader.GetOrdinal("email")),
        Dni          = reader.IsDBNull(reader.GetOrdinal("dni")) ? null : reader.GetString(reader.GetOrdinal("dni")),
        EstaActivo   = reader.GetBoolean(reader.GetOrdinal("esta_activo")),
        FechaAlta    = reader.GetDateTime(reader.GetOrdinal("fecha_alta")),
        FechaBaja    = reader.IsDBNull(reader.GetOrdinal("fecha_baja")) ? null : reader.GetDateTime(reader.GetOrdinal("fecha_baja")),
    };
}
```

---

## 🌐 4. Exposición en la API REST

Los endpoints correspondientes se encuentran expuestos en el controlador [[SociosController.cs](file:///Users/franciscoleiva/GitHub/SistemaDeGestionDeClub/Backend/API/Controllers/SociosController.cs)].

### Endpoint de Consulta Financiera
*   **Ruta:** `GET /api/socios/{id}/resumen`
*   **Permiso:** Cualquier usuario autenticado (JWT).
*   **Método:**
```csharp
[HttpGet("{id}/resumen")]
public async Task<ActionResult<ResumenSocioDto>> ObtenerResumen(int id)
{
    var resumen = await _socioService.ObtenerResumenAsync(id);
    if (resumen == null)
        return NotFound(new { message = "Socio no encontrado" });
    return Ok(resumen);
}
```

### Endpoint de Actualización de Estado (Alta/Baja)
*   **Ruta:** `PUT /api/socios/{id}/estado`
*   **Permiso:** Restringido a `superadmin` o `admin`. Obtiene el ID del usuario procesa del token JWT.
*   **Método:**
```csharp
[HttpPut("{id}/estado")]
[Authorize(Roles = "superadmin,admin")]
public async Task<ActionResult<SocioDto>> CambiarEstado(int id, [FromBody] CambiarEstadoSocioDto dto)
{
    var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    int? idUsuarioProcesa = claimId != null && int.TryParse(claimId, out var uid) ? uid : null;

    var socio = await _socioService.CambiarEstadoAsync(id, dto.EstaActivo, idUsuarioProcesa);
    if (socio == null)
        return NotFound(new { message = "Socio no encontrado" });
    return Ok(socio);
}
```

---

## 💻 5. Invocación desde el Frontend (React / Next.js 14)

En el frontend, el módulo se comunica con la API mediante peticiones HTTP asíncronas.

### A. Servicio TypeScript (`socioService.ts`)

```typescript
import apiClient from './apiClient'; // Instancia de axios configurada con token JWT

export interface ResumenSocio {
  idSocio: number;
  nombreCompleto: string;
  numeroSocio: string;
  email: string;
  dni: string | null;
  estaActivo: boolean;
  fechaAlta: string;
  fechaBaja: string | null;
  totalMembresias: number;
  totalCargado: number;
  totalPagado: number;
  saldoPendiente: number;
  ultimaFechaPago: string | null;
  asistenciasEsteMes: number;
}

export interface Socio {
  id: number;
  numeroSocio: string;
  nombre: string;
  apellido: string;
  email: string;
  estaActivo: boolean;
  fechaBaja: string | null;
}

export const socioService = {
  /**
   * Obtiene el resumen financiero y de asistencias del socio (sp_ResumenSocio)
   */
  obtenerResumen: async (idSocio: number): Promise<ResumenSocio> => {
    const response = await apiClient.get<ResumenSocio>(`/socios/${idSocio}/resumen`);
    return response.data;
  },

  /**
   * Cambia el estado del socio entre activo e inactivo (sp_CambiarEstadoSocio)
   */
  cambiarEstado: async (idSocio: number, estaActivo: boolean): Promise<Socio> => {
    const response = await apiClient.put<Socio>(`/socios/${idSocio}/estado`, { estaActivo });
    return response.data;
  }
};
```

### B. Componente React: Widget de Resumen Financiero (`ResumenFinancieroWidget.tsx`)

Este componente llama al endpoint que invoca `sp_ResumenSocio` al cargarse, renderizando la información financiera consolidada.

```tsx
import React, { useEffect, useState } from 'react';
import { socioService, ResumenSocio } from '@/services/socioService';

export default function ResumenFinancieroWidget({ idSocio }: { idSocio: number }) {
  const [resumen, setResumen] = useState<ResumenSocio | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    socioService.obtenerResumen(idSocio)
      .then(setResumen)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [idSocio]);

  if (loading) return <div className="animate-pulse h-32 bg-slate-800 rounded-xl" />;
  if (!resumen) return <div className="text-slate-400">No se pudo cargar el resumen.</div>;

  return (
    <div className="bg-slate-900 border border-slate-800 p-6 rounded-2xl text-white shadow-xl">
      <h3 className="text-lg font-bold bg-gradient-to-r from-emerald-400 to-teal-400 bg-clip-text text-transparent mb-4">
        Resumen de Cuenta: {resumen.nombreCompleto}
      </h3>
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-slate-850 p-4 rounded-xl border border-slate-800">
          <p className="text-xs text-slate-400">Total Cargado</p>
          <p className="text-xl font-extrabold mt-1 text-slate-200">${resumen.totalCargado.toLocaleString()}</p>
        </div>
        <div className="bg-slate-850 p-4 rounded-xl border border-slate-800">
          <p className="text-xs text-slate-400">Total Pagado</p>
          <p className="text-xl font-extrabold mt-1 text-emerald-400">${resumen.totalPagado.toLocaleString()}</p>
        </div>
        <div className="bg-slate-850 p-4 rounded-xl border border-slate-800">
          <p className="text-xs text-slate-400">Saldo Pendiente</p>
          <p className="text-xl font-extrabold mt-1 text-rose-400">${resumen.saldoPendiente.toLocaleString()}</p>
        </div>
        <div className="bg-slate-850 p-4 rounded-xl border border-slate-800">
          <p className="text-xs text-slate-400">Asistencias del Mes</p>
          <p className="text-xl font-extrabold mt-1 text-sky-400">{resumen.asistenciasEsteMes}</p>
        </div>
      </div>
    </div>
  );
}
```

### C. Componente React: Interruptor de Estado de Socio (`EstadoSocioToggle.tsx`)

Este componente permite a un administrador dar de alta o baja a un socio en tiempo real, invocando la actualización y auditoría de `sp_CambiarEstadoSocio`.

```tsx
import React, { useState } from 'react';
import { socioService } from '@/services/socioService';

interface ToggleProps {
  idSocio: number;
  estadoInicial: boolean;
  onStateChanged?: (nuevoEstado: boolean) => void;
}

export default function EstadoSocioToggle({ idSocio, estadoInicial, onStateChanged }: ToggleProps) {
  const [estaActivo, setEstaActivo] = useState(estadoInicial);
  const [loading, setLoading] = useState(false);

  const handleToggle = async () => {
    const nuevoEstado = !estaActivo;
    const confirmación = confirm(
      nuevoEstado 
        ? "¿Está seguro de reactivar este socio?" 
        : "¿Está seguro de dar de baja a este socio del club?"
    );
    if (!confirmación) return;

    try {
      setLoading(true);
      const socioActualizado = await socioService.cambiarEstado(idSocio, nuevoEstado);
      setEstaActivo(socioActualizado.estaActivo);
      if (onStateChanged) onStateChanged(socioActualizado.estaActivo);
    } catch (error) {
      alert("Error al intentar cambiar el estado del socio.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <button
      onClick={handleToggle}
      disabled={loading}
      className={`px-4 py-2 rounded-xl text-xs font-bold transition-all ${
        estaActivo
          ? 'bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 border border-rose-500/25'
          : 'bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400 border border-emerald-500/25'
      }`}
    >
      {loading ? 'Procesando...' : estaActivo ? 'Dar de Baja' : 'Dar de Alta'}
    </button>
  );
}
```
