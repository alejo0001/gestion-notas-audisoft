import { HttpResponse } from '@angular/common/http';
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
import { PagedQuery, PagedResult, Profesor } from '../../core/models/api.models';
import { ProfesoresService } from '../../core/services/api.services';
import { NombreFormDialogComponent, NombreFormDialogData } from '../../shared/nombre-form-dialog.component';
import { PagedListBase } from '../../shared/paged-list.base';

@Component({
  selector: 'app-profesores-list',
  imports: [
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
  templateUrl: './profesores-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfesoresListComponent extends PagedListBase<Profesor> {
  private readonly service = inject(ProfesoresService);
  private readonly dialog = inject(MatDialog);

  protected readonly displayedColumns = ['id', 'nombre', 'cantidadNotas', 'acciones'];

  protected override fetch(query: PagedQuery): Observable<PagedResult<Profesor>> {
    return this.service.getPaged(query);
  }

  protected override readonly archivoExportacion = 'profesores.xlsx';

  protected override descargar(filtros: Omit<PagedQuery, 'page' | 'pageSize'>): Observable<HttpResponse<Blob>> {
    return this.service.exportar(filtros);
  }

  protected crear(): void {
    this.abrirFormulario(
      { entidad: 'profesor', save: (nombre) => this.service.create({ nombre }) },
      'Profesor creado correctamente.',
    );
  }

  protected editar(profesor: Profesor): void {
    this.abrirFormulario(
      {
        entidad: 'profesor',
        nombre: profesor.nombre,
        save: (nombre) => this.service.update(profesor.id, { nombre }),
      },
      'Profesor actualizado correctamente.',
    );
  }

  protected eliminar(profesor: Profesor): void {
    void this.confirmAndDelete(
      'el profesor',
      profesor.nombre,
      this.service.delete(profesor.id),
      'Profesor eliminado correctamente.',
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
