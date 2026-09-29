using System.Net;
using GestionNotas.Application.Reportes;

namespace GestionNotas.IntegrationTests;

/// <summary>Piezas transversales del pipeline: health check, reportes y CORS.</summary>
public sealed class InfraestructuraApiTests(GestionNotasApiFactory factory) : IClassFixture<GestionNotasApiFactory>
{
    private const string OrigenPermitido = "http://localhost:4300"; // Cors:AllowedOrigins en appsettings.json

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_ConBaseDisponible_DevuelveHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Resumen_EsConsistenteConLosDatosSemilla()
    {
        var resumen = await (await _client.GetAsync("/api/reportes/resumen")).LeerAsync<ResumenDto>(HttpStatusCode.OK);

        Assert.Equal(12, resumen.TotalEstudiantes);
        Assert.Equal(5, resumen.TotalProfesores);
        Assert.Equal(28, resumen.TotalNotas);
        Assert.Equal(resumen.TotalNotas, resumen.NotasAprobadas + resumen.NotasReprobadas);
        Assert.InRange(resumen.MejoresEstudiantes.Count, 1, 5);
        Assert.Equal(resumen.MejoresEstudiantes.Select(e => e.Promedio).OrderDescending(), resumen.MejoresEstudiantes.Select(e => e.Promedio));
    }

    [Fact]
    public async Task Cors_PreflightDesdeOrigenPermitido_AutorizaElOrigen()
    {
        var response = await _client.SendAsync(Preflight(OrigenPermitido));

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origenes));
        Assert.Equal(OrigenPermitido, Assert.Single(origenes));
    }

    [Fact]
    public async Task Cors_PreflightDesdeOrigenNoPermitido_NoAutorizaElOrigen()
    {
        var response = await _client.SendAsync(Preflight("https://sitio-desconocido.example"));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    /// <summary>Petición OPTIONS que el navegador envía antes de un POST desde otro origen.</summary>
    private static HttpRequestMessage Preflight(string origen)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/estudiantes");
        request.Headers.Add("Origin", origen);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");
        return request;
    }
}
