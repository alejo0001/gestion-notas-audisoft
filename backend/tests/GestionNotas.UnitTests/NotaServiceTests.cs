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
    public async Task CreateAsync_ConMismaEvaluacionEstudianteYProfesor_DevuelveErrorEnNombre()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var profesor = db.AddProfesor("Carlos Rodríguez");
        var service = CreateService(db);
        await service.CreateAsync(new NotaSaveRequest("Parcial 1", estudiante.Id, profesor.Id, 4m), CancellationToken.None);

        // Mismo texto con espacios de más: debe reconocerse como la misma evaluación.
        var result = await service.CreateAsync(
            new NotaSaveRequest("  Parcial   1 ", estudiante.Id, profesor.Id, 2.5m),
            CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.True(result.Error.ValidationErrors!.ContainsKey("nombre"));
        Assert.Single(db.Notas);
    }

    [Fact]
    public async Task CreateAsync_MismaEvaluacionConOtroProfesor_EsValida()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var profesor1 = db.AddProfesor("Carlos Rodríguez");
        var profesor2 = db.AddProfesor("María López");
        var service = CreateService(db);
        await service.CreateAsync(new NotaSaveRequest("Parcial 1", estudiante.Id, profesor1.Id, 4m), CancellationToken.None);

        var result = await service.CreateAsync(
            new NotaSaveRequest("Parcial 1", estudiante.Id, profesor2.Id, 3m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, db.Notas.Count());
    }

    [Fact]
    public async Task CreateAsync_MismaEvaluacionConOtroEstudiante_EsValida()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante1 = db.AddEstudiante("Ana Pérez");
        var estudiante2 = db.AddEstudiante("Luis Gómez");
        var profesor = db.AddProfesor("Carlos Rodríguez");
        var service = CreateService(db);
        await service.CreateAsync(new NotaSaveRequest("Parcial 1", estudiante1.Id, profesor.Id, 4m), CancellationToken.None);

        var result = await service.CreateAsync(
            new NotaSaveRequest("Parcial 1", estudiante2.Id, profesor.Id, 3m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_ConservandoSuPropiaEvaluacion_EsValido()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var profesor = db.AddProfesor("Carlos Rodríguez");
        var service = CreateService(db);
        var creada = await service.CreateAsync(
            new NotaSaveRequest("Parcial 1", estudiante.Id, profesor.Id, 4m), CancellationToken.None);

        // Solo cambia el valor: no debe compararse consigo misma.
        var result = await service.UpdateAsync(
            creada.Value!.Id,
            new NotaSaveRequest("Parcial 1", estudiante.Id, profesor.Id, 4.5m),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4.5m, result.Value!.Valor);
    }

    [Fact]
    public async Task UpdateAsync_QueDuplicaOtraEvaluacion_DevuelveErrorEnNombre()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var profesor1 = db.AddProfesor("Carlos Rodríguez");
        var profesor2 = db.AddProfesor("María López");
        var service = CreateService(db);
        await service.CreateAsync(new NotaSaveRequest("Parcial 1", estudiante.Id, profesor1.Id, 4m), CancellationToken.None);
        var otra = await service.CreateAsync(
            new NotaSaveRequest("Parcial 1", estudiante.Id, profesor2.Id, 3m), CancellationToken.None);

        // Cambiar el profesor de la segunda nota la volvería idéntica a la primera.
        var result = await service.UpdateAsync(
            otra.Value!.Id,
            new NotaSaveRequest("Parcial 1", estudiante.Id, profesor1.Id, 3m),
            CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.True(result.Error.ValidationErrors!.ContainsKey("nombre"));
    }

    [Fact]
    public async Task UpdateAsync_ConIdInexistente_DevuelveNotFound()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var service = CreateService(db);

        var result = await service.UpdateAsync(999, new NotaSaveRequest("Parcial 1", 1, 1, 4m), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
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
