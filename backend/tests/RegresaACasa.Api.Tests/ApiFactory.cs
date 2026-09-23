using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RegresaACasa.Api.Tests;

/// <summary>
/// API aislada para pruebas: base en memoria y sin Azure, aunque el desarrollador
/// tenga un backend/.env con PostgreSQL o Azure reales.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", "");
        builder.UseSetting("AzureBlob:ConnectionString", "");
    }
}
