namespace RegresaACasa.Api.Configuration;

public class AzureBlobOptions
{
    public const string SectionName = "AzureBlob";

    /// <summary>Cadena de conexión de la cuenta de almacenamiento (o "UseDevelopmentStorage=true" para Azurite).</summary>
    public string? ConnectionString { get; set; }

    public string ContainerName { get; set; } = "pet-images";

    public int SasExpiryMinutes { get; set; } = 10;
}
