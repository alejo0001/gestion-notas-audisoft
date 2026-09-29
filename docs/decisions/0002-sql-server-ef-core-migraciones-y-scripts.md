# 0002 — SQL Server con EF Core: migraciones y scripts SQL

**Estado:** Aceptada (2026-09-28)

## Contexto

El enunciado permite cualquier base de datos y crearla con migraciones de EF o con un script SQL, y pide entregar "scripts SQL" dentro del ZIP. SQL Server es el motor más habitual en empresas .NET en Colombia.

## Decisión

- Motor: **SQL Server** (LocalDB por defecto; también Express o Developer). En la nube, Azure SQL Database (misma sintaxis y mismas migraciones).
- Acceso a datos: **EF Core 10** con Fluent API (`IEntityTypeConfiguration<T>`), nombres de tablas y columnas exactamente como el enunciado (`Estudiante`, `Nota`, `Profesor`, `IdProfesor`, `IdEstudiante`...).
- Dos caminos equivalentes para crear la base:
  - **A (recomendado):** migraciones EF Core (`dotnet ef database update`), con datos semilla vía `HasData`.
  - **B:** `database/01_crear_base_datos.sql` + `database/02_datos_prueba.sql`.
- Restricciones explícitas y con nombre: `PK_*`, `FK_Nota_Estudiante`, `FK_Nota_Profesor`, `CK_Nota_Valor` (0 a 5) e índices sobre las FK y los nombres.
- `Valor` es `DECIMAL(3,2)`: nunca `float` para calificaciones (evita errores de redondeo binario).

## Consecuencias

- Cualquier cambio de esquema debe reflejarse en la configuración EF **y** en el script SQL para que ambos caminos sigan siendo equivalentes.
- La migración inicial se genera en la máquina del desarrollador (`dotnet ef migrations add InitialCreate`) y luego se versiona.
- `EnableRetryOnFailure` activo en el proveedor SQL Server para errores transitorios.
