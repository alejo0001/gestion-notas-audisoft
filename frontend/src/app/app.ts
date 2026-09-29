import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs';
import { environment } from '../environments/environment';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly menu: MenuItem[] = [
    { label: 'Inicio', icon: 'dashboard', route: '/inicio' },
    { label: 'Estudiantes', icon: 'school', route: '/estudiantes' },
    { label: 'Profesores', icon: 'co_present', route: '/profesores' },
    { label: 'Notas', icon: 'grading', route: '/notas' },
  ];

  protected readonly year = new Date().getFullYear();

  /** Documentación interactiva del API (Swagger UI), derivada de la URL base configurada. */
  protected readonly apiDocsUrl = `${environment.apiUrl.replace(/\/api$/, '')}/swagger`;

  /** En pantallas pequeñas el menú se muestra como panel superpuesto. */
  protected readonly isHandset = toSignal(
    inject(BreakpointObserver)
      .observe([Breakpoints.XSmall, Breakpoints.Small])
      .pipe(map((result) => result.matches)),
    { initialValue: false },
  );
}
