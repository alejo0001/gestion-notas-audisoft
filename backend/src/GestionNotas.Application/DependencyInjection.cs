using FluentValidation;
using GestionNotas.Application.Estudiantes;
using GestionNotas.Application.Notas;
using GestionNotas.Application.Profesores;
using GestionNotas.Application.Reportes;
using Microsoft.Extensions.DependencyInjection;

namespace GestionNotas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Scoped: una instancia por petición HTTP, igual que el DbContext del que dependen.
        // Si fueran Singleton capturarían un DbContext Scoped (dependencia cautiva).
        services.AddScoped<IEstudianteService, EstudianteService>();
        services.AddScoped<IProfesorService, ProfesorService>();
        services.AddScoped<INotaService, NotaService>();
        services.AddScoped<IReporteService, ReporteService>();

        services.AddValidatorsFromAssemblyContaining<EstudianteSaveRequestValidator>(includeInternalTypes: true);

        return services;
    }
}
