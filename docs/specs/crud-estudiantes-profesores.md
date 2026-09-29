# Spec: CRUD de Estudiantes y Profesores

**Estado:** Implementada (2026-09-28) — pendiente de verificación

## Objetivo

Permitir crear, consultar, editar y eliminar estudiantes y profesores desde una API REST y una interfaz web, con paginación, búsqueda y alertas de confirmación de cada acción.

## Requisitos funcionales

1. Listar estudiantes/profesores de forma paginada (servidor), con búsqueda por nombre y ordenamiento.
2. Consultar un registro por su `id`.
3. Crear un registro con `nombre` obligatorio (2 a 100 caracteres, sin espacios sobrantes).
4. Editar el `nombre` de un registro existente.
5. Eliminar un registro. Si el registro tiene notas asociadas, la eliminación se rechaza con un mensaje claro (ver ADR 0005).
6. Cada listado muestra, además del nombre, la cantidad de notas asociadas; en estudiantes también el promedio.
7. Exponer un endpoint `lookup` (id + nombre, sin paginar) para llenar los selectores del formulario de notas.
8. El nombre es único por recurso: no se permiten dos estudiantes (ni dos profesores) con el mismo nombre. Se valida en el servicio (mensaje en el campo `nombre`) y con un índice único en la base de datos. Antes de comparar y guardar, el nombre se normaliza: sin espacios en los extremos y con un solo espacio entre palabras (`Texto.Normalizar`), así «Samuel  Torres» es duplicado de «Samuel Torres». La comparación no distingue mayúsculas/minúsculas (collation de SQL Server); sí distingue tildes («Sebastián» ≠ «Sebastian»).
9. La interfaz muestra una alerta tras cada acción: "Estudiante creado correctamente", "Estudiante actualizado correctamente", "Estudiante eliminado correctamente" (ídem para profesores), y pide confirmación antes de eliminar.

## Contrato de API

Base: `/api/estudiantes` y `/api/profesores` (mismo contrato).

| Método | Ruta | Respuesta |
|---|---|---|
| GET | `/?page=1&pageSize=10&search=ana&sortBy=nombre&sortDirection=asc` | `200 PagedResult<EstudianteDto>` |
| GET | `/{id}` | `200 EstudianteDto` · `404 ProblemDetails` |
| GET | `/lookup` | `200 LookupDto[]` |
| POST | `/` body `{ "nombre": "Ana Pérez" }` | `201 EstudianteDto` + header `Location` · `400 ValidationProblemDetails` |
| PUT | `/{id}` body `{ "nombre": "..." }` | `200 EstudianteDto` · `400` · `404` |
| DELETE | `/{id}` | `204` · `404` · `409 ProblemDetails` (tiene notas) |

```jsonc
// PagedResult<T>
{ "items": [], "page": 1, "pageSize": 10, "totalCount": 42, "totalPages": 5 }
// EstudianteDto
{ "id": 1, "nombre": "Ana Pérez", "cantidadNotas": 3, "promedio": 4.15 }
// ProfesorDto
{ "id": 1, "nombre": "Carlos Rodríguez", "cantidadNotas": 7 }
```

Valores permitidos de `sortBy`: `id`, `nombre`, `cantidadNotas` y, solo en estudiantes, `promedio`. `pageSize` se limita a 1–100.

## Criterios de aceptación

- `POST` con `nombre` vacío devuelve 400 con el error en el campo `nombre`, y la UI lo muestra.
- `POST` o `PUT` con un nombre que ya usa otro registro devuelve 400 con el error «Ya existe un estudiante con el nombre…» en el campo `nombre`; el diálogo sigue abierto.
- `GET ?page=2&pageSize=5` con 12 registros devuelve 5 ítems, `totalCount = 12`, `totalPages = 3`.
- `DELETE` de un estudiante con notas devuelve 409 y la base de datos no cambia.
- La UI muestra un toast de éxito después de crear, editar y eliminar.
- Todos los textos visibles tienen ortografía y tildes correctas.

## Fuera de alcance

- Autenticación/autorización (posible mejora futura).
- Borrado lógico (soft delete).
- Campos adicionales en las tablas (el esquema del enunciado se respeta tal cual).
