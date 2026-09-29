import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';

export type Tema = 'light' | 'dark';

const CLAVE_STORAGE = 'gestion-notas.tema';

/**
 * Tema claro/oscuro de la aplicación.
 *
 * Angular Material 3 genera cada color con la función CSS light-dark(claro, oscuro), y el navegador
 * elige uno u otro según la propiedad `color-scheme` del documento. Cambiar de tema es, entonces,
 * cambiar esa propiedad en <html>: no hay hojas de estilo duplicadas ni recarga de la página.
 *
 * La primera vez se usa la preferencia del sistema operativo; después, la última elección del usuario.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  readonly tema = signal<Tema>(this.temaInicial());
  readonly esOscuro = computed(() => this.tema() === 'dark');

  constructor() {
    // effect: se ejecuta al iniciar y cada vez que cambia la señal `tema`.
    effect(() => {
      const tema = this.tema();
      this.document.documentElement.style.colorScheme = tema;
      guardar(tema);
    });
  }

  alternar(): void {
    this.tema.update((actual) => (actual === 'dark' ? 'light' : 'dark'));
  }

  private temaInicial(): Tema {
    const guardado = leer();
    if (guardado) {
      return guardado;
    }
    const prefiereOscuro = this.document.defaultView?.matchMedia?.('(prefers-color-scheme: dark)').matches;
    return prefiereOscuro ? 'dark' : 'light';
  }
}

// localStorage puede no estar disponible (modo privado, cookies bloqueadas): la preferencia es opcional.
function leer(): Tema | null {
  try {
    const valor = localStorage.getItem(CLAVE_STORAGE);
    return valor === 'dark' || valor === 'light' ? valor : null;
  } catch {
    return null;
  }
}

function guardar(tema: Tema): void {
  try {
    localStorage.setItem(CLAVE_STORAGE, tema);
  } catch {
    // Sin almacenamiento el tema funciona igual; solo no se recuerda para la próxima visita.
  }
}
