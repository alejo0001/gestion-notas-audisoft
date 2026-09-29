# 0005 — No permitir eliminar estudiantes o profesores con notas

**Estado:** Aceptada (2026-09-28)

## Contexto

Al eliminar un estudiante o profesor con notas hay tres opciones: borrar en cascada sus notas, dejar las FK en `NULL` o impedir la eliminación. Las notas son un registro académico: perderlas silenciosamente sería un error grave.

## Decisión

- Las FK usan `ON DELETE NO ACTION` (`DeleteBehavior.Restrict` en EF Core).
- El servicio verifica antes de eliminar y devuelve `Conflict` (HTTP 409) con un mensaje claro: *"No se puede eliminar el estudiante porque tiene 3 nota(s) registrada(s). Elimine primero sus notas."*
- Si aun así la base rechaza la operación (condición de carrera), `GlobalExceptionHandler` convierte el `DbUpdateException` en 409.

## Consecuencias

- El usuario debe eliminar primero las notas. La UI muestra la cantidad de notas de cada registro para anticiparlo.
- Alternativa futura: borrado lógico (columna `Activo`), que requeriría modificar el esquema del enunciado.
