import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

/**
 * Punto único para las alertas de la aplicación (SweetAlert2).
 * Los componentes no llaman a Swal directamente: si mañana se cambia la librería, solo cambia este archivo.
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3500,
    timerProgressBar: true,
    didOpen: (popup) => {
      popup.addEventListener('mouseenter', Swal.stopTimer);
      popup.addEventListener('mouseleave', Swal.resumeTimer);
    },
  });

  /** Alerta de éxito tras crear, actualizar o eliminar. */
  success(message: string): void {
    void this.toast.fire({ icon: 'success', title: message });
  }

  error(title: string, message?: string, details: string[] = []): void {
    const list = details.length
      ? `<ul style="text-align:left;margin:12px 0 0">${details.map((d) => `<li>${escapeHtml(d)}</li>`).join('')}</ul>`
      : '';
    void Swal.fire({
      icon: 'error',
      title,
      html: `${message ? escapeHtml(message) : ''}${list}`,
      confirmButtonText: 'Entendido',
      confirmButtonColor: '#1565c0',
    });
  }

  /** Confirmación antes de una acción destructiva. Devuelve true si el usuario confirma. */
  async confirmDelete(entity: string, name: string): Promise<boolean> {
    const result = await Swal.fire({
      icon: 'warning',
      title: `¿Eliminar ${entity}?`,
      html: `Se eliminará <strong>${escapeHtml(name)}</strong>. Esta acción no se puede deshacer.`,
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#c62828',
      reverseButtons: true,
      focusCancel: true,
    });
    return result.isConfirmed;
  }
}

function escapeHtml(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}
