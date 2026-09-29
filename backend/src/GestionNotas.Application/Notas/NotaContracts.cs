using FluentValidation;
using GestionNotas.Application.Common;
using GestionNotas.Domain.Entities;

namespace GestionNotas.Application.Notas;

public sealed record NotaDto(
    int Id,
    string Nombre,
    int IdEstudiante,
    string EstudianteNombre,
    int IdProfesor,
    string ProfesorNombre,
    decimal Valor);

public sealed record NotaSaveRequest(string Nombre, int IdEstudiante, int IdProfesor, decimal Valor);

/// <summary>Filtros adicionales del listado de notas. Hereda paginación, búsqueda y orden.</summary>
public sealed record NotaQuery : PagedQuery
{
    public int? IdEstudiante { get; init; }

    public int? IdProfesor { get; init; }
}

public sealed class NotaSaveRequestValidator : AbstractValidator<NotaSaveRequest>
{
    public NotaSaveRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre de la nota es obligatorio.")
            .Must(nombre => nombre.Trim().Length >= 2).WithMessage("El nombre debe tener al menos 2 caracteres.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.IdEstudiante)
            .GreaterThan(0).WithMessage("Debe seleccionar un estudiante.");

        RuleFor(x => x.IdProfesor)
            .GreaterThan(0).WithMessage("Debe seleccionar un profesor.");

        RuleFor(x => x.Valor)
            .InclusiveBetween(Nota.ValorMinimo, Nota.ValorMaximo)
                .WithMessage("La nota debe estar entre 0,0 y 5,0.")
            .PrecisionScale(3, 2, ignoreTrailingZeros: true)
                .WithMessage("La nota admite máximo 2 decimales.");
    }
}

public interface INotaService
{
    Task<PagedResult<NotaDto>> GetPagedAsync(NotaQuery query, CancellationToken cancellationToken);

    Task<Result<NotaDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<NotaDto>> CreateAsync(NotaSaveRequest request, CancellationToken cancellationToken);

    Task<Result<NotaDto>> UpdateAsync(int id, NotaSaveRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
