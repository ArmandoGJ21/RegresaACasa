using System.Data.Common;
using Microsoft.Extensions.Options;

namespace RegresaACasa.Api.Configuration;

/// <summary>
/// Reglas que dependen del entorno o del formato de la cadena de conexión.
/// Si fallan, la API no arranca y muestra el mensaje (ValidateOnStart).
/// </summary>
public class AzureBlobOptionsValidator(IHostEnvironment environment) : IValidateOptions<AzureBlobOptions>
{
    private const string DevelopmentStorage = "UseDevelopmentStorage=true";

    public ValidateOptionsResult Validate(string? name, AzureBlobOptions options)
    {
        var connectionString = options.ConnectionString?.Trim();

        if (string.IsNullOrEmpty(connectionString))
        {
            return environment.IsDevelopment()
                ? ValidateOptionsResult.Success // En desarrollo la subida responde 503.
                : ValidateOptionsResult.Fail("AzureBlob:ConnectionString es obligatorio fuera de Development");
        }

        if (connectionString.Equals(DevelopmentStorage, StringComparison.OrdinalIgnoreCase))
        {
            return environment.IsDevelopment()
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail("AzureBlob:ConnectionString no puede usar Azurite (UseDevelopmentStorage) fuera de Development");
        }

        var parts = new DbConnectionStringBuilder();
        try
        {
            parts.ConnectionString = connectionString;
        }
        catch (ArgumentException)
        {
            return ValidateOptionsResult.Fail("AzureBlob:ConnectionString no tiene un formato válido (Clave=Valor;...)");
        }

        if (!HasValue(parts, "AccountName") || !HasValue(parts, "AccountKey"))
        {
            return ValidateOptionsResult.Fail(
                "AzureBlob:ConnectionString debe incluir AccountName y AccountKey para firmar URLs SAS");
        }

        if (!environment.IsDevelopment()
            && parts.TryGetValue("DefaultEndpointsProtocol", out var protocol)
            && !"https".Equals(protocol?.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail("AzureBlob:ConnectionString debe usar DefaultEndpointsProtocol=https fuera de Development");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool HasValue(DbConnectionStringBuilder parts, string key) =>
        parts.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value?.ToString());
}
