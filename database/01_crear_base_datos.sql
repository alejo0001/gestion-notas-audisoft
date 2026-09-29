-- =============================================================
-- Gestión de Notas — Creación de la base de datos
-- Motor: SQL Server 2019+ / LocalDB / Azure SQL
--
-- Alternativa a las migraciones de EF Core (ver README y ADR 0002).
-- Es compatible con ellas: registra las migraciones en __EFMigrationsHistory.
-- =============================================================

IF DB_ID(N'GestionNotas') IS NULL
BEGIN
    CREATE DATABASE GestionNotas;
END
GO

USE GestionNotas;
GO

-- -------------------------------------------------------------
-- Tabla: Profesor
-- -------------------------------------------------------------
IF OBJECT_ID(N'dbo.Profesor', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Profesor
    (
        Id     INT IDENTITY(1,1) NOT NULL,
        Nombre NVARCHAR(100)     NOT NULL,
        CONSTRAINT PK_Profesor PRIMARY KEY CLUSTERED (Id)
    );

    -- Índice único: no se permiten dos registros con el mismo nombre.
    CREATE UNIQUE INDEX IX_Profesor_Nombre ON dbo.Profesor (Nombre);
END
GO

-- -------------------------------------------------------------
-- Tabla: Estudiante
-- -------------------------------------------------------------
IF OBJECT_ID(N'dbo.Estudiante', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Estudiante
    (
        Id     INT IDENTITY(1,1) NOT NULL,
        Nombre NVARCHAR(100)     NOT NULL,
        CONSTRAINT PK_Estudiante PRIMARY KEY CLUSTERED (Id)
    );

    -- Índice único: no se permiten dos registros con el mismo nombre.
    CREATE UNIQUE INDEX IX_Estudiante_Nombre ON dbo.Estudiante (Nombre);
END
GO

-- -------------------------------------------------------------
-- Tabla: Nota
-- Llaves foráneas hacia Profesor y Estudiante (requisito del enunciado).
-- ON DELETE NO ACTION: no se borran notas en cascada (ADR 0005).
-- -------------------------------------------------------------
IF OBJECT_ID(N'dbo.Nota', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Nota
    (
        Id           INT IDENTITY(1,1) NOT NULL,
        Nombre       NVARCHAR(100)     NOT NULL,
        IdProfesor   INT               NOT NULL,
        IdEstudiante INT               NOT NULL,
        Valor        DECIMAL(3,2)      NOT NULL,
        CONSTRAINT PK_Nota PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Nota_Profesor FOREIGN KEY (IdProfesor)
            REFERENCES dbo.Profesor (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Nota_Estudiante FOREIGN KEY (IdEstudiante)
            REFERENCES dbo.Estudiante (Id) ON DELETE NO ACTION,
        CONSTRAINT CK_Nota_Valor CHECK ([Valor] >= 0 AND [Valor] <= 5)
    );

    -- SQL Server no indexa las FK automáticamente: se crean para acelerar JOIN y filtros.
    CREATE INDEX IX_Nota_IdProfesor ON dbo.Nota (IdProfesor);
    CREATE INDEX IX_Nota_IdEstudiante ON dbo.Nota (IdEstudiante);

    -- Llave natural: un estudiante no puede tener dos veces la misma evaluación con el mismo profesor.
    CREATE UNIQUE INDEX IX_Nota_IdEstudiante_IdProfesor_Nombre ON dbo.Nota (IdEstudiante, IdProfesor, Nombre);
END
GO

-- -------------------------------------------------------------
-- Registro de las migraciones de EF Core equivalentes a este script.
-- Así, si después se ejecuta el API (que aplica migraciones al iniciar
-- en Development), EF Core reconoce que el esquema ya existe y no
-- intenta crear las tablas de nuevo. Scripts y migraciones quedan compatibles.
-- -------------------------------------------------------------
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__EFMigrationsHistory
    (
        MigrationId    NVARCHAR(150) NOT NULL,
        ProductVersion NVARCHAR(32)  NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260928214508_InitialCreate')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260928214508_InitialCreate', N'10.0.0');
END
GO

-- El índice único de nombres (migración NombreUnico) ya está incluido arriba en CREATE UNIQUE INDEX.
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260928234406_NombreUnico')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260928234406_NombreUnico', N'10.0.0');
END
GO

-- El índice único de evaluaciones (migración NotaUnica) ya está incluido arriba en la tabla Nota.
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20260929193000_NotaUnica')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260929193000_NotaUnica', N'10.0.0');
END
GO

PRINT N'Base de datos GestionNotas creada correctamente.';
GO
