# Changelog

Registro técnico de hitos. Formato: fecha — resumen, con referencia a ADRs y specs.

## 2026-09-29 — Seguridad de dependencias

- `Microsoft.OpenApi` fijado en 2.7.5 (la 2.0.0, transitiva de `Microsoft.AspNetCore.OpenApi` 10.0.0, tenía la vulnerabilidad GHSA-v5pm-xwqc-g5wc, advertencia NU1903).

## 2026-09-28 — Correcciones tras la primera ejecución local

- Corregido: el formulario de estudiante/profesor recargaba la página al pulsar «Crear» y no enviaba nada (faltaba `[formGroup]`, sin él `(ngSubmit)` no existe).
- Nombre único para estudiantes y profesores: validación en servicio (400 en `nombre`) + índice único `IX_Estudiante_Nombre` / `IX_Profesor_Nombre` (spec actualizada). Nuevas pruebas unitarias.
- CORS: en Development se acepta cualquier origen `localhost` (el frontend no conectaba porque Angular tomó otro puerto al estar ocupado el 4200). En producción siguen solo los orígenes de `Cors:AllowedOrigins`.
- Angular sirve en el puerto fijo 4300.
- `01_crear_base_datos.sql` registra la migración `20260928214508_InitialCreate` en `__EFMigrationsHistory`: crear la base con scripts y luego ejecutar el API ya no provoca el error "There is already an object named...".
- Documentación del API: Scalar reemplazado por Swagger UI (`/swagger`), leyendo el mismo documento OpenAPI nativo de .NET (`/openapi/v1.json`).

## 2026-09-28 — Estructura inicial completa

- Documentación base: `AGENTS.md`, ADRs 0001–0006, specs de CRUD de estudiantes/profesores, CRUD de notas y dashboard.
- Backend .NET 10 en cuatro capas (ADR 0001): entidades, servicios con `Result<T>` (ADR 0004), validación con FluentValidation, paginación/búsqueda/orden en servidor (ADR 0003), `GlobalExceptionHandler` con ProblemDetails, OpenAPI + Scalar, CORS y health check.
- Persistencia con EF Core 10 + SQL Server: tablas `Estudiante`, `Profesor`, `Nota` con `FK_Nota_Estudiante`, `FK_Nota_Profesor` (NO ACTION, ADR 0005) y `CK_Nota_Valor`; datos semilla (ADR 0002).
- Scripts SQL equivalentes en `database/`.
- Pruebas unitarias xUnit de `EstudianteService` y `NotaService`.
- Frontend Angular 20 (ADR 0006): menú lateral, dashboard, páginas de Estudiantes, Profesores y Notas con tabla paginada, búsqueda, orden, filtros, diálogos de creación/edición, confirmación de eliminación y alertas de éxito/error.
- Estado: pendiente de compilación y verificación end-to-end en la máquina local.
