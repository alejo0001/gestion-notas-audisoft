using GestionNotas.Application.Abstractions;
using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionNotas.Application.Reportes;

public sealed record PromedioItemDto(int Id, string Nombre, decimal Promedio, int CantidadNotas);

public sealed record ResumenDto(
    int TotalEstudiantes,
    int TotalProfesores,
    int TotalNotas,
    decimal? PromedioGeneral,
    int NotasAprobadas,
    int NotasReprobadas,
    IReadOnlyList<PromedioItemDto> MejoresEstudiantes,
    IReadOnlyList<PromedioItemDto> PromedioPorProfesor);

public interface IReporteService
{
    Task<ResumenDto> GetResumenAsync(CancellationToken cancellationToken);
}

public sealed class ReporteService(IApplicationDbContext db) : IReporteService
{
    private const int TopEstudiantes = 5;

    public async Task<ResumenDto> GetResumenAsync(CancellationToken cancellationToken)
    {
        // Consultas secuenciales: un DbContext no admite operaciones concurrentes (no usar Task.WhenAll aquí).
        var totalEstudiantes = await db.Estudiantes.CountAsync(cancellationToken);
        var totalProfesores = await db.Profesores.CountAsync(cancellationToken);
        var totalNotas = await db.Notas.CountAsync(cancellationToken);
        var promedioGeneral = await db.Notas.AverageAsync(n => (decimal?)n.Valor, cancellationToken);
        var aprobadas = await db.Notas.CountAsync(n => n.Valor >= Nota.NotaMinimaAprobatoria, cancellationToken);

        var mejoresEstudiantes = await db.Estudiantes
            .AsNoTracking()
            .Where(e => e.Notas.Any())
            // Se ordena ANTES del Select: EF Core no puede traducir un OrderBy sobre
            // propiedades de un record creado con constructor posicional.
            .OrderByDescending(e => e.Notas.Average(n => n.Valor))
            .ThenBy(e => e.Nombre)
            .Take(TopEstudiantes)
            .Select(e => new PromedioItemDto(
                e.Id,
                e.Nombre,
                e.Notas.Average(n => n.Valor),
                e.Notas.Count))
            .ToListAsync(cancellationToken);

        var promedioPorProfesor = await db.Profesores
            .AsNoTracking()
            .Where(p => p.Notas.Any())
            .OrderByDescending(p => p.Notas.Average(n => n.Valor))
            .ThenBy(p => p.Nombre)
            .Select(p => new PromedioItemDto(
                p.Id,
                p.Nombre,
                p.Notas.Average(n => n.Valor),
                p.Notas.Count))
            .ToListAsync(cancellationToken);

        return new ResumenDto(
            totalEstudiantes,
            totalProfesores,
            totalNotas,
            promedioGeneral is null ? null : Math.Round(promedioGeneral.Value, 2),
            aprobadas,
            totalNotas - aprobadas,
            mejoresEstudiantes,
            promedioPorProfesor);
    }
}
