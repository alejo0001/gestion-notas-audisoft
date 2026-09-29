using System.Net;
using System.Net.Http.Json;
using GestionNotas.Application.Common;
using GestionNotas.Application.Notas;

namespace GestionNotas.IntegrationTests;

public sealed class NotasApiTests(GestionNotasApiFactory factory) : IClassFixture<GestionNotasApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Crear_Valida_Devuelve201ConNombresDeEstudianteYProfesor()
    {
        var evaluacion = Apoyo.NombreUnico("Evaluación");

        var nota = await (await _client.PostAsJsonAsync("/api/notas", new NotaSaveRequest(evaluacion, 3, 1, 4.25m)))
            .LeerAsync<NotaDto>(HttpStatusCode.Created);

        Assert.Equal(4.25m, nota.Valor);
        Assert.Equal("Valentina Muñoz", nota.EstudianteNombre);
        Assert.Equal("Carlos Rodríguez", nota.ProfesorNombre);
    }

    [Theory]
    [InlineData(5.5)]
    [InlineData(-0.5)]
    [InlineData(3.456)]
    public async Task Crear_ConValorInvalido_Devuelve400EnValor(double valor)
    {
        var problema = await (await _client.PostAsJsonAsync("/api/notas", new NotaSaveRequest("Parcial", 3, 1, (decimal)valor)))
            .LeerProblemaAsync(HttpStatusCode.BadRequest);

        Assert.True(problema.Errors!.ContainsKey("valor"));
    }

    [Fact]
    public async Task Crear_ConEstudianteYProfesorInexistentes_ReportaAmbosCampos()
    {
        var problema = await (await _client.PostAsJsonAsync("/api/notas", new NotaSaveRequest("Parcial", 999999, 999999, 4m)))
            .LeerProblemaAsync(HttpStatusCode.BadRequest);

        Assert.True(problema.Errors!.ContainsKey("idEstudiante"));
        Assert.True(problema.Errors.ContainsKey("idProfesor"));
    }

    [Fact]
    public async Task Crear_EvaluacionRepetidaParaElMismoEstudianteYProfesor_Devuelve400()
    {
        // Datos semilla: el estudiante 1 ya tiene «Parcial 2» con el profesor 2.
        var problema = await (await _client.PostAsJsonAsync("/api/notas", new NotaSaveRequest("Parcial 2", 1, 2, 4m)))
            .LeerProblemaAsync(HttpStatusCode.BadRequest);

        Assert.Contains("Parcial 2", problema.Errors!["nombre"][0]);
    }

    [Fact]
    public async Task Crear_MismaEvaluacionConOtroProfesor_Devuelve201()
    {
        // Mismo estudiante y evaluación que la prueba anterior, pero con el profesor 5: es otra nota válida.
        await (await _client.PostAsJsonAsync("/api/notas", new NotaSaveRequest("Parcial 2", 1, 5, 4m)))
            .LeerAsync<NotaDto>(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Listar_FiltradoPorEstudiante_SoloDevuelveSusNotas()
    {
        var pagina = await (await _client.GetAsync("/api/notas?idEstudiante=2&pageSize=50"))
            .LeerAsync<PagedResult<NotaDto>>(HttpStatusCode.OK);

        Assert.NotEmpty(pagina.Items);
        Assert.All(pagina.Items, n => Assert.Equal(2, n.IdEstudiante));
        Assert.Equal(pagina.Items.Count, pagina.TotalCount);
    }

    [Fact]
    public async Task Listar_OrdenadoPorValorDescendente()
    {
        var pagina = await (await _client.GetAsync("/api/notas?sortBy=valor&sortDirection=desc&pageSize=100"))
            .LeerAsync<PagedResult<NotaDto>>(HttpStatusCode.OK);

        var valores = pagina.Items.Select(n => n.Valor).ToList();
        Assert.Equal(valores.OrderDescending(), valores);
    }

    [Fact]
    public async Task Editar_Y_Eliminar_Nota()
    {
        var creada = await (await _client.PostAsJsonAsync("/api/notas", new NotaSaveRequest(Apoyo.NombreUnico("Quiz"), 4, 3, 2m)))
            .LeerAsync<NotaDto>(HttpStatusCode.Created);

        var editada = await (await _client.PutAsJsonAsync($"/api/notas/{creada.Id}", new NotaSaveRequest(creada.Nombre, 4, 3, 3.75m)))
            .LeerAsync<NotaDto>(HttpStatusCode.OK);
        Assert.Equal(3.75m, editada.Valor);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/notas/{creada.Id}")).StatusCode);
        await (await _client.DeleteAsync($"/api/notas/{creada.Id}")).LeerProblemaAsync(HttpStatusCode.NotFound);
    }
}
