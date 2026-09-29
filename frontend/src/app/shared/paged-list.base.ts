import { HttpResponse } from '@angular/common/http';
import { DestroyRef, Directive, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import {
  Observable,
  Subject,
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  firstValueFrom,
  of,
  switchMap,
} from 'rxjs';
import { PagedQuery, PagedResult } from '../core/models/api.models';
import { NotificationService } from '../core/services/notification.service';
import { descargarArchivo } from './descargar-archivo';

/**
 * Lógica común de las pantallas con tabla paginada en el servidor (ADR 0003):
 * estado con Signals, paginación, ordenamiento, búsqueda con debounce y recarga tras cada acción.
 * Cada pantalla solo implementa fetch() (patrón Template Method).
 */
@Directive()
export abstract class PagedListBase<T> {
  protected readonly notifications = inject(NotificationService);
  protected readonly destroyRef = inject(DestroyRef);

  readonly items = signal<T[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(10);
  readonly sort = signal<Sort>({ active: '', direction: '' });
  readonly search = signal('');
  readonly pageSizeOptions = [5, 10, 25, 50];
  readonly exportando = signal(false);

  private readonly reload$ = new Subject<void>();
  private readonly searchInput$ = new Subject<string>();

  constructor() {
    // switchMap cancela la petición anterior si llega una nueva (p. ej. el usuario cambia de página rápido):
    // así nunca se pinta una respuesta vieja encima de una nueva.
    this.reload$
      .pipe(
        switchMap(() => {
          this.loading.set(true);
          return this.fetch(this.buildQuery()).pipe(
            catchError(() => of(null)), // el interceptor ya mostró la alerta
            finalize(() => this.loading.set(false)),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe((result) => {
        if (!result) {
          return;
        }
        // Si se eliminó el último registro de una página, retroceder una página.
        if (result.items.length === 0 && result.totalCount > 0 && this.pageIndex() > 0) {
          this.pageIndex.set(Math.max(0, result.totalPages - 1));
          this.load();
          return;
        }
        this.items.set(result.items);
        this.totalCount.set(result.totalCount);
      });

    this.searchInput$
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((term) => {
        this.search.set(term.trim());
        this.pageIndex.set(0);
        this.load();
      });

    // Primera carga después de que la subclase termine de construirse.
    queueMicrotask(() => this.load());
  }

  protected abstract fetch(query: PagedQuery): Observable<PagedResult<T>>;

  /** Petición de exportación a Excel del recurso, con los filtros actuales. */
  protected abstract descargar(filtros: Omit<PagedQuery, 'page' | 'pageSize'>): Observable<HttpResponse<Blob>>;

  /** Nombre del archivo si el API no lo envía, p. ej. «notas.xlsx». */
  protected abstract readonly archivoExportacion: string;

  /** Las subclases pueden sobrescribirlo para agregar filtros propios. */
  protected buildQuery(): PagedQuery {
    const sort = this.sort();
    return {
      page: this.pageIndex() + 1,
      pageSize: this.pageSize(),
      search: this.search() || null,
      sortBy: sort.direction ? sort.active : null,
      sortDirection: sort.direction || null,
    };
  }

  load(): void {
    this.reload$.next();
  }

  /** Exporta a Excel lo que muestra la tabla: mismos filtros, búsqueda y orden, pero todas las páginas. */
  exportar(): void {
    // Desestructuración con rest: se descartan page y pageSize y se conserva el resto de filtros.
    const { page: _page, pageSize: _pageSize, ...filtros } = this.buildQuery();

    this.exportando.set(true);
    this.descargar(filtros)
      .pipe(
        finalize(() => this.exportando.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (response) => {
          descargarArchivo(response, this.archivoExportacion);
          this.notifications.success('Archivo de Excel descargado correctamente.');
        },
        error: () => {
          // El interceptor ya mostró la alerta.
        },
      });
  }

  onSearchInput(term: string): void {
    this.searchInput$.next(term);
  }

  clearSearch(input: HTMLInputElement): void {
    input.value = '';
    this.searchInput$.next('');
  }

  onPage(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  onSort(sort: Sort): void {
    this.sort.set(sort);
    this.pageIndex.set(0);
    this.load();
  }

  /** Pide confirmación, elimina, muestra la alerta de éxito y recarga la tabla. */
  protected async confirmAndDelete(
    entity: string,
    name: string,
    request: Observable<unknown>,
    successMessage: string,
  ): Promise<void> {
    if (!(await this.notifications.confirmDelete(entity, name))) {
      return;
    }
    try {
      await firstValueFrom(request, { defaultValue: undefined });
      this.notifications.success(successMessage);
      this.load();
    } catch {
      // El interceptor ya informó el error (por ejemplo, 409 si tiene notas).
    }
  }
}
