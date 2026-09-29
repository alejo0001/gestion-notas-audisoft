using System.Net;
using System.Net.Http.Json;

namespace GestionNotas.IntegrationTests;

/// <summary>Cuerpo de las respuestas de error (ProblemDetails / ValidationProblemDetails).</summary>
public sealed record ProblemaRespuesta(
    int? Status,
    string? Title,
    string? Detail,
    Dictionary<string, string[]>? Errors);

internal static class Apoyo
{
    public const string ProblemJson = "application/problem+json";

    /// <summary>Nombre único por prueba: las pruebas de una clase comparten base y no deben chocar entre sí.</summary>
    public static string NombreUnico(string prefijo) => $"{prefijo} {Guid.NewGuid():N}";

    /// <summary>Verifica que la respuesta sea un ProblemDetails (RFC 9457) con el código esperado.</summary>
    public static async Task<ProblemaRespuesta> LeerProblemaAsync(this HttpResponseMessage response, HttpStatusCode esperado)
    {
        Assert.Equal(esperado, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);

        var problema = await response.Content.ReadFromJsonAsync<ProblemaRespuesta>();
        Assert.NotNull(problema);
        Assert.Equal((int)esperado, problema.Status);
        return problema;
    }

    public static async Task<T> LeerAsync<T>(this HttpResponseMessage response, HttpStatusCode esperado)
    {
        Assert.Equal(esperado, response.StatusCode);
        var cuerpo = await response.Content.ReadFromJsonAsync<T>();
        Assert.NotNull(cuerpo);
        return cuerpo;
    }
}
