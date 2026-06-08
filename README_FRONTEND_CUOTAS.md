# Guía de Implementación del Módulo de Cuotas en el Frontend (Next.js 14 & React)

Este documento detalla la estructura, interfaces, servicios y componentes necesarios para integrar la funcionalidad de **Gestión de Cuotas** en el Frontend (Next.js 14 / React / TypeScript). 

El backend expone un conjunto de endpoints REST bajo `/api/cuotas` para interactuar con las cuotas mensuales asociadas a las membresías de los socios.

---

## 📌 1. Endpoints de la API (Backend .NET)

Todas las rutas requieren autenticación mediante token JWT (`Authorization: Bearer <token>`). Algunas operaciones críticas están restringidas a roles **Admin** o **Superadmin**.

| Método | Endpoint | Roles Permitidos | Descripción |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/cuotas` | Todos | Obtiene cuotas filtradas y paginadas. |
| **GET** | `/api/cuotas/{id}` | Todos | Obtiene el detalle de una cuota por su ID. |
| **GET** | `/api/cuotas/membresia/{idMembresia}` | Todos | Obtiene todas las cuotas asociadas a una membresía. |
| **GET** | `/api/cuotas/socio/{idSocio}` | Todos | Obtiene todas las cuotas de un socio (historial). |
| **GET** | `/api/cuotas/resumen` | `admin`, `superadmin` | Estadísticas generales de cuotas y montos pendientes/vencidos. |
| **GET** | `/api/cuotas/morosos` | `admin`, `superadmin` | Obtiene el listado de socios morosos (cuotas vencidas). |
| **POST** | `/api/cuotas/generar/{idMembresia}` | `admin`, `superadmin` | Genera las cuotas de una membresía (si no tiene cuotas activas). |
| **POST** | `/api/cuotas/actualizar-vencidas` | `admin`, `superadmin` | Proceso por lotes que marca como `vencida` toda cuota pendiente con fecha pasada. |

---

## 💻 2. Modelos y Tipos de TypeScript

Crea un archivo de tipos en tu frontend, por ejemplo, en `src/types/cuota.types.ts`:

```typescript
export type CuotaEstado = 'pendiente' | 'pagada' | 'vencida';

export interface CuotaDto {
  id: number;
  idMembresia: number;
  periodoMembresia: string; // Ejemplo: "01/01/2025 - 31/03/2025"
  idSocio: number;
  numeroSocio: string;
  nombreSocio: string;
  numeroCuota: number;
  monto: number;
  fechaVencimiento: string; // ISO DateTime
  estado: CuotaEstado;
  esMorosa: boolean;
  diasVencida: number;
  fechaCreacion: string; // ISO DateTime
}

export interface MorosoDto {
  idSocio: number;
  numeroSocio: string;
  nombreSocio: string;
  email: string;
  cuotasVencidas: number;
  deudaTotal: number;
  fechaVencimientoMasTemprana: string; // ISO DateTime
  cuotas: CuotaDto[];
}

export interface ResumenCuotasDto {
  totalCuotas: number;
  cuotasPendientes: number;
  cuotasPagadas: number;
  cuotasVencidas: number;
  montoTotalPendiente: number;
  montoTotalVencido: number;
  totalMorosos: number;
}

export interface FiltrosCuotasDto {
  idMembresia?: number;
  idSocio?: number;
  estado?: CuotaEstado;
  soloVencidas?: boolean;
  fechaVencimientoDesde?: string;
  fechaVencimientoHasta?: string;
  page: number;
  pageSize: number;
}
```

---

## 📡 3. Servicio de API (`cuotaService.ts`)

Implementa las llamadas al backend utilizando tu cliente Axios o Fetch configurado con JWT.
Ubicación recomendada: `src/services/cuotaService.ts`

```typescript
import apiClient from './apiClient'; // Tu cliente configurado de Axios/Fetch
import { CuotaDto, MorosoDto, ResumenCuotasDto, FiltrosCuotasDto } from '../types/cuota.types';

export const cuotaService = {
  /**
   * Obtiene la lista de cuotas según filtros (soporta paginación)
   */
  obtenerTodas: async (filtros: Partial<FiltrosCuotasDto> = {}): Promise<CuotaDto[]> => {
    const response = await apiClient.get<CuotaDto[]>('/cuotas', { params: filtros });
    return response.data;
  },

  /**
   * Obtiene una cuota por su ID
   */
  obtenerPorId: async (id: number): Promise<CuotaDto> => {
    const response = await apiClient.get<CuotaDto>(`/cuotas/${id}`);
    return response.data;
  },

  /**
   * Obtiene las cuotas de una membresía específica
   */
  obtenerPorMembresia: async (idMembresia: number): Promise<CuotaDto[]> => {
    const response = await apiClient.get<CuotaDto[]>(`/cuotas/membresia/${idMembresia}`);
    return response.data;
  },

  /**
   * Obtiene el historial de cuotas de un socio
   */
  obtenerPorSocio: async (idSocio: number): Promise<CuotaDto[]> => {
    const response = await apiClient.get<CuotaDto[]>(`/cuotas/socio/${idSocio}`);
    return response.data;
  },

  /**
   * Obtiene estadísticas de recaudación y estado de cuotas (Solo Admin)
   */
  obtenerResumen: async (): Promise<ResumenCuotasDto> => {
    const response = await apiClient.get<ResumenCuotasDto>('/cuotas/resumen');
    return response.data;
  },

  /**
   * Obtiene la lista de socios morosos (Solo Admin)
   */
  obtenerMorosos: async (): Promise<MorosoDto[]> => {
    const response = await apiClient.get<MorosoDto[]>('/cuotas/morosos');
    return response.data;
  },

  /**
   * Genera las cuotas para una membresía (Solo Admin)
   */
  generarCuotas: async (idMembresia: number): Promise<CuotaDto[]> => {
    const response = await apiClient.post<CuotaDto[]>(`/cuotas/generar/${idMembresia}`);
    return response.data;
  },

  /**
   * Actualiza el estado de cuotas vencidas (lote automático) (Solo Admin)
   */
  actualizarVencidas: async (): Promise<{ message: string; cantidad: number }> => {
    const response = await apiClient.post<{ message: string; cantidad: number }>('/cuotas/actualizar-vencidas');
    return response.data;
  },
};
```

---

## 🎨 4. Componentes y Vistas Recomendados

Para lograr una interfaz premium y coherente, se sugieren los siguientes diseños utilizando **Tailwind CSS** y **micro-animaciones**.

### A. Dashboard General de Finanzas / Cuotas (`page.tsx`)
Esta pantalla permite al administrador monitorear el estado general de las finanzas y realizar mantenimiento (actualizar vencimientos).

```tsx
import React, { useEffect, useState } from 'react';
import { cuotaService } from '@/services/cuotaService';
import { ResumenCuotasDto } from '@/types/cuota.types';
import { CurrencyDollarIcon, UsersIcon, BellAlertIcon, ArrowPathIcon } from '@heroicons/react/24/outline';

export default function ResumenCuotas() {
  const [resumen, setResumen] = useState<ResumenCuotasDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(false);

  const cargarResumen = async () => {
    try {
      setLoading(true);
      const data = await cuotaService.ObtenerResumen();
      setResumen(data);
    } catch (error) {
      console.error("Error al cargar resumen de cuotas", error);
    } finally {
      setLoading(false);
    }
  };

  const ejecutarActualizacion = async () => {
    if (!confirm("¿Desea actualizar y marcar las cuotas vencidas a la fecha de hoy?")) return;
    try {
      setUpdating(true);
      const res = await cuotaService.actualizarVencidas();
      alert(res.message);
      await cargarResumen();
    } catch (error) {
      alert("Error al actualizar cuotas");
    } finally {
      setUpdating(false);
    }
  };

  useEffect(() => {
    cargarResumen();
  }, []);

  if (loading) return <div className="text-center py-10 animate-pulse text-slate-400">Cargando estadísticas...</div>;

  return (
    <div className="space-y-8 p-6 bg-slate-900 text-white min-h-screen">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-extrabold tracking-tight bg-gradient-to-r from-emerald-400 to-teal-500 bg-clip-text text-transparent">
            Control de Cuotas e Ingresos
          </h1>
          <p className="text-slate-400 text-sm mt-1">Monitoreo de recaudación, morosidad y estados de pagos.</p>
        </div>
        <button
          onClick={ejecutarActualizacion}
          disabled={updating}
          className="flex items-center gap-2 bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 disabled:from-slate-700 disabled:to-slate-700 text-white font-semibold px-4 py-2 rounded-xl shadow-lg transition-all transform hover:-translate-y-0.5"
        >
          <ArrowPathIcon className={`h-5 w-5 ${updating ? 'animate-spin' : ''}`} />
          {updating ? 'Procesando...' : 'Actualizar Vencidas'}
        </button>
      </div>

      {resumen && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {/* Card Pendientes */}
          <div className="relative overflow-hidden bg-slate-800/80 border border-slate-700/50 p-6 rounded-2xl transition-all hover:scale-[1.02]">
            <div className="flex justify-between items-center">
              <div>
                <p className="text-slate-400 text-xs font-semibold uppercase tracking-wider">Monto Pendiente Total</p>
                <h3 className="text-2xl font-bold mt-2 text-amber-400">${resumen.montoTotalPendiente.toLocaleString()}</h3>
                <p className="text-slate-400 text-xs mt-1">{resumen.cuotasPendientes} cuotas activas</p>
              </div>
              <div className="p-3 rounded-xl bg-amber-500/10 text-amber-400">
                <CurrencyDollarIcon className="h-6 w-6" />
              </div>
            </div>
            <div className="absolute bottom-0 left-0 right-0 h-1 bg-amber-500" />
          </div>

          {/* Card Vencidas (Morosas) */}
          <div className="relative overflow-hidden bg-slate-800/80 border border-slate-700/50 p-6 rounded-2xl transition-all hover:scale-[1.02]">
            <div className="flex justify-between items-center">
              <div>
                <p className="text-slate-400 text-xs font-semibold uppercase tracking-wider">Deuda Vencida (Morosa)</p>
                <h3 className="text-2xl font-bold mt-2 text-rose-500">${resumen.montoTotalVencido.toLocaleString()}</h3>
                <p className="text-slate-400 text-xs mt-1">{resumen.cuotasVencidas} cuotas vencidas</p>
              </div>
              <div className="p-3 rounded-xl bg-rose-500/10 text-rose-500">
                <BellAlertIcon className="h-6 w-6" />
              </div>
            </div>
            <div className="absolute bottom-0 left-0 right-0 h-1 bg-rose-500" />
          </div>

          {/* Card Socios Morosos */}
          <div className="relative overflow-hidden bg-slate-800/80 border border-slate-700/50 p-6 rounded-2xl transition-all hover:scale-[1.02]">
            <div className="flex justify-between items-center">
              <div>
                <p className="text-slate-400 text-xs font-semibold uppercase tracking-wider">Socios Morosos</p>
                <h3 className="text-2xl font-bold mt-2 text-rose-400">{resumen.totalMorosos}</h3>
                <p className="text-slate-400 text-xs mt-1">Con al menos 1 cuota vencida</p>
              </div>
              <div className="p-3 rounded-xl bg-rose-500/10 text-rose-400">
                <UsersIcon className="h-6 w-6" />
              </div>
            </div>
            <div className="absolute bottom-0 left-0 right-0 h-1 bg-red-400" />
          </div>

          {/* Card Cuotas Pagadas */}
          <div className="relative overflow-hidden bg-slate-800/80 border border-slate-700/50 p-6 rounded-2xl transition-all hover:scale-[1.02]">
            <div className="flex justify-between items-center">
              <div>
                <p className="text-slate-400 text-xs font-semibold uppercase tracking-wider">Cuotas Pagadas</p>
                <h3 className="text-2xl font-bold mt-2 text-emerald-400">{resumen.cuotasPagadas}</h3>
                <p className="text-slate-400 text-xs mt-1">De un total de {resumen.totalCuotas} cuotas</p>
              </div>
              <div className="p-3 rounded-xl bg-emerald-500/10 text-emerald-400">
                <CurrencyDollarIcon className="h-6 w-6" />
              </div>
            </div>
            <div className="absolute bottom-0 left-0 right-0 h-1 bg-emerald-500" />
          </div>
        </div>
      )}
    </div>
  );
}
```

---

### B. Listado de Socios Morosos (`/morosos/page.tsx`)
Muestra el detalle de todos los socios con deudas vencidas, ordenados por monto de mayor a menor.

```tsx
import React, { useEffect, useState } from 'react';
import { cuotaService } from '@/services/cuotaService';
import { MorosoDto } from '@/types/cuota.types';
import Link from 'next/link';

export default function ListaMorosos() {
  const [morosos, setMorosos] = useState<MorosoDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const cargarMorosos = async () => {
      try {
        const data = await cuotaService.obtenerMorosos();
        setMorosos(data);
      } catch (err) {
        console.error("Error al cargar morosos", err);
      } finally {
        setLoading(false);
      }
    };
    cargarMorosos();
  }, []);

  if (loading) return <div className="text-center py-10 text-slate-400">Cargando socios morosos...</div>;

  return (
    <div className="p-6 bg-slate-900 min-h-screen text-white">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-slate-100">Socios en Estado de Morosidad</h2>
        <p className="text-slate-400 text-sm">Listado ordenado de socios con cuotas vencidas y deuda pendiente.</p>
      </div>

      <div className="overflow-hidden border border-slate-700/60 rounded-2xl bg-slate-800/50 backdrop-blur-md">
        <table className="min-w-full divide-y divide-slate-700/50">
          <thead className="bg-slate-850">
            <tr>
              <th className="px-6 py-4 text-left text-xs font-semibold text-slate-400 uppercase">Socio</th>
              <th className="px-6 py-4 text-center text-xs font-semibold text-slate-400 uppercase">Cuotas Vencidas</th>
              <th className="px-6 py-4 text-right text-xs font-semibold text-slate-400 uppercase">Deuda Total</th>
              <th className="px-6 py-4 text-left text-xs font-semibold text-slate-400 uppercase">Vencimiento Más Viejo</th>
              <th className="px-6 py-4 text-center text-xs font-semibold text-slate-400 uppercase">Acción</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-700/50">
            {morosos.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-6 py-10 text-center text-slate-500">
                  ¡Excelente! No hay socios morosos en el sistema.
                </td>
              </tr>
            ) : (
              morosos.map((moroso) => (
                <tr key={moroso.idSocio} className="hover:bg-slate-800/70 transition-colors">
                  <td className="px-6 py-4">
                    <div className="font-semibold text-slate-200">{moroso.nombreSocio}</div>
                    <div className="text-xs text-slate-400">N° Socio: {moroso.numeroSocio} | {moroso.email}</div>
                  </td>
                  <td className="px-6 py-4 text-center">
                    <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium bg-rose-500/10 text-rose-400 border border-rose-500/20">
                      {moroso.cuotasVencidas} cuota(s)
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right text-rose-400 font-bold">
                    ${moroso.deudaTotal.toLocaleString()}
                  </td>
                  <td className="px-6 py-4 text-slate-300 text-sm">
                    {new Date(moroso.fechaVencimientoMasTemprana).toLocaleDateString('es-AR')}
                  </td>
                  <td className="px-6 py-4 text-center">
                    <Link
                      href={`/socios/${moroso.idSocio}`}
                      className="text-xs text-teal-400 hover:text-teal-300 hover:underline font-semibold"
                    >
                      Ver Detalle
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
```

---

### C. Visualizador de Cuotas en el Detalle de la Membresía / Socio (`TablaCuotas.tsx`)
Este componente debe incrustarse en la vista detallada de un Socio o de una Membresía específica para ver la lista de sus cuotas y registrar pagos.

```tsx
import React from 'react';
import { CuotaDto } from '@/types/cuota.types';

interface TablaCuotasProps {
  cuotas: CuotaDto[];
  onGenerarCuotas?: () => void;
  mostrarBotonGenerar?: boolean;
}

export default function TablaCuotas({ cuotas, onGenerarCuotas, mostrarBotonGenerar }: TablaCuotasProps) {
  const getBadgeStyle = (estado: string) => {
    switch (estado) {
      case 'pagada':
        return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/25';
      case 'vencida':
        return 'bg-rose-500/10 text-rose-400 border-rose-500/25 animate-pulse';
      case 'pendiente':
      default:
        return 'bg-amber-500/10 text-amber-400 border-amber-500/25';
    }
  };

  return (
    <div className="bg-slate-850 border border-slate-700/50 rounded-2xl p-6 shadow-xl text-white">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h3 className="text-lg font-bold text-slate-200">Plan de Cuotas Mensuales</h3>
          <p className="text-xs text-slate-400 mt-0.5">Control de vencimientos y estado de facturación.</p>
        </div>
        {mostrarBotonGenerar && cuotas.length === 0 && onGenerarCuotas && (
          <button
            onClick={onGenerarCuotas}
            className="text-xs bg-teal-500 hover:bg-teal-600 text-slate-900 font-bold py-2 px-4 rounded-xl shadow-md transition-all"
          >
            Generar Cuotas
          </button>
        )}
      </div>

      <div className="overflow-hidden border border-slate-700/40 rounded-xl">
        <table className="min-w-full divide-y divide-slate-700/50 text-sm">
          <thead className="bg-slate-800/80">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold text-slate-400 uppercase">N° Cuota</th>
              <th className="px-4 py-3 text-left text-xs font-semibold text-slate-400 uppercase">Período Membresía</th>
              <th className="px-4 py-3 text-right text-xs font-semibold text-slate-400 uppercase">Monto</th>
              <th className="px-4 py-3 text-left text-xs font-semibold text-slate-400 uppercase">Vencimiento</th>
              <th className="px-4 py-3 text-center text-xs font-semibold text-slate-400 uppercase">Estado</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-700/30">
            {cuotas.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-slate-500">
                  No hay cuotas generadas para esta membresía.
                </td>
              </tr>
            ) : (
              cuotas.map((cuota) => (
                <tr key={cuota.id} className="hover:bg-slate-800/30 transition-colors">
                  <td className="px-4 py-3 text-slate-300 font-medium">Cuota {cuota.numeroCuota}</td>
                  <td className="px-4 py-3 text-slate-400 text-xs">{cuota.periodoMembresia}</td>
                  <td className="px-4 py-3 text-right text-slate-200 font-bold">${cuota.monto.toFixed(2)}</td>
                  <td className="px-4 py-3 text-slate-300">
                    {new Date(cuota.fechaVencimiento).toLocaleDateString('es-AR')}
                    {cuota.estado === 'vencida' && (
                      <span className="block text-[10px] text-rose-400 mt-0.5">
                        Hace {cuota.diasVencida} días
                      </span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-center">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border ${getBadgeStyle(cuota.estado)}`}>
                      {cuota.estado.toUpperCase()}
                    </span>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
```

---

## 🛠️ 5. Flujo UX Recomendado para Generación Automática

El flujo principal en el Frontend para el registro de membresías y el control de cobros debe seguir esta lógica:

```
                  ┌───────────────────────────────┐
                  │   Nuevo Registro Membresía    │
                  └───────────────┬───────────────┘
                                  │ POST /api/membresias
                                  ▼
             ┌────────────────────────────────────────┐
             │ Backend crea Membresía                 │
             │   1. Asocia actividades                │
             │   2. Genera Cuotas automáticamente     │
             │   3. Registra Pago de Cuota #1         │
             └────────────────────┬───────────────────┘
                                  │
                                  ▼
             ┌────────────────────────────────────────┐
             │ Redirección en Front a:                │
             │ /socios/{id} o /membresias/{id}        │
             │ (Muestra TablaCuotas con Cuota 1 PAGO) │
             └────────────────────────────────────────┘
```

1. **Inscripción Inicial**: Cuando el administrador registra a un socio mediante `POST /api/membresias`, el backend calcula automáticamente las cuotas e impacta la Cuota 1 como pagada (si se ingresa el pago inicial). El frontend simplemente debe recargar y renderizar el plan de cuotas resultante.
2. **Generación Manual de Respaldo**: En caso de que se necesite regenerar o generar las cuotas para una membresía antigua que no se configuró correctamente, se puede llamar a `POST /api/cuotas/generar/{idMembresia}` desde el botón "Generar Cuotas" (mostrado en `TablaCuotas.tsx`).
3. **Mantenimiento Programado**: Se recomienda añadir un widget persistente en la barra de administración o en la pantalla de finanzas que recuerde al usuario hacer clic en **"Actualizar Vencidas"** para mantener los estados sincronizados, o llamar al endpoint `/api/cuotas/actualizar-vencidas` mediante un cron job diario en el servidor de producción.
