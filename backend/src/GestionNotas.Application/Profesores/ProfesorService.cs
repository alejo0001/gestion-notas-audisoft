using FluentValidation;
using GestionNotas.Application.Abstractions;
using GestionNotas.Application.Common;
using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionNotas.Application.Profesores;

public sealed class ProfesorService(
    IApplicationDbContext db,
    IValidator<ProfesorSaveRequest> validator,
    ILogger<ProfesorService> logger) : IProfesorService
{
    public async Task<PagedResult<ProfesorDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken)
    {
        var profesores = db.Profesores.AsNoTracking();

        if (query.NormalizedSearch is { } search)
        {
            profesores = profesores.Where(p => p.Nombre.Contains(search));
        }

        profesores = (query.SortBy?.ToLowerInvariant(), query.IsDescending) switch
        {
            ("nombre", false) => profesores.OrderBy(p => p.Nombre),
            ("nombre", true) => profesores.OrderByDescending(p => p.Nombre),
            ("cantidadnotas", false) => profesores.OrderBy(p => p.Notas.Count).ThenBy(p => p.Id),
            ("cantidadnotas", true) => profesores.OrderByDescending(p => p.Notas.Count).ThenBy(p => p.Id),
            (_, true) => profesores.OrderByDescending(p => p.Id),
            _ => profesores.OrderBy(p => p.Id)
        };

        return await profesores
            .Select(p => new ProfesorDto(p.Id, p.Nombre, p.Notas.Count))
            .ToPagedResultAsync(query, cancellationToken);
    }

    public async Task<Result<ProfesorDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var profesor = await db.Profesores
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProfesorDto(p.Id, p.Nombre, p.Notas.Count))
            .FirstOrDefaultAsync(cancellationToken);

        return profesor is null ? NotFound(id) : Result<ProfesorDto>.Success(profesor);
    }

    public async Task<IReadOnlyList<LookupDto>> GetLookupAsync(CancellationToken cancellationToken) =>
        await db.Profesores
            .AsNoTracking()
            .OrderBy(p => p.Nombre)
            .Select(p => new LookupDto(p.Id, p.Nombre))
            .ToListAsync(cancellationToken);

    public async Task<Result<ProfesorDto>> CreateAsync(ProfesorSaveRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var duplicado = await NombreDuplicadoAsync(request.Nombre, idActual: null, cancellationToken);
        if (duplicado is not null)
        {
            return duplicado;
        }

        var profesor = new Profesor { Nombre = request.Nombre.Trim() };
        db.Profesores.Add(profesor);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Profesor {ProfesorId} creado", profesor.Id);
        return new ProfesorDto(profesor.Id, profesor.Nombre, 0);
    }

    public async Task<Result<ProfesorDto>> UpdateAsync(int id, ProfesorSaveRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var profesor = await db.Profesores.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (profesor is null)
        {
            return NotFound(id);
        }

        var duplicado = await NombreDuplicadoAsync(request.Nombre, idActual: id, cancellationToken);
        if (duplicado is not null)
        {
            return duplicado;
        }

        profesor.Nombre = request.Nombre.Trim();
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Profesor {ProfesorId} actualizado", id);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var profesor = await db.Profesores.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (profesor is null)
        {
            return Result.Failure(NotFound(id));
        }

        var cantidadNotas = await db.Notas.CountAsync(n => n.IdProfesor == id, cancellationToken);
        if (cantidadNotas > 0)
        {
            logger.LogWarning(
                "Eliminación rechazada: el profesor {ProfesorId} tiene {CantidadNotas} notas",
                id, cantidadNotas);
            return Result.Failure(Error.Conflict(
                $"No se puede eliminar el profesor porque tiene {cantidadNotas} nota(s) registrada(s). Elimine primero sus notas."));
        }

        db.Profesores.Remove(profesor);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Profesor {ProfesorId} eliminado", id);
        return Result.Success();
    }

    /// <summary>
    /// Devuelve un error de validación si ya existe otro profesor con el mismo nombre.
    /// En SQL Server la comparación no distingue mayúsculas/minúsculas (collation *_CI_AS).
    /// </summary>
    private async Task<Error?> NombreDuplicadoAsync(string nombre, int? idActual, CancellationToken cancellationToken)
    {
        var normalizado = nombre.Trim();
        var existe = await db.Profesores.AnyAsync(
            p => p.Nombre == normalizado && (idActual == null || p.Id != idActual),
            cancellationToken);

        if (!existe)
        {
            return null;
        }

        logger.LogWarning("Nombre de profesor duplicado rechazado: {Nombre}", normalizado);
        return Error.Validation("nombre", $"Ya existe un profesor con el nombre «{normalizado}».");
    }

    private Error NotFound(int id)
    {
        logger.LogWarning("Profesor {ProfesorId} no encontrado", id);
        return Error.NotFound($"No existe un profesor con id {id}.");
    }
}
