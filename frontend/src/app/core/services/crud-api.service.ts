import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedQuery, PagedResult } from '../models/api.models';

/**
 * Servicio genérico para un recurso REST con CRUD + paginación.
 * Cada recurso concreto solo define su ruta; así no se repite el mismo código HTTP tres veces.
 */
export abstract class CrudApiService<TDto, TSave, TQuery extends PagedQuery = PagedQuery> {
  protected readonly http = inject(HttpClient);
  protected readonly baseUrl: string;

  protected constructor(resource: string) {
    this.baseUrl = `${environment.apiUrl}/${resource}`;
  }

  getPaged(query: TQuery): Observable<PagedResult<TDto>> {
    return this.http.get<PagedResult<TDto>>(this.baseUrl, { params: toHttpParams(query) });
  }

  getById(id: number): Observable<TDto> {
    return this.http.get<TDto>(`${this.baseUrl}/${id}`);
  }

  create(body: TSave): Observable<TDto> {
    return this.http.post<TDto>(this.baseUrl, body);
  }

  update(id: number, body: TSave): Observable<TDto> {
    return this.http.put<TDto>(`${this.baseUrl}/${id}`, body);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Descarga el Excel con los mismos filtros, búsqueda y orden de la tabla (sin paginar).
   * observe: 'response' para leer también las cabeceras (nombre del archivo en Content-Disposition).
   */
  exportar(filtros: Omit<TQuery, 'page' | 'pageSize'>): Observable<HttpResponse<Blob>> {
    return this.http.get(`${this.baseUrl}/exportar`, {
      params: toHttpParams(filtros),
      observe: 'response',
      responseType: 'blob',
    });
  }
}

/** Convierte un objeto de filtros en query string, omitiendo valores vacíos. */
export function toHttpParams(query: object): HttpParams {
  let params = new HttpParams();
  for (const [key, value] of Object.entries(query)) {
    if (value !== null && value !== undefined && value !== '') {
      params = params.set(key, String(value));
    }
  }
  return params;
}
