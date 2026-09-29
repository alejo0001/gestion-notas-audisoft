using GestionNotas.Application.Common;
using GestionNotas.Application.Notas;
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
