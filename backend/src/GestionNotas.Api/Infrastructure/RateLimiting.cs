using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace GestionNotas.Api.Infrastructure;

/// <summary>Límites por IP y por ventana de tiempo. Sección "RateLimiting" de appsettings.</summary>
public sealed class RateLimitingOptions
{
    public const string Section = "RateLimiting";

    /// <summary>Apagado en Development (appsettings.Development.json); encendido en producción.</summary>
    public bool Enabled { get; init; }

    /// <summary>Peticiones de lectura (GET) permitidas por IP en cada ventana.</summary>
    public int LecturasPorVentana { get; init; } = 120;

    /// <summary>Peticiones de escritura (POST, PUT, DELETE) permitidas por IP en cada ventana.</summary>
    public int EscriturasPorVentana { get; init; } = 40;

    public int VentanaSegundos { get; init; } = 60;
}

/// <summary>
/// Limitación de peticiones (rate limiting) con el middleware incluido en ASP.NET Core.
/// Protege la demo pública de ráfagas de peticiones: al superar el cupo responde 429 Too Many Requests.
/// </summary>
internal static class RateLimitingExtensions
{
    public static IServiceCollection AddLimiteDePeticiones(this IServiceCollection services, IConfiguration configuration)
    {
        // Patrón Options: la sección se enlaza a una clase tipada y se lee en cada petición con IOptionsMonitor.
        // Así la configuración final (appsettings + entorno + variables + la que inyectan las pruebas de
        // integración) decide, sin importar en qué momento del arranque se agregó.
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.Section));

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Un limitador global con una "partición" (cubeta) independiente por IP y por tipo de operación:
            // un cliente que abusa agota SU cupo sin afectar a los demás.
            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var opciones = context.RequestServices.GetRequiredService<IOptionsMonitor<RateLimitingOptions>>().CurrentValue;

                // Sin límite: si está deshabilitado (Development), preflight de CORS (lo hace el navegador,
                // no el usuario) y el health check.
                if (!opciones.Enabled
                    || HttpMethods.IsOptions(context.Request.Method)
                    || context.Request.Path.StartsWithSegments("/health"))
                {
                    return RateLimitPartition.GetNoLimiter("sin-limite");
                }

                var esEscritura = !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method);
                var clave = $"{ObtenerIpCliente(context)}:{(esEscritura ? "escritura" : "lectura")}";

                // Ventana fija: cada IP tiene N peticiones por minuto; el contador se reinicia al cerrar la ventana.
                return RateLimitPartition.GetFixedWindowLimiter(clave, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = esEscritura ? opciones.EscriturasPorVentana : opciones.LecturasPorVentana,
                    Window = TimeSpan.FromSeconds(opciones.VentanaSegundos),
                    QueueLimit = 0, // sin cola: se rechaza de inmediato en lugar de hacer esperar la petición
                });
            });

            // Respuesta 429 con ProblemDetails en español y cabecera Retry-After (segundos para reintentar).
            limiter.OnRejected = async (context, cancellationToken) =>
            {
                var http = context.HttpContext;
                var opciones = http.RequestServices.GetRequiredService<IOptionsMonitor<RateLimitingOptions>>().CurrentValue;
                var segundos = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (int)Math.Ceiling(retryAfter.TotalSeconds)
                    : opciones.VentanaSegundos;

                http.Response.Headers.RetryAfter = segundos.ToString(CultureInfo.InvariantCulture);
                http.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GestionNotas.RateLimiting")
                    .LogWarning("Límite de peticiones superado por {Ip} en {Method} {Path}",
                        ObtenerIpCliente(http), http.Request.Method, http.Request.Path);

                await http.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = http,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Demasiadas peticiones",
                        Detail = $"Se superó el límite de peticiones permitidas. Intente de nuevo en {segundos} segundos.",
                    },
                });
            };
        });

        return services;
    }

    /// <summary>El middleware siempre está en el pipeline; si el límite está deshabilitado, no restringe nada.</summary>
    public static WebApplication UseLimiteDePeticiones(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }

    /// <summary>
    /// IP real del cliente. En Azure Container Apps el API está detrás de un proxy (el ingress), así que
    /// RemoteIpAddress es la IP del proxy: si se usara, TODOS los visitantes compartirían un único cupo.
    /// El ingress agrega la IP del cliente al FINAL de X-Forwarded-For. Se toma el último valor porque
    /// los anteriores los puede escribir el propio cliente para intentar evadir el límite.
    /// </summary>
    private static string ObtenerIpCliente(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var ultima = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[^1];
            if (IPAddress.TryParse(ultima, out var ip))
            {
                return ip.ToString();
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
    }
}
