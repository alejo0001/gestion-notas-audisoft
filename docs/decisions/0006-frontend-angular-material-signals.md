# 0006 — Frontend: Angular standalone + Signals + Angular Material + SweetAlert2

**Estado:** Aceptada (2026-09-28)

## Contexto

El enunciado sugiere Angular. Se necesita: menú de navegación, tablas con paginación, formularios de crear/editar y alertas visibles de cada acción.

## Decisión

- **Angular 22** (versión en soporte activo; se actualizó desde la 20 con `ng update`, una versión mayor a la vez) con componentes standalone, rutas con carga diferida (`loadComponent`) y detección de cambios **zoneless** basada en **Signals**.
- **Angular Material (Material 3)** para tabla, paginador, ordenamiento, diálogos, formularios y menú lateral: componentes accesibles y consistentes sin construir UI desde cero.
- **SweetAlert2** para las alertas de éxito (toast) y la confirmación antes de eliminar: son más visibles que un snackbar, que es justo lo que pidió el evaluador.
- Un `HttpInterceptor` centraliza el manejo de errores HTTP y muestra el mensaje del ProblemDetails.
- Una clase base genérica `PagedListBase<T>` concentra la lógica de paginación/búsqueda/ordenamiento que comparten las tres pantallas.

## Consecuencias

- Cada versión mayor tiene 12 meses de soporte activo + 12 de LTS: actualizar una versión por vez con `ng update` para que corran sus migraciones.
- Desde CDK 21 los diálogos usan la "top layer" del navegador (popover nativo). Toda alerta o capa que deba verse sobre un diálogo debe usar también la top layer (en SweetAlert2: `topLayer: true`).
- SweetAlert2 agrega ~40 KB al bundle; aceptable.
