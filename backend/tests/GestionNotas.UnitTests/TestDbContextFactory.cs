using GestionNotas.Domain.Entities;
using GestionNotas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionNotas.UnitTests;

/// <summary>
/// Crea un AppDbContext en memoria, aislado por prueba (nombre de BD único).
/// Nota: InMemory no aplica FKs ni CHECKs; esas reglas se prueban aquí a nivel de servicio.
/// </summary>
internal static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"gestion-notas-{Guid.NewGuid()}")
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated(); // inserta los datos semilla (HasData)
        return db;
    }

    public static AppDbContext CreateEmpty()
    {
        var db = Create();
        db.Notas.RemoveRange(db.Notas);
        db.Estudiantes.RemoveRange(db.Estudiantes);
        db.Profesores.RemoveRange(db.Profesores);
        db.SaveChanges();
        return db;
    }

    public static Estudiante AddEstudiante(this AppDbContext db, string nombre)
    {
        var estudiante = new Estudiante { Nombre = nombre };
        db.Estudiantes.Add(estudiante);
        db.SaveChanges();
        return estudiante;
    }

    public static Profesor AddProfesor(this AppDbContext db, string nombre)
    {
        var profesor = new Profesor { Nombre = nombre };
        db.Profesores.Add(profesor);
        db.SaveChanges();
        return profesor;
    }
}
