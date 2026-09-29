using GestionNotas.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GestionNotas.Api.Controllers;

/// <summary>
/// Base común: traduce los errores esperados del <see cref="Result"/> a respuestas HTTP con ProblemDetails (ADR 0004).
/// </summary>
[ApiController]
// Sin [Produces("application/json")]: ese atributo forzaba el tipo en TODAS las respuestas, incluidos los errores,
// y reemplazaba el "application/problem+json" que exige el RFC 9457. Sin él, los DTO salen como application/json
// y los errores (400/404/409) como application/problem+json.
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ToProblem(Error error) => error.Type switch
    {
        ErrorType.NotFound => Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Recurso no encontrado",
            detail: error.Message),

        ErrorType.Conflict => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Operación no permitida",
            detail: error.Message),

        ErrorType.Validation => ValidationProblem(
            new ValidationProblemDetails(
                (error.ValidationErrors ?? new Dictionary<string, string[]>())
                    .ToDictionary(pair => pair.Key, pair => pair.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = error.Message
            }),

        _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
    };
}
