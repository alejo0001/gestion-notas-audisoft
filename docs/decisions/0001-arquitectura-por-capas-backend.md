# 0001 — Arquitectura por capas en el backend

**Estado:** Aceptada (2026-09-28)

## Contexto

La prueba pide una API REST con CRUD sobre tres tablas. Un solo proyecto funcionaría, pero el evaluador pidió explícitamente que "no sea tan básica", y la separación de responsabilidades es uno de los temas que más se evalúan en entrevistas .NET. Al mismo tiempo, no queremos sobre-arquitectura (CQRS, MediatR, repositorios genéricos sobre EF Core, etc.) para tres entidades.

## Decisión

Cuatro proyectos con dependencias en una sola dirección:

```
Api ──► Application ──► Domain
 └────► Infrastructure ──► Application
```

- **Domain:** entidades (`Estudiante`, `Profesor`, `Nota`). Sin dependencias.
- **Application:** DTOs, servicios de caso de uso, validadores (FluentValidation), `Result<T>`, paginación e `IApplicationDbContext`.
- **Infrastructure:** `AppDbContext` (EF Core + SQL Server), configuraciones Fluent API, datos semilla y migraciones.
- **Api:** controladores delgados, manejo global de excepciones, OpenAPI, CORS y health checks.

No se usa patrón Repository: `DbContext` ya es una unidad de trabajo y `DbSet` ya es un repositorio. Application depende de la abstracción `IApplicationDbContext` (inversión de dependencias), lo que permite probar los servicios con el proveedor InMemory.

## Consecuencias

- Controladores sin lógica de negocio; la lógica se prueba sin HTTP.
- Application referencia el paquete `Microsoft.EntityFrameworkCore` (por `DbSet<T>`). Es una concesión pragmática aceptada en lugar de abstraer IQueryable detrás de repositorios.
- Agregar una entidad nueva implica tocar las cuatro capas; aceptable para el tamaño del proyecto.
