#:package Azure.Storage.Blobs@12.29.2

// Configura CORS en la cuenta de Azure Blob Storage para que la versión WEB de la app
// pueda subir fotos con la URL firmada. La app nativa (celular) no lo necesita.
//
// Uso (desde la raíz del repo):
//   dotnet run infra/configure-cors.cs
//   dotnet run infra/configure-cors.cs http://localhost:8081 https://mi-dominio.com
//
// Lee AzureBlob__ConnectionString de la variable de entorno o de backend/.env.
// Conserva las reglas CORS que ya existan para otros orígenes.

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

string[] origins = args.Length > 0 ? args : ["http://localhost:8081"];

var connectionString = Environment.GetEnvironmentVariable("AzureBlob__ConnectionString") ?? ReadFromEnvFile("backend/.env");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("UseDevelopmentStorage", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Falta AzureBlob__ConnectionString de Azure real en backend/.env.");
    return 1;
}

var service = new BlobServiceClient(connectionString);
var properties = (await service.GetPropertiesAsync()).Value;
var allowedOrigins = string.Join(",", origins);

var rules = properties.Cors?.Where(r => r.AllowedOrigins != allowedOrigins).ToList() ?? [];
rules.Add(new BlobCorsRule
{
    AllowedOrigins = allowedOrigins,
    AllowedMethods = "GET,PUT,OPTIONS",
    AllowedHeaders = "content-type,x-ms-*",
    ExposedHeaders = "etag",
    MaxAgeInSeconds = 3600,
});
properties.Cors = rules;

await service.SetPropertiesAsync(properties);
Console.WriteLine($"CORS configurado en '{service.AccountName}' para: {allowedOrigins}");
return 0;

static string? ReadFromEnvFile(string path)
{
    if (!File.Exists(path))
    {
        return null;
    }

    const string key = "AzureBlob__ConnectionString=";
    return File.ReadLines(path)
        .Select(line => line.Trim())
        .FirstOrDefault(line => line.StartsWith(key, StringComparison.Ordinal))?[key.Length..]
        .Trim()
        .Trim('"');
}
