using GestionNotas.Application.Common;
using GestionNotas.Application.Estudiantes;
using GestionNotas.Domain.Entities;
using GestionNotas.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionNotas.UnitTests;

public sealed class EstudianteServiceTests
{
    private static EstudianteService CreateService(AppDbContext db) =>
        new(db, new EstudianteSaveRequestValidator(), NullLogger<EstudianteService>.Instance);

    [Fact]
    public async Task CreateAsync_ConNombreValido_CreaEstudianteSinEspaciosSobrantes()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var service = CreateService(db);

        var result = await service.CreateAsync(new EstudianteSaveRequest("  Laura Sánchez  "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Laura Sánchez", result.Value!.Nombre);
        Assert.Equal(1, db.Estudiantes.Count());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public async Task CreateAsync_ConNombreInvalido_DevuelveErrorDeValidacion(string nombre)
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var service = CreateService(db);

        var result = await service.CreateAsync(new EstudianteSaveRequest(nombre), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.True(result.Error.ValidationErrors!.ContainsKey("nombre"));
        Assert.Empty(db.Estudiantes);
    }

    [Fact]
    public async Task CreateAsync_ConNombreExistente_DevuelveErrorEnNombre()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        db.AddEstudiante("Samuel Torres");
        var service = CreateService(db);

        var result = await service.CreateAsync(new EstudianteSaveRequest("  Samuel Torres "), CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.True(result.Error.ValidationErrors!.ContainsKey("nombre"));
        Assert.Single(db.Estudiantes);
    }

    [Fact]
    public async Task UpdateAsync_ConservandoSuPropioNombre_EsValido()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Samuel Torres");
        var service = CreateService(db);

        var result = await service.UpdateAsync(estudiante.Id, new EstudianteSaveRequest("Samuel Torres"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_ConNombreDeOtroEstudiante_DevuelveErrorEnNombre()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        db.AddEstudiante("Samuel Torres");
        var otro = db.AddEstudiante("Camila Ortiz");
        var service = CreateService(db);

        var result = await service.UpdateAsync(otro.Id, new EstudianteSaveRequest("Samuel Torres"), CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task UpdateAsync_ConIdInexistente_DevuelveNotFound()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var service = CreateService(db);

        var result = await service.UpdateAsync(999, new EstudianteSaveRequest("Pedro Pérez"), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task DeleteAsync_EstudianteConNotas_DevuelveConflictoYNoElimina()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var profesor = db.AddProfesor("Carlos Rodríguez");
        db.Notas.Add(new Nota { Nombre = "Parcial 1", IdEstudiante = estudiante.Id, IdProfesor = profesor.Id, Valor = 4.0m });
        await db.SaveChangesAsync();
        var service = CreateService(db);

        var result = await service.DeleteAsync(estudiante.Id, CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Single(db.Estudiantes);
    }

    [Fact]
    public async Task DeleteAsync_EstudianteSinNotas_Elimina()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        var estudiante = db.AddEstudiante("Ana Pérez");
        var service = CreateService(db);

        var result = await service.DeleteAsync(estudiante.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.Estudiantes);
    }

    [Fact]
    public async Task GetPagedAsync_DevuelvePaginaSolicitadaYTotales()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        for (var i = 1; i <= 12; i++)
        {
            db.AddEstudiante($"Estudiante {i:00}");
        }
        var service = CreateService(db);

        var page = await service.GetPagedAsync(
            new PagedQuery { Page = 3, PageSize = 5, SortBy = "nombre" },
            CancellationToken.None);

        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal("Estudiante 11", page.Items[0].Nombre);
    }

    [Fact]
    public async Task GetPagedAsync_ConBusqueda_FiltraPorNombre()
    {
        await using var db = TestDbContextFactory.CreateEmpty();
        db.AddEstudiante("Mariana Vargas");
        db.AddEstudiante("Mateo Rojas");
        db.AddEstudiante("Camila Ortiz");
        var service = CreateService(db);

        var page = await service.GetPagedAsync(new PagedQuery { Search = "Ma" }, CancellationToken.None);

        Assert.Equal(2, page.TotalCount);
    }
}
