using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestionNotas.Infrastructure.Persistence;

/// <summary>
/// Crea el <see cref="AppDbContext"/> para las herramientas de EF Core (dotnet ef, migrations bundle)
/// SIN arrancar el API: no depende de Program.cs, appsettings.json ni de la inyección de dependencias.
/// EF la detecta automáticamente y la prefiere sobre el host de la aplicación.
///
/// La cadena de conexión solo se usa como valor por defecto: el bundle la reemplaza con --connection
/// y dotnet ef database update acepta --connection también. Se puede sobrescribir con la variable de
/// entorno ConnectionStrings__GestionNotas.
/// </summary>
public sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string ConexionPorDefecto =
        "Server=(localdb)\\MSSQLLocalDB;Database=GestionNotas;Trusted_Connection=True;TrustServerCertificate=True";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__GestionNotas") ?? ConexionPorDefecto;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(maxRetryCount: 3))
            .Options;

        return new AppDbContext(options);
    }
}
