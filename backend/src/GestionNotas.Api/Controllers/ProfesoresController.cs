using GestionNotas.Application.Common;
using GestionNotas.Application.Profesores;
using GestionNotas.Api.Exportacion;
using Microsoft.AspNetCore.Mvc;

namespace GestionNotas.Api.Controllers;

[Route("api/profesores")]
public sealed class ProfesoresController(IProfesorService service) : ApiControllerBase
{
    /// <summary>Lista paginada de profesores con búsqueda y ordenamiento.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ProfesorDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProfesorDto>>> GetPaged(
        [FromQuery] PagedQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.GetPagedAsync(query, cancellationToken));

    /// <summary>Lista id/nombre de todos los profesores (para listas desplegables).</summary>
    [HttpGet("lookup")]
    [ProducesResponseType<IReadOnlyList<LookupDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupDto>>> GetLookup(CancellationToken cancellationToken) =>
        Ok(await service.GetLookupAsync(cancellationToken));

    private static readonly ColumnaExcel<ProfesorDto>[] ColumnasExcel =
    [
        new("ID", p => p.Id, 8),
        new("Nombre", p => p.Nombre, 35),
        new("Cantidad de notas", p => p.CantidadNotas, 18),
    ];

    /// <summary>Excel con los profesores que cumplen la búsqueda (mismo orden que la tabla).</summary>
    [HttpGet("exportar")]
    [Produces(LibroExcel.ContentType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Exportar([FromQuery] PagedQuery query, CancellationToken cancellationToken)
    {
        var filas = await service.ListarAsync(query, cancellationToken);
        return File(LibroExcel.Crear("Profesores", ColumnasExcel, filas), LibroExcel.ContentType, LibroExcel.NombreArchivo("profesores"));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProfesorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfesorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType<ProfesorDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProfesorDto>> Create(
        ProfesorSaveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ToProblem(result.Error!);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ProfesorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfesorDto>> Update(
        int id,
        ProfesorSaveRequest request,
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
