# 0007 — Despliegue en Azure con GitHub Actions

**Estado:** Aceptada (2026-09-29)

## Contexto

Además del ZIP con código y scripts, se quiere una demo pública con un link para el evaluador. Requisitos: costo prácticamente cero (poco tráfico), aspecto profesional y que cada cambio en `main` se publique solo. La ejecución local (LocalDB, `dotnet run`, `npm start`) no debe cambiar ni requerir Docker.

## Decisión

| Pieza | Servicio | Por qué |
|---|---|---|
| Frontend | **Azure Static Web Apps (Free)** | HTTPS y CDN gratis; ideal para una SPA. |
| API | **Azure Container Apps (Consumption)** con imagen Docker en **ghcr.io** | Escala a cero (sin tráfico no cobra); cupo mensual gratuito; ghcr.io es gratis para repos públicos (Azure Container Registry cuesta ~US$5/mes). |
| Base de datos | **Azure SQL Database, oferta gratuita (serverless)** | Mismo motor que local; 100.000 vCore-segundos y 32 GB al mes gratis; configurada para **pausarse** (no cobrar) si se agota el cupo. |
| CI/CD | **GitHub Actions** (`.github/workflows/ci-cd.yml`) | Pruebas en cada PR; en `main`: pruebas → imagen → migraciones → API → frontend. |

- **Autenticación GitHub → Azure con OIDC** (federated credentials): no se guardan contraseñas de Azure en GitHub; el service principal solo tiene rol Contributor sobre el grupo de recursos.
- **Migraciones en el pipeline, no al arrancar el API** (`dotnet ef migrations bundle`): el runner abre temporalmente su IP en el firewall de Azure SQL, aplica las migraciones pendientes y cierra la regla. En producción el API nunca modifica el esquema.
- **Secretos fuera del código:** la cadena de conexión vive como *secret* del Container App y de GitHub; la URL del API se inyecta en el build de Angular desde una variable de GitHub (`__API_URL__` en `environment.ts`).
- **Swagger en producción** habilitado por configuración (`Swagger__Enabled=true`) solo para la demo.
- Docker se usa **únicamente** para construir la imagen en el pipeline; `backend/Dockerfile` es multi-etapa (SDK para compilar, runtime ASP.NET para ejecutar) y corre con usuario sin privilegios.

## Consecuencias

- Arranque en frío: tras un rato sin uso, la primera petición tarda (réplica del API en cero y base serverless en pausa). Antes de una demo conviene abrir la app unos minutos antes.
- El usuario SQL del pipeline y del API es el administrador del servidor: aceptable para la demo; en un entorno real se usarían usuarios distintos con mínimo privilegio (el del API sin permisos DDL) o identidad administrada.
- La demo es pública y sin autenticación: cualquiera puede crear o borrar datos. Posible mejora: restaurar datos de prueba periódicamente o agregar login (JWT).
- La primera imagen en ghcr.io nace privada y debe marcarse pública una vez (ver `docs/despliegue-azure.md`).
