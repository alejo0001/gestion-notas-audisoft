using GestionNotas.Api.Infrastructure;
using GestionNotas.Application;
using GestionNotas.Infrastructure;
using GestionNotas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios ----------
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// ProblemDetails (RFC 9457) para todas las respuestas de error, con traceId para rastrear en logs.
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // En desarrollo se acepta cualquier puerto de localhost: el dev server de Angular
            // puede cambiar de puerto si el configurado está ocupado por otro proyecto.
            policy.SetIsOriginAllowed(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback);
        }
        else
        {
            // En producción solo los orígenes explícitos de la configuración.
            policy.WithOrigins(allowedOrigins);
        }

        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Location");
    }));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("base-de-datos");

var app = builder.Build();

// ---------- Pipeline HTTP ----------
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // documento OpenAPI generado por .NET: /openapi/v1.json

    // Swagger UI solo como visor del documento anterior (no se usa el generador de Swashbuckle).
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Gestión de Notas API v1");
        options.DocumentTitle = "Gestión de Notas API";
    });

    if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
else
{
    // En desarrollo el frontend usa http://localhost:5080; redirigir rompería el preflight de CORS.
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

/// <summary>Expuesto para futuras pruebas de integración con WebApplicationFactory.</summary>
public partial class Program { }
