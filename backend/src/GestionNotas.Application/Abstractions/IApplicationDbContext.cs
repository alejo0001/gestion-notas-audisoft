using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionNotas.Application.Abstractions;

/// <summary>
/// Abstracción del contexto de datos que usan los servicios. La implementación concreta
/// (SQL Server) vive en Infrastructure: Application depende de la abstracción, no del detalle.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Estudiante> Estudiantes { get; }

    DbSet<Profesor> Profesores { get; }

    DbSet<Nota> Notas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
