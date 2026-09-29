import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  LOCALE_ID,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeEsCo from '@angular/common/locales/es-CO';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withComponentInputBinding, withViewTransitions } from '@angular/router';
import { routes } from './app.routes';
import { httpErrorInterceptor } from './core/interceptors/http-error.interceptor';
import { SpanishPaginatorIntl } from './shared/spanish-paginator-intl';

registerLocaleData(localeEsCo);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    // Compatibilidad con componentes de Material que aún usen @angular/animations (carga diferida).
    provideAnimationsAsync(),
    provideRouter(routes, withComponentInputBinding(), withViewTransitions()),
    provideHttpClient(withFetch(), withInterceptors([httpErrorInterceptor])),
    { provide: LOCALE_ID, useValue: 'es-CO' },
    { provide: MatPaginatorIntl, useClass: SpanishPaginatorIntl },
  ],
};
