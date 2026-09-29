import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ProblemDetails } from '../models/api.models';
import { NotificationService } from '../services/notification.service';

/**
 * Manejo centralizado de errores HTTP: lee el ProblemDetails del API y muestra una alerta clara.
 * El error se vuelve a lanzar para que el componente pueda reaccionar (por ejemplo, apagar el "cargando").
 */
export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        showError(notifications, error);
      }
      return throwError(() => error);
    }),
  );
};

function showError(notifications: NotificationService, error: HttpErrorResponse): void {
  if (error.status === 0) {
    notifications.error(
      'Sin conexión con el servidor',
      'No fue posible comunicarse con el API. Verifique que el backend esté en ejecución.',
    );
    return;
  }

  const problem = (error.error ?? {}) as ProblemDetails;

  if (error.status === 400 && problem.errors) {
    const details = Object.values(problem.errors).flat();
    notifications.error('Revise los datos ingresados', undefined, details);
    return;
  }

  const titles: Record<number, string> = {
    404: 'Registro no encontrado',
    409: 'Operación no permitida',
    429: 'Demasiadas peticiones',
  };

  notifications.error(
    problem.title ?? titles[error.status] ?? 'Ocurrió un error',
    problem.detail ?? 'Intente de nuevo más tarde.',
  );
}
