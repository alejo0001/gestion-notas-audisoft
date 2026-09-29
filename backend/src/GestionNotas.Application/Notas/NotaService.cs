using System.Linq.Expressions;
using FluentValidation;
using GestionNotas.Application.Abstractions;
using GestionNotas.Application.Common;
using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionNotas.Application.Notas;

public sealed class NotaService(
    IApplicationDbContext db,
    IValidator<NotaSaveRequest> validator,
    ILogger<NotaService> logger) : INotaService
{
    /// <summary>
    /// Proyección reutilizable. Al ser un Expression (no un Func), EF Core la traduce a SQL
    /// con los JOIN necesarios y solo trae las columnas usadas.
    /// </summary>
    private static readonly Expression<Func<Nota, NotaDto>> ToDto = n => new NotaDto(
        n.Id,
        n.Nombre,
        n.IdEstudiante,
        n.Estudiante.Nombre,
        n.IdProfesor,
        n.Profesor.Nombre,
        n.Valor);

    public async Task<PagedResult<NotaDto>> GetPagedAsync(NotaQuery query, CancellationToken cancellationToken)
    {
        var notas = db.Notas.AsNoTracking();

        if (query.IdEstudiante is { } idEstudiante)
        {
            notas = notas.Where(n => n.IdEstudiante == idEstudiante);
        }

        if (query.IdProfesor is { } idProfesor)
        {
            notas = notas.Where(n => n.IdProfesor == idProfesor);
        }

        if (query.NormalizedSearch is { } search)
        {
            notas = notas.Where(n =>
                n.Nombre.Contains(search) ||
                n.Estudiante.Nombre.Contains(search) ||
                n.Profesor.Nombre.Contains(search));
        }

        notas = (query.SortBy?.ToLowerInvariant(), query.IsDescending) switch
        {
            ("nombre", false) => notas.OrderBy(n => n.Nombre).ThenBy(n => n.Id),
            ("nombre", true) => notas.OrderByDescending(n => n.Nombre).ThenBy(n => n.Id),
            ("valor", false) => notas.OrderBy(n => n.Valor).ThenBy(n => n.Id),
            ("valor", true) => notas.OrderByDescending(n => n.Valor).ThenBy(n => n.Id),
            ("estudiante", false) => notas.OrderBy(n => n.Estudiante.Nombre).ThenBy(n => n.Id),
            ("estudiante", true) => notas.OrderByDescending(n => n.Estudiante.Nombre).ThenBy(n => n.Id),
            ("profesor", false) => notas.OrderBy(n => n.Profesor.Nombre).ThenBy(n => n.Id),
            ("profesor", true) => notas.OrderByDescending(n => n.Profesor.Nombre).ThenBy(n => n.Id),
            (_, true) => notas.OrderByDescending(n => n.Id),
            _ => notas.OrderBy(n => n.Id)
        };

        return await notas.Select(ToDto).ToPagedResultAsync(query, cancellationToken);
    }

    public async Task<Result<NotaDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var nota = await db.Notas
            .AsNoTracking()
            .Where(n => n.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return nota is null ? NotFound(id) : Result<NotaDto>.Success(nota);
    }

    public async Task<Result<NotaDto>> CreateAsync(NotaSaveRequest request, CancellationToken cancellationToken)
    {
        var error = await ValidateAsync(request, idActual: null, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var nota = new Nota
        {
            Nombre = Texto.Normalizar(request.Nombre),
            IdEstudiante = request.IdEstudiante,
            IdProfesor = request.IdProfesor,
            Valor = request.Valor
        };

        db.Notas.Add(nota);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Nota {NotaId} creada para el estudiante {EstudianteId} por el profesor {ProfesorId}",
            nota.Id, nota.IdEstudiante, nota.IdProfesor);

        return await GetByIdAsync(nota.Id, cancellationToken);
    }

    public async Task<Result<NotaDto>> UpdateAsync(int id, NotaSaveRequest request, CancellationToken cancellationToken)
    {
        var nota = await db.Notas.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (nota is null)
        {
            return NotFound(id);
        }

        var error = await ValidateAsync(request, idActual: id, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        nota.Nombre = Texto.Normalizar(request.Nombre);
        nota.IdEstudiante = request.IdEstudiante;
        nota.IdProfesor = request.IdProfesor;
        nota.Valor = request.Valor;
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Nota {NotaId} actualizada", id);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var nota = await db.Notas.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (nota is null)
        {
            return Result.Failure(NotFound(id));
        }

        db.Notas.Remove(nota);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Nota {NotaId} eliminada", id);
        return Result.Success();
    }

    /// <summary>
    /// Valida en orden: formato (FluentValidation), que el estudiante y el profesor existan y que la
    /// evaluación no esté repetida. <paramref name="idActual"/> es la nota que se edita (null al crear),
    /// para no compararla consigo misma.
    /// </summary>
    private async Task<Error?> ValidateAsync(NotaSaveRequest request, int? idActual, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var errores = new Dictionary<string, string[]>();

        if (!await db.Estudiantes.AnyAsync(e => e.Id == request.IdEstudiante, cancellationToken))
        {
            errores["idEstudiante"] = [$"No existe un estudiante con id {request.IdEstudiante}."];
        }

        if (!await db.Profesores.AnyAsync(p => p.Id == request.IdProfesor, cancellationToken))
        {
            errores["idProfesor"] = [$"No existe un profesor con id {request.IdProfesor}."];
        }

        if (errores.Count > 0)
        {
            return Error.Validation(errores);
        }

        return await EvaluacionDuplicadaAsync(request, idActual, cancellationToken);
    }

    /// <summary>
    /// Llave natural de la nota: un estudiante no puede tener dos veces la misma evaluación con el mismo profesor
    /// (índice único IX_Nota_IdEstudiante_IdProfesor_Nombre). La misma evaluación con otro profesor sí se permite.
    /// En SQL Server la comparación no distingue mayúsculas/minúsculas (collation *_CI_AS).
    /// </summary>
    private async Task<Error?> EvaluacionDuplicadaAsync(NotaSaveRequest request, int? idActual, CancellationToken cancellationToken)
    {
        var nombre = Texto.Normalizar(request.Nombre);

        var duplicada = await db.Notas
            .Where(n => n.IdEstudiante == request.IdEstudiante
                        && n.IdProfesor == request.IdProfesor
                        && n.Nombre == nombre
                        && (idActual == null || n.Id != idActual))
            .Select(n => new { Estudiante = n.Estudiante.Nombre, Profesor = n.Profesor.Nombre })
            .FirstOrDefaultAsync(cancellationToken);

        if (duplicada is null)
        {
            return null;
        }

        logger.LogWarning(
            "Evaluación duplicada rechazada: {Nombre} (estudiante {EstudianteId}, profesor {ProfesorId})",
            nombre, request.IdEstudiante, request.IdProfesor);

        return Error.Validation(
            "nombre",
            $"{duplicada.Estudiante} ya tiene la evaluación «{nombre}» registrada con el profesor {duplicada.Profesor}.");
    }

    private Error NotFound(int id)
    {
        logger.LogWarning("Nota {NotaId} no encontrada", id);
        return Error.NotFound($"No existe una nota con id {id}.");
    }
}
