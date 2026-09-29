using GestionNotas.Application.Common;
using GestionNotas.Application.Notas;
using GestionNotas.Domain.Entities;
using GestionNotas.Api.Exportacion;
using Microsoft.AspNetCore.Mvc;

namespace GestionNotas.Api.Controllers;

[Route("api/notas")]
public sealed class NotasController(INotaService service) : ApiControllerBase
{
    /// <summary>Lista paginada de notas. Permite filtrar por estudiante y/o profesor.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<NotaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotaDto>>> GetPaged(
        [FromQuery] NotaQuery query,
        CancellationToken cancellationToken) =>
        Ok(await service.GetPagedAsync(query, cancellationToken));

    private static readonly ColumnaExcel<NotaDto>[] ColumnasExcel =
    [
        new("ID", n => n.Id, 8),
        new("Evaluación", n => n.Nombre, 28),
        new("Estudiante", n => n.EstudianteNombre, 28),
        new("Profesor", n => n.ProfesorNombre, 28),
        new("Valor", n => n.Valor, 10, "0.00"),
        new("Resultado", n => n.Valor >= Nota.NotaMinimaAprobatoria ? "Aprobada" : "Reprobada", 14),
    ];

    /// <summary>Excel con las notas que cumplen la búsqueda y los filtros por estudiante y profesor (mismo orden que la tabla).</summary>
    [HttpGet("exportar")]
    [Produces(LibroExcel.ContentType)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Exportar([FromQuery] NotaQuery query, CancellationToken cancellationToken)
    {
        var filas = await service.ListarAsync(query, cancellationToken);
        return File(LibroExcel.Crear("Notas", ColumnasExcel, filas), LibroExcel.ContentType, LibroExcel.NombreArchivo("notas"));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<NotaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType<NotaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotaDto>> Create(NotaSaveRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ToProblem(result.Error!);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<NotaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaDto>> Update(int id, NotaSaveRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error!);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : ToProblem(result.Error!);
    }
}
