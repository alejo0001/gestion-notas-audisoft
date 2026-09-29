using System.Net;
using System.Net.Http.Json;
using GestionNotas.Application.Profesores;

namespace GestionNotas.IntegrationTests;

public sealed class ProfesoresApiTests(GestionNotasApiFactory factory) : IClassFixture<GestionNotasApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Crear_Valido_Devuelve201SinNotas()
    {
        var nombre = Apoyo.NombreUnico("Profesor");

        var profesor = await (await _client.PostAsJsonAsync("/api/profesores", new ProfesorSaveRequest(nombre)))
            .LeerAsync<ProfesorDto>(HttpStatusCode.Created);

        Assert.Equal(nombre, profesor.Nombre);
        Assert.Equal(0, profesor.CantidadNotas);
    }

    [Fact]
    public async Task Editar_ConNombreDeOtroProfesor_Devuelve400()
    {
        // Renombrar el profesor 2 como el profesor 1 («Carlos Rodríguez», datos semilla).
        var problema = await (await _client.PutAsJsonAsync("/api/profesores/2", new ProfesorSaveRequest("Carlos Rodríguez")))
            .LeerProblemaAsync(HttpStatusCode.BadRequest);

        Assert.True(problema.Errors!.ContainsKey("nombre"));
    }

    [Fact]
    public async Task Editar_IdInexistente_Devuelve404()
    {
        await (await _client.PutAsJsonAsync("/api/profesores/999999", new ProfesorSaveRequest("No existe")))
            .LeerProblemaAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Eliminar_ProfesorConNotas_Devuelve409()
    {
        await (await _client.DeleteAsync("/api/profesores/2")).LeerProblemaAsync(HttpStatusCode.Conflict);
    }
}
