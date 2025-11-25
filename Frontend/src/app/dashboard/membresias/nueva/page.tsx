'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save, Loader2, Search } from 'lucide-react';
import Link from 'next/link';
import { membresiasService, CrearMembresiaDto } from '@/lib/api/membresias';
import { sociosService, type Socio } from '@/lib/api/socios';
import { actividadesService, type Actividad } from '@/lib/api/actividades';

const membresiaSchema = z.object({
  idSocio: z.number().min(1, 'Debe seleccionar un socio'),
  periodoAnio: z.number().min(2020).max(2100),
  periodoMes: z.number().min(1).max(12),
  actividadesIds: z.array(z.number()).min(1, 'Debe seleccionar al menos una actividad'),
});

type MembresiaFormData = z.infer<typeof membresiaSchema>;

export default function NuevaMembresiaPage() {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [socios, setSocios] = useState<Socio[]>([]);
  const [actividades, setActividades] = useState<Actividad[]>([]);
  const [selectedActividades, setSelectedActividades] = useState<number[]>([]);
  const [searchSocio, setSearchSocio] = useState('');
  const [isLoadingSocios, setIsLoadingSocios] = useState(false);

  const currentDate = new Date();
  const [periodoAnio, setPeriodoAnio] = useState(currentDate.getFullYear());
  const [periodoMes, setPeriodoMes] = useState(currentDate.getMonth() + 1);

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<MembresiaFormData>({
    resolver: zodResolver(membresiaSchema),
    defaultValues: {
      periodoAnio,
      periodoMes,
      actividadesIds: [],
    },
  });

  const idSocioWatch = watch('idSocio');

  useEffect(() => {
    cargarActividades();
  }, []);

  const cargarActividades = async () => {
    try {
      const data = await actividadesService.obtenerTodas();
      setActividades(data);
    } catch (error) {
      console.error('Error al cargar actividades:', error);
    }
  };

  const buscarSocios = async () => {
    if (searchSocio.trim().length < 2) return;

    try {
      setIsLoadingSocios(true);
      const data = await sociosService.obtenerTodos({ search: searchSocio, estaActivo: true });
      setSocios(data);
    } catch (error) {
      console.error('Error al buscar socios:', error);
    } finally {
      setIsLoadingSocios(false);
    }
  };

  const toggleActividad = (actividadId: number) => {
    const newSelected = selectedActividades.includes(actividadId)
      ? selectedActividades.filter((id) => id !== actividadId)
      : [...selectedActividades, actividadId];

    setSelectedActividades(newSelected);
    setValue('actividadesIds', newSelected);
  };

  const onSubmit = async (data: MembresiaFormData) => {
    setIsSubmitting(true);
    setError(null);
    setSuccess(null);

    try {
      const membresiaData: CrearMembresiaDto = {
        idSocio: data.idSocio,
        periodoAnio: data.periodoAnio,
        periodoMes: data.periodoMes,
        actividadesIds: data.actividadesIds,
      };

      await membresiasService.crear(membresiaData);
      setSuccess('Membresía creada exitosamente');

      setTimeout(() => {
        router.push('/dashboard/membresias');
      }, 2000);
    } catch (err: any) {
      const errorMessage =
        err.response?.data?.message || err.response?.data || err.message || 'Error al crear la membresía';
      setError(typeof errorMessage === 'string' ? errorMessage : JSON.stringify(errorMessage));
    } finally {
      setIsSubmitting(false);
    }
  };

  const totalMonto = selectedActividades.reduce((sum, actividadId) => {
    const actividad = actividades.find((a) => a.id === actividadId);
    return sum + (actividad?.precio || 0);
  }, 0);

  const meses = [
    'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
  ];

  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-6">
        <Link
          href="/dashboard/membresias"
          className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700"
        >
          <ArrowLeft className="w-4 h-4 mr-1" />
          Volver al listado
        </Link>
      </div>

      <div className="bg-white shadow rounded-lg">
        <div className="px-6 py-4 border-b border-gray-200">
          <h1 className="text-xl font-semibold text-gray-900">Nueva Membresía</h1>
          <p className="text-sm text-gray-500 mt-1">Crea una nueva membresía mensual para un socio</p>
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

          {/* Buscar Socio */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Buscar Socio *
            </label>
            <div className="flex gap-2">
              <div className="flex-1 relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="text"
                  value={searchSocio}
                  onChange={(e) => setSearchSocio(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), buscarSocios())}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
                  placeholder="Buscar por nombre, DNI o número de socio..."
                />
              </div>
              <button
                type="button"
                onClick={buscarSocios}
                disabled={isLoadingSocios}
                className="px-4 py-2 bg-gray-800 text-white rounded-lg hover:bg-gray-900 transition-colors disabled:opacity-50"
              >
                {isLoadingSocios ? 'Buscando...' : 'Buscar'}
              </button>
            </div>

            {/* Lista de socios encontrados */}
            {socios.length > 0 && (
              <div className="mt-2 border border-gray-200 rounded-lg max-h-48 overflow-y-auto">
                {socios.map((socio) => (
                  <button
                    key={socio.id}
                    type="button"
                    onClick={() => {
                      setValue('idSocio', socio.id);
                      setSocios([]);
                      setSearchSocio(`${socio.nombre} ${socio.apellido} - ${socio.numeroSocio}`);
                    }}
                    className={`w-full px-4 py-2 text-left hover:bg-gray-50 transition-colors ${
                      idSocioWatch === socio.id ? 'bg-blue-50' : ''
                    }`}
                  >
                    <p className="text-sm font-medium text-gray-900">
                      {socio.nombre} {socio.apellido}
                    </p>
                    <p className="text-xs text-gray-500">
                      #{socio.numeroSocio} - {socio.email}
                    </p>
                  </button>
                ))}
              </div>
            )}
            {errors.idSocio && (
              <p className="mt-1 text-sm text-red-600">{errors.idSocio.message}</p>
            )}
          </div>

          {/* Período */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label htmlFor="periodoMes" className="block text-sm font-medium text-gray-700">
                Mes *
              </label>
              <select
                id="periodoMes"
                {...register('periodoMes', { valueAsNumber: true })}
                onChange={(e) => setPeriodoMes(parseInt(e.target.value))}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm border px-3 py-2"
              >
                {meses.map((mes, index) => (
                  <option key={index + 1} value={index + 1}>
                    {mes}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label htmlFor="periodoAnio" className="block text-sm font-medium text-gray-700">
                Año *
              </label>
              <input
                type="number"
                id="periodoAnio"
                {...register('periodoAnio', { valueAsNumber: true })}
                onChange={(e) => setPeriodoAnio(parseInt(e.target.value))}
                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm border px-3 py-2"
                min="2020"
                max="2100"
              />
            </div>
          </div>

          {/* Actividades */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-3">
              Actividades * (Selecciona las actividades a incluir)
            </label>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {actividades.map((actividad) => (
                <div
                  key={actividad.id}
                  onClick={() => toggleActividad(actividad.id)}
                  className={`p-4 border-2 rounded-lg cursor-pointer transition-all ${
                    selectedActividades.includes(actividad.id)
                      ? 'border-blue-500 bg-blue-50'
                      : 'border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <p className="font-medium text-gray-900">{actividad.nombre}</p>
                      {actividad.descripcion && (
                        <p className="text-xs text-gray-500 mt-1">{actividad.descripcion}</p>
                      )}
                    </div>
                    <div className="ml-3">
                      <span className="text-lg font-bold text-blue-600">
                        ${actividad.precio.toFixed(2)}
                      </span>
                    </div>
                  </div>
                  <div className="mt-2">
                    <input
                      type="checkbox"
                      checked={selectedActividades.includes(actividad.id)}
                      onChange={() => {}}
                      className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                    />
                  </div>
                </div>
              ))}
            </div>
            {errors.actividadesIds && (
              <p className="mt-1 text-sm text-red-600">{errors.actividadesIds.message}</p>
            )}
          </div>

          {/* Total */}
          {selectedActividades.length > 0 && (
            <div className="bg-gray-50 rounded-lg p-4 border border-gray-200">
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Total mensual:</span>
                <span className="text-2xl font-bold text-blue-600">${totalMonto.toFixed(2)}</span>
              </div>
              <p className="text-xs text-gray-500 mt-2">
                {selectedActividades.length} actividad(es) seleccionada(s)
              </p>
            </div>
          )}

          <div className="flex justify-end space-x-3 pt-4 border-t">
            <Link
              href="/dashboard/membresias"
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
                  Crear Membresía
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
