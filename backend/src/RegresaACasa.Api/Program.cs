using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Extensions;

// 1. Variables de backend/.env (ver .env.example). Las del sistema tienen prioridad.
DotEnvFile.Load(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);

// 2. Servicios (ver Extensions/ServiceCollectionExtensions.cs).
builder.Services
    .AddApiControllers()
    .AddDatabase(builder.Configuration, builder.Environment)
    .AddAppServices(builder.Configuration)
    .AddRateLimits(builder.Configuration);

var app = builder.Build();

// 3. Validar configuración, migrar la BD y cargar datos de ejemplo (Development).
await app.InitializeDatabaseAsync();

// 4. Pipeline HTTP: errores, documentación, CORS, límites y controladores.
app.UseApiErrorHandler();

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
