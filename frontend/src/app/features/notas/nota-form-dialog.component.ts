import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
import { LookupItem, Nota, NotaSaveRequest } from '../../core/models/api.models';
import { noSoloEspacios } from '../../shared/nombre-form-dialog.component';

export interface NotaFormDialogData {
  nota?: Nota;
  estudiantes: LookupItem[];
  profesores: LookupItem[];
  save: (request: NotaSaveRequest) => Observable<unknown>;
}

/** Máximo dos decimales (coincide con DECIMAL(3,2) en la base de datos). */
function maxDosDecimales(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (value === null || value === undefined || value === '') {
    return null;
  }
  return Math.round(Number(value) * 100) / 100 === Number(value) ? null : { decimales: true };
}

@Component({
  selector: 'app-nota-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ esEdicion ? 'Editar nota' : 'Nueva nota' }}</h2>

    <form [formGroup]="form" (ngSubmit)="guardar()">
      <mat-dialog-content class="grid">
        <mat-form-field appearance="outline" class="span-2">
          <mat-label>Nombre de la evaluación</mat-label>
          <input matInput formControlName="nombre" maxlength="100" placeholder="Ej.: Parcial 1" autocomplete="off" />
          @if (form.controls.nombre.invalid) {
            <mat-error>Ingrese un nombre de al menos 2 caracteres.</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="span-2">
          <mat-label>Estudiante</mat-label>
          <mat-select formControlName="idEstudiante">
            @for (e of data.estudiantes; track e.id) {
              <mat-option [value]="e.id">{{ e.nombre }}</mat-option>
            }
          </mat-select>
          <mat-error>Seleccione un estudiante.</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Profesor</mat-label>
          <mat-select formControlName="idProfesor">
            @for (p of data.profesores; track p.id) {
              <mat-option [value]="p.id">{{ p.nombre }}</mat-option>
            }
          </mat-select>
          <mat-error>Seleccione un profesor.</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Valor (0,0 a 5,0)</mat-label>
          <input matInput type="number" formControlName="valor" min="0" max="5" step="0.1" />
          @if (form.controls.valor.hasError('decimales')) {
            <mat-error>Máximo 2 decimales.</mat-error>
          } @else {
            <mat-error>La nota debe estar entre 0,0 y 5,0.</mat-error>
          }
        </mat-form-field>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" mat-dialog-close [disabled]="guardando()">Cancelar</button>
        <button mat-flat-button type="submit" [disabled]="guardando()">
          @if (guardando()) {
            <mat-spinner diameter="18" />
          } @else {
            <mat-icon>save</mat-icon>
          }
          {{ esEdicion ? 'Guardar cambios' : 'Crear' }}
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: `
    .grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      column-gap: 16px;
      min-width: min(520px, 80vw);
      padding-top: 8px !important;
    }
    .span-2 { grid-column: span 2; }
    mat-spinner { display: inline-block; margin-right: 8px; }
    @media (max-width: 600px) {
      .grid { grid-template-columns: 1fr; }
      .span-2 { grid-column: auto; }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotaFormDialogComponent {
  protected readonly data = inject<NotaFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<NotaFormDialogComponent, boolean>);

  protected readonly esEdicion = this.data.nota !== undefined;
  protected readonly guardando = signal(false);

  protected readonly form = inject(FormBuilder).group({
    nombre: [
      this.data.nota?.nombre ?? '',
      [Validators.required, Validators.minLength(2), Validators.maxLength(100), noSoloEspacios],
    ],
    idEstudiante: [this.data.nota?.idEstudiante ?? (null as number | null), Validators.required],
    idProfesor: [this.data.nota?.idProfesor ?? (null as number | null), Validators.required],
    valor: [
      this.data.nota?.valor ?? (null as number | null),
      [Validators.required, Validators.min(0), Validators.max(5), maxDosDecimales],
    ],
  });

  protected guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { nombre, idEstudiante, idProfesor, valor } = this.form.getRawValue();
    this.guardando.set(true);
    this.data
      .save({
        nombre: (nombre ?? '').trim(),
        idEstudiante: idEstudiante!,
        idProfesor: idProfesor!,
        valor: Number(valor),
      })
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: () => this.guardando.set(false),
      });
  }
}
