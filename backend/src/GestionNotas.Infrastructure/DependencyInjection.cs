using GestionNotas.Application.Abstractions;
using GestionNotas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionNotas.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "GestionNotas";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Falta la cadena de conexión '{ConnectionStringName}' en appsettings.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                // Reintenta automáticamente ante errores transitorios de red/SQL.
                sql.EnableRetryOnFailure(maxRetryCount: 3);
                sql.MigrationsHistoryTable("__EFMigrationsHistory");
            }));

        // La misma instancia Scoped de AppDbContext se expone a Application como IApplicationDbContext.
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
