# 0003 — Paginación, búsqueda y ordenamiento del lado del servidor

**Estado:** Aceptada (2026-09-28)

## Contexto

La paginación es requisito explícito del evaluador. Paginar en el cliente (traer todo y cortar en Angular) funciona con pocos datos pero no escala y no demuestra dominio del backend.

## Decisión

- Paginación por **offset** (`page`, `pageSize`) con `Skip/Take` en EF Core, más `COUNT` para el total.
- Respuesta estándar `PagedResult<T>`: `items`, `page`, `pageSize`, `totalCount`, `totalPages`.
- `pageSize` limitado a 1–100 para proteger la API.
- Ordenamiento con lista blanca de columnas (`switch` sobre `sortBy`), nunca SQL dinámico: evita inyección y errores en tiempo de ejecución.
- En Angular, `MatPaginator` y `MatSort` disparan peticiones al servidor; la búsqueda usa `debounceTime` para no saturar la API.

## Consecuencias

- Dos consultas por página (datos + conteo). Aceptable para este volumen.
- La paginación por cursor (keyset) queda descartada: es mejor para feeds infinitos, pero no permite saltar a la página N, que es lo que espera un usuario en una tabla administrativa.
