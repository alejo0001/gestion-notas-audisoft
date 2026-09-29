-- =============================================================
-- Gestión de Notas — Datos de prueba
-- Ejecutar después de 01_crear_base_datos.sql.
-- Coinciden con los datos semilla de EF Core (SeedData.cs).
-- =============================================================

USE GestionNotas;
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;

SET IDENTITY_INSERT dbo.Profesor ON;
INSERT INTO dbo.Profesor (Id, Nombre) VALUES
    (1, N'Carlos Rodríguez'),
    (2, N'María Fernanda López'),
    (3, N'Jorge Iván Martínez'),
    (4, N'Ana Lucía Herrera'),
    (5, N'Andrés Felipe Castaño');
SET IDENTITY_INSERT dbo.Profesor OFF;

SET IDENTITY_INSERT dbo.Estudiante ON;
INSERT INTO dbo.Estudiante (Id, Nombre) VALUES
    (1, N'Sofía Ramírez'),
    (2, N'Santiago Gómez'),
    (3, N'Valentina Muñoz'),
    (4, N'Mateo Rojas'),
    (5, N'Isabella Díaz'),
    (6, N'Samuel Torres'),
    (7, N'Mariana Vargas'),
    (8, N'Sebastián Peña'),
    (9, N'Camila Ortiz'),
    (10, N'Nicolás Jiménez'),
    (11, N'Daniela Ríos'),
    (12, N'Tomás Álvarez');
SET IDENTITY_INSERT dbo.Estudiante OFF;

SET IDENTITY_INSERT dbo.Nota ON;
INSERT INTO dbo.Nota (Id, Nombre, IdProfesor, IdEstudiante, Valor) VALUES
    (1, N'Parcial 2', 2, 1, 3.50),
    (2, N'Taller de álgebra', 3, 1, 2.90),
    (3, N'Quiz de programación', 4, 1, 3.80),
    (4, N'Taller de álgebra', 3, 2, 4.70),
    (5, N'Quiz de programación', 4, 2, 1.80),
    (6, N'Examen final', 5, 2, 2.40),
    (7, N'Quiz de programación', 4, 3, 4.20),
    (8, N'Examen final', 5, 3, 2.40),
    (9, N'Exposición', 1, 3, 3.50),
    (10, N'Examen final', 5, 4, 4.50),
    (11, N'Exposición', 1, 4, 1.80),
    (12, N'Proyecto integrador', 2, 4, 4.20),
    (13, N'Exposición', 1, 5, 3.00),
    (14, N'Proyecto integrador', 2, 5, 1.80),
    (15, N'Parcial 1', 3, 5, 2.40),
    (16, N'Proyecto integrador', 2, 6, 3.80),
    (17, N'Parcial 1', 3, 6, 3.80),
    (18, N'Parcial 2', 4, 6, 2.40),
    (19, N'Parcial 1', 3, 7, 3.00),
    (20, N'Parcial 2', 4, 7, 2.40),
    (21, N'Parcial 2', 4, 8, 4.20),
    (22, N'Taller de álgebra', 5, 8, 3.80),
    (23, N'Taller de álgebra', 5, 9, 1.80),
    (24, N'Quiz de programación', 1, 9, 4.50),
    (25, N'Quiz de programación', 1, 10, 2.40),
    (26, N'Examen final', 2, 10, 3.00),
    (27, N'Examen final', 2, 11, 4.70),
    (28, N'Exposición', 3, 11, 4.70);
SET IDENTITY_INSERT dbo.Nota OFF;

COMMIT TRANSACTION;
GO

PRINT N'Datos de prueba insertados correctamente.';
GO
