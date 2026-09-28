using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Data;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Services;

namespace RegresaACasa.Api.Extensions;

/// <summary>
/// Registro de servicios, agrupado por tema para que Program.cs se lea como un índice.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Controladores con JSON en snake_case y errores de validación con el formato del contrato:
    /// 400 { "success": false, "message": "El campo 'zone' es requerido" }.
    /// </summary>
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                // Los errores de validación usan el nombre JSON del campo ("zone", no "Zone").
                options.ModelMetadataDetailsProviders.Add(
                    new SystemTextJsonValidationMetadataProvider(JsonNamingPolicy.SnakeCaseLower));
                options.ModelMetadataDetailsProviders.Add(new SnakeCaseDisplayNameProvider());
            })
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower)
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = context =>
                {
                    var message = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                        ?? "La solicitud no es válida";
                    return new BadRequestObjectResult(new ErrorResponse(message));
                });

        services.AddOpenApi();

        // La app móvil no necesita CORS, pero Expo Web (navegador) sí.
        services.AddCors(options =>
            options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        return services;
    }

    /// <summary>
    /// Base de datos: PostgreSQL (con reintentos ante fallas transitorias) si hay
    /// ConnectionStrings:Default; si no, base en memoria (solo permitido en Development).
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString) && !environment.IsDevelopment())
        {
            throw new InvalidOperationException("ConnectionStrings:Default es obligatorio fuera de Development");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase("RegresaACasa");
            }
            else
            {
                // Reintenta errores transitorios (base aún arrancando, corte breve de red).
                options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
            }
        });

        return services;
    }

    /// <summary>Lógica de negocio (publicaciones) y fotos en Azure Blob Storage.</summary>
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IPetService, PetService>();

        services.AddOptions<AzureBlobOptions>()
            .Bind(configuration.GetSection(AzureBlobOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AzureBlobOptions>, AzureBlobOptionsValidator>();
        services.AddSingleton<IImageStorageService, AzureBlobImageStorageService>();

        return services;
    }

    /// <summary>
    /// Límites anti-abuso por IP y un tope diario global de fotos. Viven en memoria (se
    /// reinician con la API) y responden 429 { "success": false, "message": "..." }.
    /// </summary>
    public static IServiceCollection AddRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>() ?? new RateLimitOptions();
        Validator.ValidateObject(limits, new ValidationContext(limits), validateAllProperties: true);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, ct) => new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
                new ErrorResponse("Demasiadas solicitudes, intenta de nuevo más tarde"), ct));

            // Tope global diario de fotos: protege el Storage aunque el ataque venga de muchas IPs.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                http.Request.Path.StartsWithSegments("/api/v1/uploads")
                    ? RateLimitPartition.GetFixedWindowLimiter("uploads-per-day", _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits.UploadsPerDay,
                        Window = TimeSpan.FromDays(1),
                    })
                    : RateLimitPartition.GetNoLimiter("sin-limite"));

            options.AddPolicy(RateLimitPolicies.Uploads, http => PerIpPerMinute(http, limits.UploadsPerMinutePerIp));
            options.AddPolicy(RateLimitPolicies.Writes, http => PerIpPerMinute(http, limits.WritesPerMinutePerIp));
        });

        return services;

        static RateLimitPartition<string> PerIpPerMinute(HttpContext http, int permits) =>
            RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = TimeSpan.FromMinutes(1) });
    }
}
