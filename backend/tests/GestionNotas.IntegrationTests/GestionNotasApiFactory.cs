using GestionNotas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GestionNotas.IntegrationTests;

/// <summary>
/// Levanta el API COMPLETO en memoria (Program.cs real: middleware, controladores, validación, ProblemDetails,
/// CORS, límite de peticiones) y le hace peticiones HTTP de verdad, sin abrir ningún puerto.
///
/// Lo único que se reemplaza es SQL Server por EF Core InMemory, con los datos semilla (5 profesores,
/// 12 estudiantes, 28 notas). Cada instancia usa su propia base, así que las clases de prueba no se pisan.
/// Nota: InMemory no aplica FKs ni índices únicos; esas reglas se prueban a través de los servicios, que las
/// validan antes de guardar.
/// </summary>
public class GestionNotasApiFactory : WebApplicationFactory<Program>
{
    private readonly string _nombreBaseDatos = $"gestion-notas-integracion-{Guid.NewGuid()}";

    // Raíz propia: garantiza que todas las instancias de AppDbContext de ESTA fábrica vean la misma base
    // en memoria y que ninguna otra fábrica la comparta.
    private readonly InMemoryDatabaseRoot _raiz = new();

    /// <summary>Configuración adicional; las subclases la usan para, por ejemplo, activar el límite de peticiones.</summary>
    protected virtual IDictionary<string, string?> Configuracion => new Dictionary<string, string?>
    {
        ["RateLimiting:Enabled"] = "false",
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Entorno propio: no es Development (no intenta migraciones de SQL Server ni carga
        // appsettings.Development.json), así que se prueba la configuración base, la de producción.
        builder.UseEnvironment("Testing");

        foreach (var (clave, valor) in Configuracion)
        {
            builder.UseSetting(clave, valor);
        }

        builder.ConfigureServices(services =>
        {
            // Quita el registro de SQL Server. Desde EF Core 9, AddDbContext también registra
            // IDbContextOptionsConfiguration<T>: si no se quita, se mezclarían los dos proveedores.
            var registrosSqlServer = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
                .ToList();
            foreach (var registro in registrosSqlServer)
            {
                services.Remove(registro);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_nombreBaseDatos, _raiz));
        });
    }

    /// <summary>Tras construir el host, crea la base e inserta los datos semilla (HasData).</summary>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        return host;
    }
}

/// <summary>Variante con el límite de peticiones activo y un cupo pequeño para poder superarlo en una prueba.</summary>
public sealed class ApiConLimiteFactory : GestionNotasApiFactory
{
    protected override IDictionary<string, string?> Configuracion => new Dictionary<string, string?>
    {
        ["RateLimiting:Enabled"] = "true",
        ["RateLimiting:LecturasPorVentana"] = "3",
        ["RateLimiting:EscriturasPorVentana"] = "2",
        ["RateLimiting:VentanaSegundos"] = "60",
    };
}
