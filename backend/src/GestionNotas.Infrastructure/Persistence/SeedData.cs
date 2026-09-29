using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionNotas.Infrastructure.Persistence;

/// <summary>
/// Datos de prueba insertados por la migración inicial (HasData).
/// Deben coincidir con database/02_datos_prueba.sql.
/// </summary>
internal static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profesor>().HasData(
            new Profesor { Id = 1, Nombre = "Carlos Rodríguez" },
            new Profesor { Id = 2, Nombre = "María Fernanda López" },
            new Profesor { Id = 3, Nombre = "Jorge Iván Martínez" },
            new Profesor { Id = 4, Nombre = "Ana Lucía Herrera" },
            new Profesor { Id = 5, Nombre = "Andrés Felipe Castaño" });

        modelBuilder.Entity<Estudiante>().HasData(
            new Estudiante { Id = 1, Nombre = "Sofía Ramírez" },
            new Estudiante { Id = 2, Nombre = "Santiago Gómez" },
            new Estudiante { Id = 3, Nombre = "Valentina Muñoz" },
            new Estudiante { Id = 4, Nombre = "Mateo Rojas" },
            new Estudiante { Id = 5, Nombre = "Isabella Díaz" },
            new Estudiante { Id = 6, Nombre = "Samuel Torres" },
            new Estudiante { Id = 7, Nombre = "Mariana Vargas" },
            new Estudiante { Id = 8, Nombre = "Sebastián Peña" },
            new Estudiante { Id = 9, Nombre = "Camila Ortiz" },
            new Estudiante { Id = 10, Nombre = "Nicolás Jiménez" },
            new Estudiante { Id = 11, Nombre = "Daniela Ríos" },
            new Estudiante { Id = 12, Nombre = "Tomás Álvarez" });

        modelBuilder.Entity<Nota>().HasData(
            new Nota { Id = 1, Nombre = "Parcial 2", IdProfesor = 2, IdEstudiante = 1, Valor = 3.5m },
            new Nota { Id = 2, Nombre = "Taller de álgebra", IdProfesor = 3, IdEstudiante = 1, Valor = 2.9m },
            new Nota { Id = 3, Nombre = "Quiz de programación", IdProfesor = 4, IdEstudiante = 1, Valor = 3.8m },
            new Nota { Id = 4, Nombre = "Taller de álgebra", IdProfesor = 3, IdEstudiante = 2, Valor = 4.7m },
            new Nota { Id = 5, Nombre = "Quiz de programación", IdProfesor = 4, IdEstudiante = 2, Valor = 1.8m },
            new Nota { Id = 6, Nombre = "Examen final", IdProfesor = 5, IdEstudiante = 2, Valor = 2.4m },
            new Nota { Id = 7, Nombre = "Quiz de programación", IdProfesor = 4, IdEstudiante = 3, Valor = 4.2m },
            new Nota { Id = 8, Nombre = "Examen final", IdProfesor = 5, IdEstudiante = 3, Valor = 2.4m },
            new Nota { Id = 9, Nombre = "Exposición", IdProfesor = 1, IdEstudiante = 3, Valor = 3.5m },
            new Nota { Id = 10, Nombre = "Examen final", IdProfesor = 5, IdEstudiante = 4, Valor = 4.5m },
            new Nota { Id = 11, Nombre = "Exposición", IdProfesor = 1, IdEstudiante = 4, Valor = 1.8m },
            new Nota { Id = 12, Nombre = "Proyecto integrador", IdProfesor = 2, IdEstudiante = 4, Valor = 4.2m },
            new Nota { Id = 13, Nombre = "Exposición", IdProfesor = 1, IdEstudiante = 5, Valor = 3.0m },
            new Nota { Id = 14, Nombre = "Proyecto integrador", IdProfesor = 2, IdEstudiante = 5, Valor = 1.8m },
            new Nota { Id = 15, Nombre = "Parcial 1", IdProfesor = 3, IdEstudiante = 5, Valor = 2.4m },
            new Nota { Id = 16, Nombre = "Proyecto integrador", IdProfesor = 2, IdEstudiante = 6, Valor = 3.8m },
            new Nota { Id = 17, Nombre = "Parcial 1", IdProfesor = 3, IdEstudiante = 6, Valor = 3.8m },
            new Nota { Id = 18, Nombre = "Parcial 2", IdProfesor = 4, IdEstudiante = 6, Valor = 2.4m },
            new Nota { Id = 19, Nombre = "Parcial 1", IdProfesor = 3, IdEstudiante = 7, Valor = 3.0m },
            new Nota { Id = 20, Nombre = "Parcial 2", IdProfesor = 4, IdEstudiante = 7, Valor = 2.4m },
            new Nota { Id = 21, Nombre = "Parcial 2", IdProfesor = 4, IdEstudiante = 8, Valor = 4.2m },
            new Nota { Id = 22, Nombre = "Taller de álgebra", IdProfesor = 5, IdEstudiante = 8, Valor = 3.8m },
            new Nota { Id = 23, Nombre = "Taller de álgebra", IdProfesor = 5, IdEstudiante = 9, Valor = 1.8m },
            new Nota { Id = 24, Nombre = "Quiz de programación", IdProfesor = 1, IdEstudiante = 9, Valor = 4.5m },
            new Nota { Id = 25, Nombre = "Quiz de programación", IdProfesor = 1, IdEstudiante = 10, Valor = 2.4m },
            new Nota { Id = 26, Nombre = "Examen final", IdProfesor = 2, IdEstudiante = 10, Valor = 3.0m },
            new Nota { Id = 27, Nombre = "Examen final", IdProfesor = 2, IdEstudiante = 11, Valor = 4.7m },
            new Nota { Id = 28, Nombre = "Exposición", IdProfesor = 3, IdEstudiante = 11, Valor = 4.7m });
    }
}
