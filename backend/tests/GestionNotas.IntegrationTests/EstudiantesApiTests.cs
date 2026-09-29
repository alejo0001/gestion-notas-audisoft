using System.Net;
using System.Net.Http.Json;
using GestionNotas.Application.Common;
using GestionNotas.Application.Estudiantes;

namespace GestionNotas.IntegrationTests;

/// <summary>
/// CRUD de estudiantes a través de HTTP: rutas, códigos de estado, cabeceras, paginación y errores.
/// IClassFixture: una sola instancia del API (y de su base en memoria) para todas las pruebas de la clase.
/// </summary>
public sealed class EstudiantesApiTests(GestionNotasApiFactory factory) : IClassFixture<GestionNotasApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Listar_DevuelvePaginaConMetadatos()
    {
        var response = await _client.GetAsync("/api/estudiantes?page=2&pageSize=5");

        var pagina = await response.LeerAsync<PagedResult<EstudianteDto>>(HttpStatusCode.OK);
        Assert.Equal(2, pagina.Page);
        Assert.Equal(5, pagina.PageSize);
        Assert.Equal(5, pagina.Items.Count);
        Assert.True(pagina.TotalCount >= 12);
    }

    [Fact]
    public async Task Listar_ConPageSizeMayorAlMaximo_LoLimitaA100()
    {
        var pagina = await (await _client.GetAsync("/api/estudiantes?pageSize=5000"))
            .LeerAsync<PagedResult<EstudianteDto>>(HttpStatusCode.OK);

        Assert.Equal(PagedQuery.MaxPageSize, pagina.PageSize);
    }

    [Fact]
    public async Task Listar_ConBusquedaYOrdenDescendente_FiltraYOrdena()
    {
        var pagina = await (await _client.GetAsync("/api/estudiantes?search=Ma&sortBy=nombre&sortDirection=desc&pageSize=50"))
            .LeerAsync<PagedResult<EstudianteDto>>(HttpStatusCode.OK);

        var nombres = pagina.Items.Select(e => e.Nombre).ToList();
        Assert.NotEmpty(nombres);
        Assert.All(nombres, n => Assert.Contains("Ma", n, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(nombres.OrderDescending(StringComparer.CurrentCulture), nombres);
    }

    [Fact]
    public async Task CicloCompleto_Crear_Consultar_Editar_Eliminar()
    {
        var nombre = Apoyo.NombreUnico("Estudiante");

        // Crear: 201 + cabecera Location que apunta al recurso nuevo.
        var creado = await _client.PostAsJsonAsync("/api/estudiantes", new EstudianteSaveRequest(nombre));
        var estudiante = await creado.LeerAsync<EstudianteDto>(HttpStatusCode.Created);
        Assert.Equal(nombre, estudiante.Nombre);
        Assert.NotNull(creado.Headers.Location);

        // Consultar por la URL de Location.
        var consultado = await (await _client.GetAsync(creado.Headers.Location))
            .LeerAsync<EstudianteDto>(HttpStatusCode.OK);
        Assert.Equal(estudiante.Id, consultado.Id);

        // Editar.
        var editado = await (await _client.PutAsJsonAsync($"/api/estudiantes/{estudiante.Id}", new EstudianteSaveRequest(nombre + " E")))
            .LeerAsync<EstudianteDto>(HttpStatusCode.OK);
        Assert.Equal(nombre + " E", editado.Nombre);

        // Eliminar: 204 y después 404.
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/estudiantes/{estudiante.Id}")).StatusCode);
        await (await _client.GetAsync($"/api/estudiantes/{estudiante.Id}")).LeerProblemaAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Crear_ConNombreVacio_Devuelve400ConErrorEnNombre()
    {
        var problema = await (await _client.PostAsJsonAsync("/api/estudiantes", new EstudianteSaveRequest("   ")))
            .LeerProblemaAsync(HttpStatusCode.BadRequest);

        Assert.True(problema.Errors!.ContainsKey("nombre"));
    }

    [Fact]
    public async Task Crear_ConNombreExistenteYEspaciosDeMas_Devuelve400()
    {
        // «Sofía Ramírez» existe en los datos semilla; los espacios extra se normalizan antes de comparar.
        var problema = await (await _client.PostAsJsonAsync("/api/estudiantes", new EstudianteSaveRequest("  Sofía   Ramírez ")))
            .LeerProblemaAsync(HttpStatusCode.BadRequest);

        Assert.Contains("Ya existe", problema.Errors!["nombre"][0]);
    }

    [Fact]
    public async Task Crear_ConJsonMalFormado_Devuelve400()
    {
        using var contenido = new StringContent("{ \"nombre\": ", System.Text.Encoding.UTF8, "application/json");

        await (await _client.PostAsync("/api/estudiantes", contenido)).LeerProblemaAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Obtener_IdInexistente_Devuelve404ConDetalle()
    {
        var problema = await (await _client.GetAsync("/api/estudiantes/999999")).LeerProblemaAsync(HttpStatusCode.NotFound);

        Assert.Contains("999999", problema.Detail);
    }

    [Fact]
    public async Task Eliminar_EstudianteConNotas_Devuelve409()
    {
        // El estudiante 1 tiene notas en los datos semilla (ADR 0005: no se borra en cascada).
        var problema = await (await _client.DeleteAsync("/api/estudiantes/1")).LeerProblemaAsync(HttpStatusCode.Conflict);

        Assert.Contains("nota", problema.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_DevuelveTodosOrdenadosPorNombre()
    {
        var lista = await (await _client.GetAsync("/api/estudiantes/lookup"))
            .LeerAsync<List<LookupDto>>(HttpStatusCode.OK);

        Assert.True(lista.Count >= 12);
        Assert.Equal(lista.Select(x => x.Nombre).Order(StringComparer.CurrentCulture), lista.Select(x => x.Nombre));
    }
}
