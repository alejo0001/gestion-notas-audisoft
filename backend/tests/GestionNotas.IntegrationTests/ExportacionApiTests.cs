using System.Net;
using ClosedXML.Excel;
using GestionNotas.Application.Common;
using GestionNotas.Application.Notas;

namespace GestionNotas.IntegrationTests;

/// <summary>Exportación a Excel: se descarga el archivo y se abre con ClosedXML para revisar su contenido.</summary>
public sealed class ExportacionApiTests(GestionNotasApiFactory factory) : IClassFixture<GestionNotasApiFactory>
{
    private const string ContentTypeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ExportarNotas_ConFiltro_ContieneLasMismasNotasQueElListado()
    {
        var listado = await (await _client.GetAsync("/api/notas?idEstudiante=2&pageSize=100"))
            .LeerAsync<PagedResult<NotaDto>>(HttpStatusCode.OK);

        var response = await _client.GetAsync("/api/notas/exportar?idEstudiante=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ContentTypeExcel, response.Content.Headers.ContentType?.MediaType);
        Assert.StartsWith("notas-", response.Content.Headers.ContentDisposition?.FileNameStar ?? response.Content.Headers.ContentDisposition?.FileName);

        using var libro = new XLWorkbook(await response.Content.ReadAsStreamAsync());
        var hoja = libro.Worksheet("Notas");

        Assert.Equal("Evaluación", hoja.Cell(1, 2).GetString());
        Assert.Equal("Resultado", hoja.Cell(1, 6).GetString());
        Assert.Equal(listado.TotalCount, hoja.RowsUsed().Count() - 1); // menos la fila de encabezado

        // Los valores quedan como números (no texto) y el resultado se calcula con la nota mínima (3,0).
        var primera = listado.Items[0];
        Assert.Equal((double)primera.Valor, hoja.Cell(2, 5).GetDouble());
        Assert.Equal(primera.Valor >= 3.0m ? "Aprobada" : "Reprobada", hoja.Cell(2, 6).GetString());
    }

    [Fact]
    public async Task ExportarEstudiantes_ConBusqueda_SoloIncluyeLasCoincidencias()
    {
        var response = await _client.GetAsync("/api/estudiantes/exportar?search=Ma");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var libro = new XLWorkbook(await response.Content.ReadAsStreamAsync());
        var nombres = libro.Worksheet("Estudiantes").RowsUsed().Skip(1).Select(r => r.Cell(2).GetString()).ToList();

        Assert.NotEmpty(nombres);
        Assert.All(nombres, n => Assert.Contains("Ma", n, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExportarProfesores_IncluyeTodos()
    {
        var response = await _client.GetAsync("/api/profesores/exportar");

        using var libro = new XLWorkbook(await response.Content.ReadAsStreamAsync());
        Assert.Equal(5, libro.Worksheet("Profesores").RowsUsed().Count() - 1);
    }
}
