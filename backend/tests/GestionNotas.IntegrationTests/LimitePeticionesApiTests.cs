using System.Net;
using System.Net.Http.Json;
using GestionNotas.Application.Estudiantes;

namespace GestionNotas.IntegrationTests;

/// <summary>
/// Límite de peticiones con un cupo pequeño (3 lecturas y 2 escrituras por minuto, ver ApiConLimiteFactory).
/// Cada prueba simula un cliente distinto con su propia IP en X-Forwarded-For, como hace el ingress de Azure,
/// para que los cupos de una prueba no afecten a otra.
/// </summary>
public sealed class LimitePeticionesApiTests(ApiConLimiteFactory factory) : IClassFixture<ApiConLimiteFactory>
{
    [Fact]
    public async Task SuperarElCupoDeLecturas_Devuelve429ConRetryAfter()
    {
        var cliente = ClienteConIp("10.0.0.1");

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/estudiantes")).StatusCode);
        }

        var rechazada = await cliente.GetAsync("/api/estudiantes");
        var problema = await rechazada.LeerProblemaAsync(HttpStatusCode.TooManyRequests);
        Assert.Equal("Demasiadas peticiones", problema.Title);
        Assert.NotNull(rechazada.Headers.RetryAfter);
    }

    [Fact]
    public async Task CadaIpTieneSuPropioCupo()
    {
        var abusador = ClienteConIp("10.0.0.2");
        for (var i = 0; i < 4; i++)
        {
            await abusador.GetAsync("/api/estudiantes");
        }

        // Otro cliente no se ve afectado por el que agotó su cupo.
        Assert.Equal(HttpStatusCode.OK, (await ClienteConIp("10.0.0.3").GetAsync("/api/estudiantes")).StatusCode);
    }

    [Fact]
    public async Task LecturasYEscriturasTienenCuposSeparados()
    {
        var cliente = ClienteConIp("10.0.0.4");
        for (var i = 0; i < 4; i++)
        {
            await cliente.GetAsync("/api/estudiantes");
        }

        // Con las lecturas agotadas, una escritura aún se procesa (aquí falla por validación: 400, no 429).
        var escritura = await cliente.PostAsJsonAsync("/api/estudiantes", new EstudianteSaveRequest(""));
        Assert.Equal(HttpStatusCode.BadRequest, escritura.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_NoTieneLimite()
    {
        var cliente = ClienteConIp("10.0.0.5");

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/health")).StatusCode);
        }
    }

    [Fact]
    public async Task IpFalsaAlInicioDeXForwardedFor_NoEvitaElLimite()
    {
        // El cliente puede escribir X-Forwarded-For, pero el ingress agrega la IP real AL FINAL: se usa esa.
        for (var i = 0; i < 3; i++)
        {
            await ClienteConIp($"1.1.1.{i}, 10.0.0.6").GetAsync("/api/estudiantes");
        }

        var rechazada = await ClienteConIp("9.9.9.9, 10.0.0.6").GetAsync("/api/estudiantes");
        Assert.Equal(HttpStatusCode.TooManyRequests, rechazada.StatusCode);
    }

    private HttpClient ClienteConIp(string xForwardedFor)
    {
        var cliente = factory.CreateClient();
        cliente.DefaultRequestHeaders.Add("X-Forwarded-For", xForwardedFor);
        return cliente;
    }
}
