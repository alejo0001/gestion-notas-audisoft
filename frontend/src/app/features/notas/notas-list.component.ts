import { HttpResponse } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
import {
  LookupItem,
  NOTA_MINIMA_APROBATORIA,
  Nota,
  NotaQuery,
  PagedQuery,
  PagedResult,
} from '../../core/models/api.models';
import { EstudiantesService, NotasService, ProfesoresService } from '../../core/services/api.services';
import { PagedListBase } from '../../shared/paged-list.base';
import { NotaFormDialogComponent, NotaFormDialogData } from './nota-form-dialog.component';

@Component({
  selector: 'app-notas-list',
  imports: [
    DecimalPipe,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressBarModule,
    MatTooltipModule,
  ],
  templateUrl: './notas-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotasListComponent extends PagedListBase<Nota> {
  private readonly service = inject(NotasService);
  private readonly estudiantesService = inject(EstudiantesService);
  private readonly profesoresService = inject(ProfesoresService);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);

  protected readonly displayedColumns = ['id', 'nombre', 'estudiante', 'profesor', 'valor', 'acciones'];
  protected readonly notaMinima = NOTA_MINIMA_APROBATORIA;

  protected readonly estudiantes = signal<LookupItem[]>([]);
  protected readonly profesores = signal<LookupItem[]>([]);

  /** Filtros iniciales desde la URL, p. ej. /notas?idEstudiante=3 (enlace desde Estudiantes). */
  protected readonly filtroEstudiante = signal<number | null>(
    toNumberOrNull(this.route.snapshot.queryParamMap.get('idEstudiante')),
  );
  protected readonly filtroProfesor = signal<number | null>(
    toNumberOrNull(this.route.snapshot.queryParamMap.get('idProfesor')),
  );

  constructor() {
    super();
    forkJoin({
      estudiantes: this.estudiantesService.getLookup(),
      profesores: this.profesoresService.getLookup(),
    })
      .pipe(takeUntilDestroyed())
      .subscribe(({ estudiantes, profesores }) => {
        this.estudiantes.set(estudiantes);
        this.profesores.set(profesores);
      });
  }

  protected override fetch(query: PagedQuery): Observable<PagedResult<Nota>> {
    return this.service.getPaged(query as NotaQuery);
  }

  protected override readonly archivoExportacion = 'notas.xlsx';

  protected override descargar(filtros: Omit<PagedQuery, 'page' | 'pageSize'>): Observable<HttpResponse<Blob>> {
    return this.service.exportar(filtros as Omit<NotaQuery, 'page' | 'pageSize'>);
  }

  protected override buildQuery(): NotaQuery {
    return {
      ...super.buildQuery(),
      idEstudiante: this.filtroEstudiante(),
      idProfesor: this.filtroProfesor(),
    };
  }

  protected onFiltroEstudiante(id: number | null): void {
    this.filtroEstudiante.set(id);
    this.pageIndex.set(0);
    this.load();
  }

  protected onFiltroProfesor(id: number | null): void {
    this.filtroProfesor.set(id);
    this.pageIndex.set(0);
    this.load();
  }

  protected crear(): void {
    this.abrirFormulario(
      {
        estudiantes: this.estudiantes(),
        profesores: this.profesores(),
        save: (request) => this.service.create(request),
      },
      'Nota creada correctamente.',
    );
  }

  protected editar(nota: Nota): void {
    this.abrirFormulario(
      {
        nota,
        estudiantes: this.estudiantes(),
        profesores: this.profesores(),
        save: (request) => this.service.update(nota.id, request),
      },
      'Nota actualizada correctamente.',
    );
  }

  protected eliminar(nota: Nota): void {
    void this.confirmAndDelete(
      'la nota',
      `${nota.nombre} de ${nota.estudianteNombre}`,
      this.service.delete(nota.id),
      'Nota eliminada correctamente.',
    );
  }

  private abrirFormulario(data: NotaFormDialogData, mensajeExito: string): void {
    this.dialog
      .open<NotaFormDialogComponent, NotaFormDialogData, boolean>(NotaFormDialogComponent, {
        data,
        width: '600px',
        disableClose: true,
      })
      .afterClosed()
      .subscribe((guardado) => {
        if (guardado) {
          this.notifications.success(mensajeExito);
          this.load();
        }
      });
  }
}

function toNumberOrNull(value: string | null): number | null {
  const parsed = Number(value);
  return value && Number.isInteger(parsed) && parsed > 0 ? parsed : null;
}
