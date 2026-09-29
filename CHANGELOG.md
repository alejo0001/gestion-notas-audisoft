# Changelog

Registro técnico de hitos. Formato: fecha — resumen, con referencia a ADRs y specs.

## 2026-09-29 — Preparación del despliegue en Azure

- `AppDbContextDesignTimeFactory` (`IDesignTimeDbContextFactory`): las herramientas de EF (migrations bundle en el pipeline) crean el DbContext sin arrancar el API ni leer `appsettings.json`.
- Workflow: `dotnet restore` antes del bundle, paso de firewall tolerante a fallos (los runners corren en Azure y `AllowAzureServices` les da acceso), acciones actualizadas a v5.
- Credencial federada adicional para el formato de *subject* de GitHub con IDs inmutables (`repo:owner@id/repo@id:environment:produccion`).
- ADR 0007 y guía `docs/despliegue-azure.md`: Static Web Apps (Free) + Container Apps (escala a cero) + Azure SQL (oferta gratuita).
- `backend/Dockerfile` multi-etapa (SDK → runtime ASP.NET, usuario sin privilegios) y `.dockerignore`.
- Workflow `.github/workflows/ci-cd.yml`: pruebas en PR; en `main` imagen a ghcr.io, migraciones con `ef migrations bundle` (firewall temporal), actualización del Container App y publicación del frontend. Login a Azure con OIDC.
- `Program.cs`: Swagger habilitable en producción (`Swagger:Enabled`); migraciones al iniciar solo en Development; se quita `UseHttpsRedirection` (en Azure el ingress termina TLS).
- Frontend: `environment.ts` con marcador `__API_URL__` (lo reemplaza el pipeline) y `staticwebapp.config.json` para las rutas de Angular.

## 2026-09-29 — Angular 22

- Frontend actualizado de Angular 20 a 22 (20 → 21 → 22 con `ng update`), Angular Material/CDK 22 y TypeScript 6. Migraciones opcionales omitidas: el proyecto ya usaba el builder `@angular/build` y no tiene pruebas Karma ni usa `Router.getCurrentNavigation`.
- Corregido: las alertas de SweetAlert2 quedaban detrás de los diálogos de Material (CDK 21+ usa la top layer del navegador). Solución: `topLayer: true` en `NotificationService` (ADR 0006).
- `.gitattributes` para normalizar finales de línea (los archivos generados por Visual Studio usan CRLF).
- Eliminado `docker-compose.yml` (SQL Server en contenedor): nunca se usó; en local se usa LocalDB y en la nube Azure SQL. El enunciado de la prueba se retiró del repositorio.

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
