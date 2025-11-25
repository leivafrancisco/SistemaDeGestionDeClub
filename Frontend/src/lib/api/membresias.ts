import apiClient from './client';

export interface MembresiaActividad {
  idActividad: number;
  nombreActividad: string;
  precioAlMomento: number;
}

export interface Membresia {
  id: number;
  idSocio: number;
  nombreSocio: string;
  numeroSocio: string;
  periodoAnio: number;
  periodoMes: number;
  fechaInicio: string;
  fechaFin: string;
  totalCargado: number;
  totalPagado: number;
  saldo: number;
  actividades: MembresiaActividad[];
  fechaCreacion: string;
}

export interface CrearMembresiaDto {
  idSocio: number;
  periodoAnio: number;
  periodoMes: number;
  actividadesIds: number[];
}

export interface AsignarActividadDto {
  idMembresia: number;
  idActividad: number;
}

export interface RemoverActividadDto {
  idMembresia: number;
  idActividad: number;
}

export const membresiasService = {
  async obtenerTodas(idSocio?: number): Promise<Membresia[]> {
    const params = idSocio ? `?idSocio=${idSocio}` : '';
    const response = await apiClient.get<Membresia[]>(`/membresias${params}`);
    return response.data;
  },

  async obtenerPorId(id: number): Promise<Membresia> {
    const response = await apiClient.get<Membresia>(`/membresias/${id}`);
    return response.data;
  },

  async crear(datos: CrearMembresiaDto): Promise<Membresia> {
    const response = await apiClient.post<Membresia>('/membresias', datos);
    return response.data;
  },

  async asignarActividad(datos: AsignarActividadDto): Promise<void> {
    await apiClient.post('/membresias/asignar-actividad', datos);
  },

  async removerActividad(datos: RemoverActividadDto): Promise<void> {
    await apiClient.post('/membresias/remover-actividad', datos);
  },

  async eliminar(id: number): Promise<void> {
    await apiClient.delete(`/membresias/${id}`);
  },
};

export default membresiasService;
