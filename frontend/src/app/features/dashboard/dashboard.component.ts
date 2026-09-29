import { DecimalPipe, PercentPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { NOTA_MINIMA_APROBATORIA, Resumen } from '../../core/models/api.models';
import { ReportesService } from '../../core/services/api.services';

@Component({
  selector: 'app-dashboard',
  imports: [DecimalPipe, PercentPipe, RouterLink, MatCardModule, MatIconModule, MatButtonModule, MatProgressBarModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  protected readonly notaMinima = NOTA_MINIMA_APROBATORIA;

  /** undefined = cargando, null = error (el interceptor ya mostró la alerta). */
  protected readonly resumen = toSignal<Resumen | null>(
    inject(ReportesService)
      .getResumen()
      .pipe(catchError(() => of(null))),
  );

  protected readonly porcentajeAprobacion = computed(() => {
    const r = this.resumen();
    return r && r.totalNotas > 0 ? r.notasAprobadas / r.totalNotas : null;
  });
}
