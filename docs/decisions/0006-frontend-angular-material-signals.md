# 0006 — Frontend: Angular standalone + Signals + Angular Material + SweetAlert2

**Estado:** Aceptada (2026-09-28)

## Contexto

El enunciado sugiere Angular. Se necesita: menú de navegación, tablas con paginación, formularios de crear/editar y alertas visibles de cada acción.

## Decisión

- **Angular 20** con componentes standalone, rutas con carga diferida (`loadComponent`) y detección de cambios **zoneless** basada en **Signals**.
- **Angular Material (Material 3)** para tabla, paginador, ordenamiento, diálogos, formularios y menú lateral: componentes accesibles y consistentes sin construir UI desde cero.
- **SweetAlert2** para las alertas de éxito (toast) y la confirmación antes de eliminar: son más visibles que un snackbar, que es justo lo que pidió el evaluador.
- Un `HttpInterceptor` centraliza el manejo de errores HTTP y muestra el mensaje del ProblemDetails.
- Una clase base genérica `PagedListBase<T>` concentra la lógica de paginación/búsqueda/ordenamiento que comparten las tres pantallas.

## Consecuencias

- Actualizar a la versión mayor más reciente de Angular es un `ng update` guiado; se deja como mejora.
- SweetAlert2 agrega ~40 KB al bundle; aceptable.
