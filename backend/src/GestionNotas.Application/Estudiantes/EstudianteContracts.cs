using FluentValidation;
using GestionNotas.Application.Common;

namespace GestionNotas.Application.Estudiantes;

/// <summary>Datos que devuelve la API. Es un record: inmutable y con igualdad por valor, ideal para DTOs.</summary>
public sealed record EstudianteDto(int Id, string Nombre, int CantidadNotas, decimal? Promedio);

/// <summary>Cuerpo de las peticiones POST/PUT.</summary>
public sealed record EstudianteSaveRequest(string Nombre);

public sealed class EstudianteSaveRequestValidator : AbstractValidator<EstudianteSaveRequest>
{
    public EstudianteSaveRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre del estudiante es obligatorio.")
            .Must(nombre => nombre.Trim().Length >= 2).WithMessage("El nombre debe tener al menos 2 caracteres.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
    }
}

public interface IEstudianteService
{
    Task<PagedResult<EstudianteDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken);

    Task<Result<EstudianteDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<LookupDto>> GetLookupAsync(CancellationToken cancellationToken);

    Task<Result<EstudianteDto>> CreateAsync(EstudianteSaveRequest request, CancellationToken cancellationToken);

    Task<Result<EstudianteDto>> UpdateAsync(int id, EstudianteSaveRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
