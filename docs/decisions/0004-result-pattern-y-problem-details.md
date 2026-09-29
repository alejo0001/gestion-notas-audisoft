# 0004 — Result pattern en servicios y ProblemDetails en la API

**Estado:** Aceptada (2026-09-28)

## Contexto

Los servicios tienen resultados esperados que no son excepcionales: registro no encontrado, datos inválidos, conflicto al eliminar. Lanzar excepciones para flujo normal es costoso y oculta el contrato del método.

## Decisión

- Los servicios devuelven `Result<T>` / `Result` con un `Error` tipado (`NotFound`, `Validation`, `Conflict`).
- Los controladores traducen el error a HTTP con **ProblemDetails (RFC 9457)**: 404, 400 (`ValidationProblemDetails` con errores por campo) y 409.
- Las fallas realmente excepcionales (base de datos caída, timeouts) sí se propagan y las atrapa `GlobalExceptionHandler` (`IExceptionHandler`), que registra la excepción completa y responde 500 sin filtrar detalles internos.
- La validación de entrada se hace con FluentValidation dentro del servicio, con mensajes en español.
- El frontend lee `detail` / `errors` del ProblemDetails para mostrar la alerta al usuario.

## Consecuencias

- Firmas de métodos honestas: quien llama sabe que puede fallar sin leer la implementación.
- Un solo formato de error para todo el API, fácil de consumir desde Angular.
