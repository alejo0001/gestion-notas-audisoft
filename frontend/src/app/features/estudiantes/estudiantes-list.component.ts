import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { Estudiante, NOTA_MINIMA_APROBATORIA, PagedQuery, PagedResult } from '../../core/models/api.models';
import { EstudiantesService } from '../../core/services/api.services';
import { NombreFormDialogComponent, NombreFormDialogData } from '../../shared/nombre-form-dialog.component';
import { PagedListBase } from '../../shared/paged-list.base';

@Component({
  selector: 'app-estudiantes-list',
  imports: [
    DecimalPipe,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
    MatTooltipModule,
  ],
  templateUrl: './estudiantes-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EstudiantesListComponent extends PagedListBase<Estudiante> {
  private readonly service = inject(EstudiantesService);
  private readonly dialog = inject(MatDialog);

  protected readonly displayedColumns = ['id', 'nombre', 'cantidadNotas', 'promedio', 'acciones'];
  protected readonly notaMinima = NOTA_MINIMA_APROBATORIA;

  protected override fetch(query: PagedQuery): Observable<PagedResult<Estudiante>> {
    return this.service.getPaged(query);
  }

  protected crear(): void {
    this.abrirFormulario(
      { entidad: 'estudiante', save: (nombre) => this.service.create({ nombre }) },
      'Estudiante creado correctamente.',
    );
  }

  protected editar(estudiante: Estudiante): void {
    this.abrirFormulario(
      {
        entidad: 'estudiante',
        nombre: estudiante.nombre,
        save: (nombre) => this.service.update(estudiante.id, { nombre }),
      },
      'Estudiante actualizado correctamente.',
    );
  }

  protected eliminar(estudiante: Estudiante): void {
    void this.confirmAndDelete(
      'el estudiante',
      estudiante.nombre,
      this.service.delete(estudiante.id),
      'Estudiante eliminado correctamente.',
    );
  }

  private abrirFormulario(data: NombreFormDialogData, mensajeExito: string): void {
    this.dialog
      .open<NombreFormDialogComponent, NombreFormDialogData, boolean>(NombreFormDialogComponent, {
        data,
        width: '480px',
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
