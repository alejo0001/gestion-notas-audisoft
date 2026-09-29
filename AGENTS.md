# AGENTS.md

Punto de entrada para cualquier agente de IA (o persona) que trabaje en este repositorio.

## Qué es el proyecto

**Gestión de Notas** — prueba técnica .NET para AudiSoft Consulting SAS. Aplicación web para administrar estudiantes, profesores y las notas que los profesores asignan a los estudiantes: API REST + base de datos SQL Server + frontend Angular, con paginación, alertas de cada acción y un panel de resumen.

- Backend: .NET 10, ASP.NET Core Web API (controladores), EF Core 10, FluentValidation, OpenAPI + Swagger UI.
- Base de datos: SQL Server (migraciones EF Core o scripts en `database/`).
- Frontend: Angular 22 standalone, Signals (zoneless), Angular Material 3, SweetAlert2.
- Pruebas: xUnit + EF Core InMemory.

## Cómo ejecutarlo

Ver [README.md](README.md).

## Mapa de documentación (orden de lectura)

1. `AGENTS.md` (este archivo)
2. `README.md` — instalación y ejecución
3. `docs/decisions/` — ADRs, decisiones de arquitectura ya tomadas
4. `docs/specs/<feature>.md` — spec de la funcionalidad en la que se va a trabajar
5. `CHANGELOG.md` — historial técnico

Regla: toda funcionalidad no trivial nueva empieza con una spec en `docs/specs/`. Toda decisión de arquitectura tomada se registra como ADR numerado (nunca se renumeran).

## Estructura

```
backend/
  src/GestionNotas.Domain          entidades
  src/GestionNotas.Application     DTOs, servicios, validadores, Result, paginación
  src/GestionNotas.Infrastructure  EF Core, configuraciones, semilla, migraciones
  src/GestionNotas.Api             controladores, manejo de errores, Program.cs
  tests/GestionNotas.UnitTests     pruebas de servicios
frontend/                          Angular
database/                          scripts SQL
docs/                              ADRs y specs
```

## Convenciones de código

- Código en español para el dominio (`Estudiante`, `Nota`, `IdProfesor`) tal como lo define el enunciado; sufijos técnicos en inglés (`Service`, `Controller`, `Dto`, `Request`).
- Conversación y documentación en español, con ortografía y tildes correctas (lo evalúa AudiSoft). Todo texto visible en la UI debe revisarse.
- C#: `Nullable` habilitado, constructores primarios para inyección, `record` para DTOs, `sealed` por defecto en clases que no se heredan.
- Controladores delgados: solo traducen `Result` a HTTP. La lógica vive en `Application`.
- Rutas REST en plural: `/api/estudiantes`, `/api/profesores`, `/api/notas`.
- Angular: componentes standalone, `inject()`, Signals para estado, `OnPush`/zoneless, servicios en `core/`, pantallas en `features/`.

## Resiliencia y manejo de errores

Aplica a todo método, handler o servicio nuevo:

- Nunca tragar una excepción en silencio: registrar el objeto de excepción completo con `ILogger` (`logger.LogError(ex, ...)`), nunca `Console.WriteLine`.
- Distinguir resultados esperados de fallas excepcionales. No encontrado, validación y conflicto → `Result.Failure(...)` explícito (ADR 0004). Infraestructura caída, timeouts → se propagan y los atrapa `GlobalExceptionHandler`.
- Cada log de una operación incluye el identificador involucrado (`{EstudianteId}`, `{NotaId}`...) como propiedad estructurada, para poder rastrear un caso puntual.
- Propagar `CancellationToken` en toda operación async de E/S (controlador → servicio → EF Core).
- En el frontend, los errores HTTP se manejan en un solo lugar (`httpErrorInterceptor`); los componentes no repiten `catchError` salvo para lógica propia.

Pendiente (decisión diferida, se escribirá su ADR cuando se implemente): logging estructurado a archivo/servicio externo (Serilog), autenticación JWT, rate limiting.

## Estilo de trabajo pedido durante la implementación

Alejandro usa este proyecto como entregable real **y** como preparación para entrevistas técnicas. Mientras se implementa código, señalar de forma proactiva los momentos de aprendizaje **atados a la decisión concreta del código que se acaba de escribir** (por qué esta interfaz existe — inversión de dependencias; `record` vs `class`; `decimal` vs `double`; `Scoped` vs `Singleton`; por qué paginación offset; etc.). Un párrafo breve ligado a la línea real, nunca una clase teórica desconectada.

Si el entorno no puede compilar o ejecutar el proyecto, decirlo claramente, revisar a mano contra los patrones existentes y pedir que se compile y pruebe localmente antes de dar algo por funcionando.
