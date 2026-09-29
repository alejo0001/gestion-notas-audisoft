# Spec: Panel de resumen (dashboard)

**Estado:** Implementada (2026-09-28) — pendiente de verificación

## Objetivo

Contenido adicional pedido por el evaluador ("que no sea tan básica"): una página de inicio con indicadores que aprovechan los datos del CRUD.

## Requisitos funcionales

1. Totales de estudiantes, profesores y notas.
2. Promedio general de todas las notas.
3. Cantidad de notas aprobadas (≥ 3.0) y reprobadas.
4. Top 5 de estudiantes por promedio.
5. Promedio de notas asignadas por cada profesor.

## Contrato de API

`GET /api/reportes/resumen` → `200`

```jsonc
{
  "totalEstudiantes": 12, "totalProfesores": 5, "totalNotas": 30,
  "promedioGeneral": 3.72, "notasAprobadas": 22, "notasReprobadas": 8,
  "mejoresEstudiantes": [ { "id": 3, "nombre": "Ana Pérez", "promedio": 4.6, "cantidadNotas": 3 } ],
  "promedioPorProfesor": [ { "id": 1, "nombre": "Carlos Rodríguez", "promedio": 3.9, "cantidadNotas": 7 } ]
}
```

## Criterios de aceptación

- Sin notas registradas, `promedioGeneral` es `null` y la UI muestra "Sin datos".
- Los números coinciden con los listados de cada módulo.

## Fuera de alcance

- Gráficas interactivas y exportación a PDF/Excel (mejoras futuras candidatas).
