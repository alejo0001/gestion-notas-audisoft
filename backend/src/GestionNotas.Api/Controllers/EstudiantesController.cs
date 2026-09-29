using GestionNotas.Application.Common;
using GestionNotas.Application.Estudiantes;
using GestionNotas.Api.Exportacion;
using Microsoft.AspNetCore.Mvc;

namespace GestionNotas.Api.Controllers;

[Route("api/estudiantes")]
public sealed class EstudiantesController(IEstudianteService service) : ApiControllerBase
{
    /// <summary>Lista paginada de estudiantes con búsqueda y ordenamiento.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<EstudianteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EstudianteDto>>> GetPaged(
        [FromQuery] PagedQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.GetPagedAsync(query, cancellationToken));

    /// <summary>Lista id/nombre de todos los estudiantes (para listas desplegables).</summary>
    [HttpGet("lookup")]
    [ProducesResponseType<IReadOnlyList<LookupDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupDto>>> GetLookup(CancellationToken cancellationToken) =>
        Ok(await service.GetLookupAsync(cancellationToken));

    private static readonly ColumnaExcel<EstudianteDto>[] ColumnasExcel =
    [
        new("ID", e => e.Id, 8),
        new("Nombre", e => e.Nombre, 35),
        new("Cantidad de notas", e => e.CantidadNotas, 18),
        new("Promedio", e => e.Promedio, 12, "0.00"),
    ];

    /// <summary>Excel con los estudiantes que cumplen la búsqueda (mismo orden que la tabla).</summary>
    [HttpGet("exportar")]
    [Produces(LibroExcel.ContentType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Exportar([FromQuery] PagedQuery query, CancellationToken cancellationToken)
    {
        var filas = await service.ListarAsync(query, cancellationToken);
        return File(LibroExcel.Crear("Estudiantes", ColumnasExcel, filas), LibroExcel.ContentType, LibroExcel.NombreArchivo("estudiantes"));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<EstudianteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudianteDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType<EstudianteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EstudianteDto>> Create(
        EstudianteSaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ToProblem(result.Error!);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<EstudianteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstudianteDto>> Update(
        int id,
        EstudianteSaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error!);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : ToProblem(result.Error!);
    }
}
