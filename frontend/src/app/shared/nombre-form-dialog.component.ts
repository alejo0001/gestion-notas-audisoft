import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable } from 'rxjs';

/** Validador: no acepta solo espacios (coincide con la regla del backend). */
export const noSoloEspacios: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = typeof control.value === 'string' ? control.value : '';
  return value.length > 0 && value.trim().length === 0 ? { soloEspacios: true } : null;
};

export interface NombreFormDialogData {
  /** "estudiante" o "profesor": se usa en títulos y mensajes. */
  entidad: string;
  nombre?: string;
  /** Operación de guardado (crear o editar) que ejecuta el diálogo. */
  save: (nombre: string) => Observable<unknown>;
}

/**
 * Formulario reutilizado por Estudiantes y Profesores (ambos solo tienen "nombre").
 * El diálogo ejecuta el guardado y solo se cierra si el API responde bien:
 * si hay error, el usuario no pierde lo que escribió.
 */
@Component({
  selector: 'app-nombre-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ esEdicion ? 'Editar' : 'Nuevo' }} {{ data.entidad }}</h2>

    <!-- [formGroup] es obligatorio: sin él (ngSubmit) no existe y el navegador hace un submit HTML que recarga la página. -->
    <form [formGroup]="form" (ngSubmit)="guardar()">
      <mat-dialog-content>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Nombre completo</mat-label>
          <input matInput [formControl]="nombre" maxlength="100" autocomplete="off" cdkFocusInitial />
          <mat-hint align="end">{{ nombre.value.length }}/100</mat-hint>
          @if (nombre.hasError('required') || nombre.hasError('soloEspacios')) {
            <mat-error>El nombre es obligatorio.</mat-error>
          } @else if (nombre.hasError('minlength')) {
            <mat-error>El nombre debe tener al menos 2 caracteres.</mat-error>
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
    mat-dialog-content { min-width: min(420px, 80vw); padding-top: 8px !important; }
    mat-spinner { display: inline-block; margin-right: 8px; }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NombreFormDialogComponent {
  protected readonly data = inject<NombreFormDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<NombreFormDialogComponent, boolean>);

  protected readonly esEdicion = this.data.nombre !== undefined;
  protected readonly guardando = signal(false);

  protected readonly nombre = new FormControl(this.data.nombre ?? '', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100), noSoloEspacios],
  });

  protected readonly form = new FormGroup({ nombre: this.nombre });

  protected guardar(): void {
    if (this.nombre.invalid) {
      this.nombre.markAsTouched();
      return;
    }

    this.guardando.set(true);
    this.data.save(this.nombre.value.trim()).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => this.guardando.set(false), // el interceptor ya mostró la alerta
    });
  }
}
