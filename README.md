# Gestión de Notas — Prueba técnica .NET (AudiSoft)

Aplicación web para administrar **estudiantes**, **profesores** y las **notas** que los profesores asignan a los estudiantes.

- **API REST** en .NET 10 (ASP.NET Core + EF Core 10 + SQL Server).
- **Frontend** en Angular 20 + Angular Material.
- Paginación, búsqueda y ordenamiento en el servidor, alertas de cada acción, validaciones, manejo global de errores, panel de indicadores, documentación interactiva del API con Swagger y pruebas unitarias.

## Contenido del repositorio

| Carpeta | Descripción |
|---|---|
| `backend/` | Solución .NET (`GestionNotas.slnx`): Domain, Application, Infrastructure, Api y UnitTests |
| `frontend/` | Aplicación Angular |
| `database/` | Scripts SQL: creación de la base con restricciones y datos de prueba |
| `docs/decisions/` | Decisiones de arquitectura (ADRs) |
| `docs/specs/` | Especificaciones funcionales |

## Requisitos previos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 22 LTS](https://nodejs.org/) (incluye npm)
- SQL Server: cualquiera de estas opciones
  - **LocalDB** (se instala con Visual Studio) — configuración por defecto
  - SQL Server Express / Developer
  - Docker (`docker compose up -d` levanta SQL Server 2022)

## Instalación y ejecución

### 1. Base de datos

Elija **una** de las dos opciones.

**Opción A — Migraciones de EF Core (recomendada)**

```bash
cd backend
dotnet tool restore
dotnet ef migrations add InitialCreate -p src/GestionNotas.Infrastructure -s src/GestionNotas.Api -o Persistence/Migrations
dotnet ef database update -p src/GestionNotas.Infrastructure -s src/GestionNotas.Api
```

> La migración inicial solo se genera una vez; luego se versiona en el repositorio. En modo Development la API también aplica las migraciones pendientes al iniciar.

**Opción B — Scripts SQL**

Ejecute en SQL Server Management Studio, Azure Data Studio o `sqlcmd`, en este orden:

1. `database/01_crear_base_datos.sql`
2. `database/02_datos_prueba.sql`

El script registra la migración inicial en `__EFMigrationsHistory`, así que después se puede ejecutar el API sin conflictos: EF Core reconoce que el esquema ya existe.

**Cadena de conexión**

Si su SQL Server no es LocalDB, solo cambie `ConnectionStrings:GestionNotas` en `backend/src/GestionNotas.Api/appsettings.json`.

Por defecto (`backend/src/GestionNotas.Api/appsettings.json`):

```
Server=(localdb)\MSSQLLocalDB;Database=GestionNotas;Trusted_Connection=True;TrustServerCertificate=True
```

Otras opciones:

- SQL Server Express: `Server=.\SQLEXPRESS;Database=GestionNotas;Trusted_Connection=True;TrustServerCertificate=True`
- Docker: `Server=localhost,1433;Database=GestionNotas;User Id=sa;Password=GestionNotas#2026;TrustServerCertificate=True`

### 2. API

```bash
cd backend
dotnet run --project src/GestionNotas.Api --launch-profile http
```

- API: http://localhost:5080/api
- Documentación interactiva (Swagger UI): http://localhost:5080/swagger
- Especificación OpenAPI: http://localhost:5080/openapi/v1.json
- Health check: http://localhost:5080/health
- Colección de pruebas: `backend/src/GestionNotas.Api/GestionNotas.Api.http` (también importable en Postman desde el OpenAPI).

### 3. Frontend

```bash
cd frontend
npm install
npm start
```

Abre http://localhost:4300 (puerto fijo para no chocar con otros proyectos Angular en el 4200). La URL del API se configura en `frontend/src/environments/environment.development.ts`.

### 4. Pruebas unitarias

```bash
cd backend
dotnet test
```

## Endpoints

| Recurso | Endpoints |
|---|---|
| Estudiantes | `GET /api/estudiantes` · `GET /api/estudiantes/{id}` · `GET /api/estudiantes/lookup` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| Profesores | `GET /api/profesores` · `GET /api/profesores/{id}` · `GET /api/profesores/lookup` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| Notas | `GET /api/notas` (filtros `idEstudiante`, `idProfesor`) · `GET /{id}` · `POST` · `PUT /{id}` · `DELETE /{id}` |
| Reportes | `GET /api/reportes/resumen` |

Parámetros de listado: `page`, `pageSize` (máx. 100), `search`, `sortBy`, `sortDirection` (`asc`/`desc`).

## Modelo de datos

```
Profesor (Id, Nombre) 1 ──< Nota (Id, Nombre, IdProfesor, IdEstudiante, Valor) >── 1 Estudiante (Id, Nombre)
```

- `FK_Nota_Profesor` y `FK_Nota_Estudiante` con `ON DELETE NO ACTION`.
- `CK_Nota_Valor`: el valor debe estar entre 0 y 5 (`DECIMAL(3,2)`).

## Funcionalidades adicionales

- Panel de inicio con totales, promedio general, porcentaje de aprobación, top 5 de estudiantes y promedio por profesor.
- Búsqueda con *debounce*, ordenamiento por columnas y filtros por estudiante/profesor en Notas.
- Enlace directo desde un estudiante o profesor a sus notas.
- Validaciones en frontend y backend con mensajes en español.
- Protección de integridad: no se puede eliminar un estudiante o profesor con notas (respuesta 409 con mensaje claro).
- Manejo global de errores con ProblemDetails (RFC 9457) y `traceId` para rastreo en logs.
- Diseño responsivo (menú lateral colapsable en móviles).

## Capturas

_Pendiente: se agregarán en el documento de instalación una vez verificada la aplicación._

## Documentación técnica

Ver [AGENTS.md](AGENTS.md), [docs/decisions](docs/decisions) y [CHANGELOG.md](CHANGELOG.md).
