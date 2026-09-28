using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Data;
using RegresaACasa.Api.Models.Dtos;

namespace RegresaACasa.Api.Extensions;

/// <summary>Pasos de arranque y del pipeline HTTP que usa Program.cs.</summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Valida la configuración de Azure (antes de tocar la BD), aplica las migraciones en
    /// PostgreSQL y, en Development, carga datos de ejemplo si la base está vacía.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        _ = app.Services.GetRequiredService<IOptions<AzureBlobOptions>>().Value;

        using var scope = app.Services.CreateScope();
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

    /// <summary>Excepciones no controladas => 500 { "success": false, "message": "Ocurrió un error inesperado" }.</summary>
    public static IApplicationBuilder UseApiErrorHandler(this IApplicationBuilder app) =>
        app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Features.Get<IExceptionHandlerFeature>()?.Error, "Error no controlado");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ErrorResponse("Ocurrió un error inesperado"));
        }));
}
