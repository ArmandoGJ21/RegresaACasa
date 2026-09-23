using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Data;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Services;

// Variables de backend/.env (ver backend/.env.example). Las del sistema tienen prioridad.
DotEnvFile.Load(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);

// ---------- Controllers + JSON (snake_case como en el contrato) ----------
builder.Services
    .AddControllers(options =>
    {
        // Los errores de validación usan el nombre JSON del campo ("zone", no "Zone").
        options.ModelMetadataDetailsProviders.Add(
            new SystemTextJsonValidationMetadataProvider(JsonNamingPolicy.SnakeCaseLower));
        options.ModelMetadataDetailsProviders.Add(new SnakeCaseDisplayNameProvider());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // 400 => { "success": false, "message": "El campo 'zone' es requerido" }
        options.InvalidModelStateResponseFactory = context =>
        {
            var message = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                ?? "La solicitud no es válida";
            return new BadRequestObjectResult(new ErrorResponse(message));
        };
    });

builder.Services.AddOpenApi();

// ---------- Base de datos ----------
// Con ConnectionStrings:Default => PostgreSQL. Sin ella => base en memoria (solo en Development).
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("ConnectionStrings:Default es obligatorio fuera de Development");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseInMemoryDatabase("RegresaACasa");
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

// ---------- Servicios de negocio ----------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddOptions<AzureBlobOptions>()
    .Bind(builder.Configuration.GetSection(AzureBlobOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<AzureBlobOptions>, AzureBlobOptionsValidator>();
builder.Services.AddSingleton<IImageStorageService, AzureBlobImageStorageService>();

// ---------- Límites de peticiones (anti-abuso) ----------
// En memoria: se reinician al reiniciar la API. Responden 429 { success: false, message }.
var rateLimits = builder.Configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>() ?? new RateLimitOptions();
System.ComponentModel.DataAnnotations.Validator.ValidateObject(
    rateLimits, new System.ComponentModel.DataAnnotations.ValidationContext(rateLimits), validateAllProperties: true);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, ct) => new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
        new ErrorResponse("Demasiadas solicitudes, intenta de nuevo más tarde"), ct));

    // Tope global diario de fotos: protege el Storage aunque el ataque venga de muchas IPs.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
        http.Request.Path.StartsWithSegments("/api/v1/uploads")
            ? RateLimitPartition.GetFixedWindowLimiter("uploads-per-day", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimits.UploadsPerDay,
                Window = TimeSpan.FromDays(1),
            })
            : RateLimitPartition.GetNoLimiter("sin-limite"));

    options.AddPolicy(RateLimitPolicies.Uploads, http => RateLimitPartition.GetFixedWindowLimiter(
        ClientIp(http),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = rateLimits.UploadsPerMinutePerIp, Window = TimeSpan.FromMinutes(1) }));

    options.AddPolicy(RateLimitPolicies.Writes, http => RateLimitPartition.GetFixedWindowLimiter(
        ClientIp(http),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = rateLimits.WritesPerMinutePerIp, Window = TimeSpan.FromMinutes(1) }));

    static string ClientIp(HttpContext http) => http.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
});

// La app móvil no necesita CORS, pero Expo Web (navegador) sí.
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Valida la configuración antes de tocar la base de datos (lanza OptionsValidationException).
_ = app.Services.GetRequiredService<IOptions<AzureBlobOptions>>().Value;

// ---------- Inicialización de BD ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        await DbSeeder.SeedAsync(db);
    }
}

// ---------- Pipeline HTTP ----------
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    logger.LogError(error, "Error no controlado");

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new ErrorResponse("Ocurrió un error inesperado"));
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseRateLimiter();
app.MapControllers();

app.Run();

// Necesario para WebApplicationFactory<Program> en las pruebas de integración.
public partial class Program;
