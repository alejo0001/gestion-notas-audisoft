import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Estudiante,
  LookupItem,
  NombreSaveRequest,
  Nota,
  NotaQuery,
  NotaSaveRequest,
  Profesor,
  Resumen,
} from '../models/api.models';
import { CrudApiService } from './crud-api.service';

@Injectable({ providedIn: 'root' })
export class EstudiantesService extends CrudApiService<Estudiante, NombreSaveRequest> {
  constructor() {
    super('estudiantes');
  }

  getLookup(): Observable<LookupItem[]> {
    return this.http.get<LookupItem[]>(`${this.baseUrl}/lookup`);
  }
}

@Injectable({ providedIn: 'root' })
export class ProfesoresService extends CrudApiService<Profesor, NombreSaveRequest> {
  constructor() {
    super('profesores');
  }

  getLookup(): Observable<LookupItem[]> {
    return this.http.get<LookupItem[]>(`${this.baseUrl}/lookup`);
  }
}

@Injectable({ providedIn: 'root' })
export class NotasService extends CrudApiService<Nota, NotaSaveRequest, NotaQuery> {
  constructor() {
    super('notas');
  }
}

/** Reportes no es un recurso CRUD: no hereda de CrudApiService (no expondría métodos que el API no tiene). */
@Injectable({ providedIn: 'root' })
export class ReportesService {
  private readonly http = inject(HttpClient);

  getResumen(): Observable<Resumen> {
    return this.http.get<Resumen>(`${environment.apiUrl}/reportes/resumen`);
  }
}
