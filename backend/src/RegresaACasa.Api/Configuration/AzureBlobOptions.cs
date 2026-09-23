using System.ComponentModel.DataAnnotations;

namespace RegresaACasa.Api.Configuration;

/// <summary>
/// Sección "AzureBlob" (variables AzureBlob__*). Las reglas se validan al arrancar
/// en <see cref="AzureBlobOptionsValidator"/>.
/// </summary>
public class AzureBlobOptions
{
    public const string SectionName = "AzureBlob";

    /// <summary>
    /// Cadena de conexión con AccountName y AccountKey (necesaria para firmar URLs SAS),
    /// o "UseDevelopmentStorage=true" para Azurite. Obligatoria fuera de Development.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>Reglas de Azure: 3-63 caracteres, minúsculas, números y guiones, sin "--" ni guion al inicio/fin.</summary>
    [RegularExpression(
        "^(?!.*--)[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$",
        ErrorMessage = "AzureBlob:ContainerName debe tener 3-63 caracteres (minúsculas, números y guiones; sin '--' ni guion al inicio o final)")]
    public string ContainerName { get; set; } = "pet-images";

    /// <summary>Vigencia de la URL de subida. Corta a propósito: solo sirve para una foto.</summary>
    [Range(1, 60, ErrorMessage = "AzureBlob:SasExpiryMinutes debe estar entre 1 y 60")]
    public int SasExpiryMinutes { get; set; } = 10;

    /// <summary>Vigencia de las URLs de lectura que devuelve el muro (el contenedor es privado).</summary>
    [Range(5, 1440, ErrorMessage = "AzureBlob:ReadSasMinutes debe estar entre 5 y 1440")]
    public int ReadSasMinutes { get; set; } = 60;
}
