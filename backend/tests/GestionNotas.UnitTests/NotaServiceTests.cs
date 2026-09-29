using GestionNotas.Application.Common;
using GestionNotas.Application.Notas;
using GestionNotas.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionNotas.UnitTests;

public sealed class NotaServiceTests
{
    private static NotaService CreateService(AppDbContext db) =>
        new(db, new NotaSaveRequestValidator(), NullLogger<NotaService>.Instance);

    [Fact]
    public async Task CreateAsync_ConDatosValidos_DevuelveNotaConNombres()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var profesor = db.AddProfesor("Carlos Rodríguez");
        var service = CreateService(db);

        var result = await service.CreateAsync(
            new NotaSaveRequest("Parcial 1", estudiante.Id, profesor.Id, 4.25m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ana Pérez", result.Value!.EstudianteNombre);
        Assert.Equal("Carlos Rodríguez", result.Value.ProfesorNombre);
        Assert.Equal(4.25m, result.Value.Valor);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(5.01)]
    [InlineData(4.555)]
    public async Task CreateAsync_ConValorInvalido_DevuelveErrorEnValor(double valor)
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var profesor = db.AddProfesor("Carlos Rodríguez");
        var service = CreateService(db);

        var result = await service.CreateAsync(
            new NotaSaveRequest("Parcial 1", estudiante.Id, profesor.Id, (decimal)valor),
            CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.True(result.Error.ValidationErrors!.ContainsKey("valor"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public async Task CreateAsync_ConValorEnElLimite_EsValida(int valor)
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var profesor = db.AddProfesor("Carlos Rodríguez");
        var service = CreateService(db);

        var result = await service.CreateAsync(
            new NotaSaveRequest("Examen final", estudiante.Id, profesor.Id, valor),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateAsync_ConEstudianteYProfesorInexistentes_ReportaAmbosCampos()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var service = CreateService(db);

        var result = await service.CreateAsync(
            new NotaSaveRequest("Parcial 1", 999, 998, 3.5m),
            CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.True(result.Error.ValidationErrors!.ContainsKey("idEstudiante"));
        Assert.True(result.Error.ValidationErrors.ContainsKey("idProfesor"));
        Assert.Empty(db.Notas);
    }

    [Fact]
    public async Task GetPagedAsync_FiltradoPorEstudiante_SoloDevuelveSusNotas()
    {
        // Usa los datos semilla: 28 notas repartidas entre 11 estudiantes.
        await using var db = TestDbContextFactory.Create();
        var service = CreateService(db);

        var page = await service.GetPagedAsync(
            new NotaQuery { IdEstudiante = 1, PageSize = 50 },
            CancellationToken.None);

        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, nota => Assert.Equal(1, nota.IdEstudiante));
        Assert.Equal(page.Items.Count, page.TotalCount);
    }
}
