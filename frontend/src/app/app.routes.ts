import { Routes } from '@angular/router';

// Carga diferida: cada página se descarga solo cuando el usuario navega a ella.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'inicio' },
  {
    path: 'inicio',
    title: 'Inicio · Gestión de Notas',
    loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'estudiantes',
    title: 'Estudiantes · Gestión de Notas',
    loadComponent: () =>
      import('./features/estudiantes/estudiantes-list.component').then((m) => m.EstudiantesListComponent),
  },
  {
    path: 'profesores',
    title: 'Profesores · Gestión de Notas',
    loadComponent: () =>
      import('./features/profesores/profesores-list.component').then((m) => m.ProfesoresListComponent),
  },
  {
    path: 'notas',
    title: 'Notas · Gestión de Notas',
    loadComponent: () => import('./features/notas/notas-list.component').then((m) => m.NotasListComponent),
  },
  { path: '**', redirectTo: 'inicio' },
];
