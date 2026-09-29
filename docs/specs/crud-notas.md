# Spec: CRUD de Notas

**Estado:** Implementada (2026-09-28) — pendiente de verificación

## Objetivo

Registrar las notas que un profesor asigna a un estudiante, con validación de rango, llaves foráneas reales en la base de datos y una vista paginada y filtrable.

## Requisitos funcionales

1. Listar notas paginadas mostrando nombre de la nota, estudiante, profesor y valor.
2. Filtrar por estudiante y/o por profesor, y buscar por texto (nombre de la nota, del estudiante o del profesor).
3. Crear y editar una nota con: `nombre` (obligatorio, 2–100 caracteres), `idEstudiante` e `idProfesor` (deben existir), `valor` (0.0 a 5.0, máximo 2 decimales — escala colombiana).
4. Eliminar una nota con confirmación previa.
5. La UI indica visualmente si la nota es aprobatoria (≥ 3.0) o no.
6. Alerta tras cada acción: "Nota creada / actualizada / eliminada correctamente".

## Contrato de API

Base: `/api/notas`

| Método | Ruta | Respuesta |
|---|---|---|
| GET | `/?page=1&pageSize=10&search=&idEstudiante=&idProfesor=&sortBy=valor&sortDirection=desc` | `200 PagedResult<NotaDto>` |
| GET | `/{id}` | `200 NotaDto` · `404` |
| POST | `/` | `201 NotaDto` · `400` |
| PUT | `/{id}` | `200 NotaDto` · `400` · `404` |
| DELETE | `/{id}` | `204` · `404` |

```jsonc
// NotaSaveRequest
{ "nombre": "Parcial 1", "idEstudiante": 3, "idProfesor": 1, "valor": 4.5 }
// NotaDto
{ "id": 10, "nombre": "Parcial 1", "idEstudiante": 3, "estudianteNombre": "Ana Pérez",
  "idProfesor": 1, "profesorNombre": "Carlos Rodríguez", "valor": 4.5 }
```

`sortBy`: `id`, `nombre`, `valor`, `estudiante`, `profesor`.

## Base de datos

- `Nota.IdEstudiante` → `FK_Nota_Estudiante` (ON DELETE NO ACTION).
- `Nota.IdProfesor` → `FK_Nota_Profesor` (ON DELETE NO ACTION).
- `CK_Nota_Valor`: `Valor BETWEEN 0 AND 5`, tipo `DECIMAL(3,2)`.

## Criterios de aceptación

- Crear una nota con `idEstudiante` inexistente devuelve 400 con el error en `idEstudiante`.
- Crear una nota con `valor = 5.5` devuelve 400; con `valor = 5` devuelve 201.
- Insertar directamente en SQL una nota con valor 6 falla por el `CHECK`.
- Filtrar por estudiante devuelve solo sus notas y la paginación refleja el total filtrado.

## Fuera de alcance

- Pesos/porcentajes por nota, periodos académicos o asignaturas.
