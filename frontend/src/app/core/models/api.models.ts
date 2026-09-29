/** Respuesta paginada estándar del API (ADR 0003). */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type SortDirection = 'asc' | 'desc';

export interface PagedQuery {
  page: number;
  pageSize: number;
  search?: string | null;
  sortBy?: string | null;
  sortDirection?: SortDirection | null;
}

export interface LookupItem {
  id: number;
  nombre: string;
}

/** Formato de error RFC 9457 que devuelve el API (ADR 0004). */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export interface Estudiante {
  id: number;
  nombre: string;
  cantidadNotas: number;
  promedio: number | null;
}

export interface Profesor {
  id: number;
  nombre: string;
  cantidadNotas: number;
}

export interface NombreSaveRequest {
  nombre: string;
}

export interface Nota {
  id: number;
  nombre: string;
  idEstudiante: number;
  estudianteNombre: string;
  idProfesor: number;
  profesorNombre: string;
  valor: number;
}

export interface NotaSaveRequest {
  nombre: string;
  idEstudiante: number;
  idProfesor: number;
  valor: number;
}

export interface NotaQuery extends PagedQuery {
  idEstudiante?: number | null;
  idProfesor?: number | null;
}

export interface PromedioItem {
  id: number;
  nombre: string;
  promedio: number;
  cantidadNotas: number;
}

export interface Resumen {
  totalEstudiantes: number;
  totalProfesores: number;
  totalNotas: number;
  promedioGeneral: number | null;
  notasAprobadas: number;
  notasReprobadas: number;
  mejoresEstudiantes: PromedioItem[];
  promedioPorProfesor: PromedioItem[];
}

export const NOTA_MINIMA_APROBATORIA = 3;
