using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionNotas.Api.Infrastructure;

/// <summary>
/// Último punto de captura para fallas realmente excepcionales (ADR 0004).
/// Registra la excepción completa y responde con ProblemDetails sin exponer detalles internos.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            // El cliente cerró la conexión: no es un error del servidor.
            logger.LogInformation("Petición {Method} {Path} cancelada por el cliente",
                httpContext.Request.Method, httpContext.Request.Path);
            httpContext.Response.StatusCode = 499;
            return true;
        }

        var (status, title, detail) = exception switch
        {
            // Condición de carrera: se validó antes, pero la BD rechazó por una FK (ADR 0005).
            DbUpdateException => (
                StatusCodes.Status409Conflict,
                "Conflicto al guardar los datos",
                "La operación no se pudo completar porque entra en conflicto con otros registros. Actualice la página e intente de nuevo."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error inesperado. Intente de nuevo más tarde.")
        };

        logger.LogError(exception,
            "Error no controlado en {Method} {Path}. TraceId: {TraceId}",
            httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }
}
