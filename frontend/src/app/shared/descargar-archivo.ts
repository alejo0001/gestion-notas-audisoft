import { HttpResponse } from '@angular/common/http';

/**
 * Guarda en el equipo del usuario un archivo recibido del API.
 * El nombre sale de la cabecera Content-Disposition (el API la expone por CORS); si no llega, se usa el
 * nombre por defecto. El navegador descarga a partir de una URL temporal que se libera al terminar.
 */
export function descargarArchivo(response: HttpResponse<Blob>, nombrePorDefecto: string): void {
  if (!response.body) {
    return;
  }

  const url = URL.createObjectURL(response.body);
  const enlace = document.createElement('a');
  enlace.href = url;
  enlace.download = nombreDesdeCabecera(response.headers.get('Content-Disposition')) ?? nombrePorDefecto;
  enlace.click();
  URL.revokeObjectURL(url);
}

/** Extrae el nombre de «attachment; filename=notas.xlsx; filename*=UTF-8''notas.xlsx». */
function nombreDesdeCabecera(contentDisposition: string | null): string | null {
  if (!contentDisposition) {
    return null;
  }
  const utf8 = /filename\*=UTF-8''([^;]+)/i.exec(contentDisposition);
  if (utf8) {
    return decodeURIComponent(utf8[1]);
  }
  const simple = /filename="?([^";]+)"?/i.exec(contentDisposition);
  return simple ? simple[1] : null;
}
