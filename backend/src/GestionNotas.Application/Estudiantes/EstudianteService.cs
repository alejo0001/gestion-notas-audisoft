using FluentValidation;
using GestionNotas.Application.Abstractions;
using GestionNotas.Application.Common;
using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestionNotas.Application.Estudiantes;

public sealed class EstudianteService(
    IApplicationDbContext db,
    IValidator<EstudianteSaveRequest> validator,
    ILogger<EstudianteService> logger) : IEstudianteService
{
    public Task<PagedResult<EstudianteDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken) =>
        Consultar(query).ToPagedResultAsync(query, cancellationToken);

    /// <summary>Todos los registros que cumplen los filtros (sin paginar), para exportar a Excel.</summary>
    public async Task<IReadOnlyList<EstudianteDto>> ListarAsync(PagedQuery query, CancellationToken cancellationToken) =>
        await Consultar(query).Take(PagedQuery.MaxFilasExportacion).ToListAsync(cancellationToken);

    /// <summary>
    /// Filtros, búsqueda y orden compartidos por el listado paginado y la exportación: así el Excel contiene
    /// exactamente lo que el usuario ve en la tabla. Devuelve un IQueryable: nada se ejecuta hasta que el
    /// llamador pagina (Skip/Take) o materializa (ToListAsync), y todo se traduce a un único SQL.
    /// </summary>
    private IQueryable<EstudianteDto> Consultar(PagedQuery query)
    {
        var estudiantes = db.Estudiantes.AsNoTracking();

        if (query.NormalizedSearch is { } search)
        {
            estudiantes = estudiantes.Where(e => e.Nombre.Contains(search));
        }

        // Lista blanca de columnas ordenables: nunca se arma SQL a partir del texto recibido.
        estudiantes = (query.SortBy?.ToLowerInvariant(), query.IsDescending) switch
        {
            ("nombre", false) => estudiantes.OrderBy(e => e.Nombre),
            ("nombre", true) => estudiantes.OrderByDescending(e => e.Nombre),
            ("cantidadnotas", false) => estudiantes.OrderBy(e => e.Notas.Count).ThenBy(e => e.Id),
            ("cantidadnotas", true) => estudiantes.OrderByDescending(e => e.Notas.Count).ThenBy(e => e.Id),
            ("promedio", false) => estudiantes.OrderBy(e => e.Notas.Average(n => (decimal?)n.Valor)).ThenBy(e => e.Id),
            ("promedio", true) => estudiantes.OrderByDescending(e => e.Notas.Average(n => (decimal?)n.Valor)).ThenBy(e => e.Id),
            (_, true) => estudiantes.OrderByDescending(e => e.Id),
            _ => estudiantes.OrderBy(e => e.Id)
        };

        return estudiantes
            .Select(e => new EstudianteDto(
                e.Id,
                e.Nombre,
                e.Notas.Count,
                e.Notas.Average(n => (decimal?)n.Valor)));
    }

    public async Task<Result<EstudianteDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var estudiante = await db.Estudiantes
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EstudianteDto(
                e.Id,
                e.Nombre,
                e.Notas.Count,
                e.Notas.Average(n => (decimal?)n.Valor)))
            .FirstOrDefaultAsync(cancellationToken);

        return estudiante is null ? NotFound(id) : Result<EstudianteDto>.Success(estudiante);
    }

    public async Task<IReadOnlyList<LookupDto>> GetLookupAsync(CancellationToken cancellationToken) =>
        await db.Estudiantes
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .Select(e => new LookupDto(e.Id, e.Nombre))
            .ToListAsync(cancellationToken);

    public async Task<Result<EstudianteDto>> CreateAsync(EstudianteSaveRequest request, CancellationToken cancellationToken)
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

        var estudiante = new Estudiante { Nombre = Texto.Normalizar(request.Nombre) };
        db.Estudiantes.Add(estudiante);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Estudiante {EstudianteId} creado", estudiante.Id);
        return new EstudianteDto(estudiante.Id, estudiante.Nombre, 0, null);
    }

    public async Task<Result<EstudianteDto>> UpdateAsync(int id, EstudianteSaveRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var estudiante = await db.Estudiantes.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (estudiante is null)
        {
            return NotFound(id);
        }

        var duplicado = await NombreDuplicadoAsync(request.Nombre, idActual: id, cancellationToken);
        if (duplicado is not null)
        {
            return duplicado;
        }

        estudiante.Nombre = Texto.Normalizar(request.Nombre);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Estudiante {EstudianteId} actualizado", id);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var estudiante = await db.Estudiantes.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (estudiante is null)
        {
            return Result.Failure(NotFound(id));
        }

        // ADR 0005: no se borran notas en cascada; se informa al usuario.
        var cantidadNotas = await db.Notas.CountAsync(n => n.IdEstudiante == id, cancellationToken);
        if (cantidadNotas > 0)
        {
            logger.LogWarning(
                "Eliminación rechazada: el estudiante {EstudianteId} tiene {CantidadNotas} notas",
                id, cantidadNotas);
            return Result.Failure(Error.Conflict(
                $"No se puede eliminar el estudiante porque tiene {cantidadNotas} nota(s) registrada(s). Elimine primero sus notas."));
        }

        db.Estudiantes.Remove(estudiante);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Estudiante {EstudianteId} eliminado", id);
        return Result.Success();
    }

    /// <summary>
    /// Devuelve un error de validación si ya existe otro estudiante con el mismo nombre.
    /// En SQL Server la comparación no distingue mayúsculas/minúsculas (collation *_CI_AS).
    /// </summary>
    private async Task<Error?> NombreDuplicadoAsync(string nombre, int? idActual, CancellationToken cancellationToken)
    {
        var normalizado = Texto.Normalizar(nombre);
        var existe = await db.Estudiantes.AnyAsync(
            e => e.Nombre == normalizado && (idActual == null || e.Id != idActual),
            cancellationToken);

        if (!existe)
        {
            return null;
        }

        logger.LogWarning("Nombre de estudiante duplicado rechazado: {Nombre}", normalizado);
        return Error.Validation("nombre", $"Ya existe un estudiante con el nombre «{normalizado}».");
    }

    private Error NotFound(int id)
    {
        logger.LogWarning("Estudiante {EstudianteId} no encontrado", id);
        return Error.NotFound($"No existe un estudiante con id {id}.");
    }
}
