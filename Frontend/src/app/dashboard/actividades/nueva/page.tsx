'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';
import Link from 'next/link';
import { actividadesService, CrearActividadDto } from '@/lib/api/actividades';

const actividadSchema = z.object({
  nombre: z
    .string()
    .min(3, 'El nombre debe tener al menos 3 caracteres')
    .max(100, 'El nombre no puede exceder 100 caracteres'),
  descripcion: z
    .string()
    .max(500, 'La descripción no puede exceder 500 caracteres')
    .optional()
    .or(z.literal('')),
  precio: z
    .string()
    .min(1, 'El precio es requerido')
    .refine((val) => !isNaN(Number(val)) && Number(val) >= 0, {
      message: 'El precio debe ser un número válido mayor o igual a 0',
    }),
  esCuotaBase: z.boolean().default(false),
});

type ActividadFormData = z.infer<typeof actividadSchema>;

export default function NuevaActividadPage() {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
  } = useForm<ActividadFormData>({
    resolver: zodResolver(actividadSchema),
    defaultValues: {
      esCuotaBase: false,
    },
  });

  const esCuotaBase = watch('esCuotaBase');

  const onSubmit = async (data: ActividadFormData) => {
    setIsSubmitting(true);
    setError(null);
    setSuccess(null);

    try {
      const actividadData: CrearActividadDto = {
        nombre: data.nombre.trim(),
        descripcion: data.descripcion?.trim() || undefined,
        precio: Number(data.precio),
        esCuotaBase: data.esCuotaBase,
      };

      const nuevaActividad = await actividadesService.crear(actividadData);
      setSuccess(
        `Actividad "${nuevaActividad.nombre}" creada exitosamente con precio $${nuevaActividad.precio}`
      );

      // Redirigir después de 2 segundos
      setTimeout(() => {
        router.push('/dashboard/actividades');
      }, 2000);
    } catch (err: any) {
      console.error('Error completo:', err);
      console.error('Error response:', err.response);
      console.error('Error response data:', err.response?.data);
      const errorMessage =
        err.response?.data?.message ||
        err.response?.data ||
        err.message ||
        'Error al crear la actividad';
      setError(typeof errorMessage === 'string' ? errorMessage : JSON.stringify(errorMessage));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto">
      <div className="mb-6">
        <Link
          href="/dashboard/actividades"
          className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700"
        >
          <ArrowLeft className="w-4 h-4 mr-1" />
          Volver al listado
        </Link>
      </div>

      <div className="bg-white shadow rounded-lg">
        <div className="px-6 py-4 border-b border-gray-200">
          <h1 className="text-xl font-semibold text-gray-900">Nueva Actividad</h1>
          <p className="text-sm text-gray-500 mt-1">
            Crea una nueva actividad o servicio del club
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-6">
          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          {success && (
            <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded">
              {success}
            </div>
          )}

          <div className="space-y-6">
            <div>
              <label htmlFor="nombre" className="block text-sm font-medium text-gray-700">
                Nombre de la Actividad *
              </label>
              <input
                type="text"
                id="nombre"
                {...register('nombre')}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm border px-3 py-2"
                placeholder="Ej: Fútbol 5, Gimnasio, Natación"
              />
              {errors.nombre && (
                <p className="mt-1 text-sm text-red-600">{errors.nombre.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="descripcion" className="block text-sm font-medium text-gray-700">
                Descripción
              </label>
              <textarea
                id="descripcion"
                {...register('descripcion')}
                rows={4}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm border px-3 py-2"
                placeholder="Descripción de la actividad (opcional)"
              />
              {errors.descripcion && (
                <p className="mt-1 text-sm text-red-600">{errors.descripcion.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="precio" className="block text-sm font-medium text-gray-700">
                Precio Mensual *
              </label>
              <div className="mt-1 relative rounded-md shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <span className="text-gray-500 sm:text-sm">$</span>
                </div>
                <input
                  type="number"
                  id="precio"
                  step="0.01"
                  min="0"
                  {...register('precio')}
                  className="block w-full pl-7 pr-12 rounded-md border-gray-300 focus:border-primary-500 focus:ring-primary-500 sm:text-sm border px-3 py-2"
                  placeholder="0.00"
                />
              </div>
              {errors.precio && (
                <p className="mt-1 text-sm text-red-600">{errors.precio.message}</p>
              )}
            </div>

            <div className="flex items-start">
              <div className="flex items-center h-5">
                <input
                  id="esCuotaBase"
                  type="checkbox"
                  {...register('esCuotaBase')}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                />
              </div>
              <div className="ml-3 text-sm">
                <label htmlFor="esCuotaBase" className="font-medium text-gray-700">
                  Es cuota base
                </label>
                <p className="text-gray-500">
                  Marca esta opción si es la cuota mensual obligatoria del club
                </p>
              </div>
            </div>

            {esCuotaBase && (
              <div className="bg-yellow-50 border border-yellow-200 rounded-md p-4">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <svg
                      className="h-5 w-5 text-yellow-400"
                      viewBox="0 0 20 20"
                      fill="currentColor"
                    >
                      <path
                        fillRule="evenodd"
                        d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                        clipRule="evenodd"
                      />
                    </svg>
                  </div>
                  <div className="ml-3">
                    <h3 className="text-sm font-medium text-yellow-800">Cuota Base</h3>
                    <div className="mt-2 text-sm text-yellow-700">
                      <p>
                        Esta actividad se aplicará automáticamente a todas las membresías como
                        cuota base del club.
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          <div className="flex justify-end space-x-3 pt-4 border-t">
            <Link
              href="/dashboard/actividades"
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
            >
              Cancelar
            </Link>
            <button
              type="submit"
              disabled={isSubmitting}
              className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Guardando...
                </>
              ) : (
                <>
                  <Save className="w-4 h-4 mr-2" />
                  Guardar Actividad
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
