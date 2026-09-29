using FluentValidation;
using GestionNotas.Application.Common;

namespace GestionNotas.Application.Profesores;

public sealed record ProfesorDto(int Id, string Nombre, int CantidadNotas);

public sealed record ProfesorSaveRequest(string Nombre);

public sealed class ProfesorSaveRequestValidator : AbstractValidator<ProfesorSaveRequest>
{
    public ProfesorSaveRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre del profesor es obligatorio.")
            .Must(nombre => nombre.Trim().Length >= 2).WithMessage("El nombre debe tener al menos 2 caracteres.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
    }
}

public interface IProfesorService
{
    Task<PagedResult<ProfesorDto>> GetPagedAsync(PagedQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProfesorDto>> ListarAsync(PagedQuery query, CancellationToken cancellationToken);

    Task<Result<ProfesorDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<LookupDto>> GetLookupAsync(CancellationToken cancellationToken);

    Task<Result<ProfesorDto>> CreateAsync(ProfesorSaveRequest request, CancellationToken cancellationToken);

    Task<Result<ProfesorDto>> UpdateAsync(int id, ProfesorSaveRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
