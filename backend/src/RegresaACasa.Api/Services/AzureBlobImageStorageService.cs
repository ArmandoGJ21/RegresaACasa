using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Models.Dtos;

namespace RegresaACasa.Api.Services;

public partial class AzureBlobImageStorageService(IOptions<AzureBlobOptions> options, TimeProvider clock) : IImageStorageService
{
    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
    };

    private readonly AzureBlobOptions _options = options.Value;
    private BlobContainerClient? _container;
    private bool _containerReady;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ConnectionString);

    private BlobContainerClient Container =>
        _container ??= new BlobContainerClient(_options.ConnectionString, _options.ContainerName);

    public async Task<ImageUploadResponse> CreateUploadUrlAsync(string contentType, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Azure Blob Storage no está configurado.");
        }

        if (!_containerReady)
        {
            // Sin parámetro de acceso público => contenedor privado.
            await Container.CreateIfNotExistsAsync(cancellationToken: ct);
            _containerReady = true;
        }

        var blob = Container.GetBlobClient($"{Guid.NewGuid():N}{Extensions[contentType]}");
        var expiresAt = clock.GetUtcNow().AddMinutes(_options.SasExpiryMinutes);

        var sas = new BlobSasBuilder(BlobSasPermissions.Create | BlobSasPermissions.Write, expiresAt)
        {
            BlobContainerName = Container.Name,
            BlobName = blob.Name,
            Resource = "b",
            ContentType = contentType,
        };

        return new ImageUploadResponse(blob.GenerateSasUri(sas).ToString(), blob.Uri.ToString(), expiresAt);
    }

    public bool IsAcceptedImageUrl(string imageUrl) => !IsConfigured || TryGetOwnBlobName(imageUrl, out _);

    public string ToReadUrl(string imageUrl)
    {
        if (!IsConfigured || !TryGetOwnBlobName(imageUrl, out var blobName))
        {
            return imageUrl;
        }

        // Generar la SAS es un cálculo local (sin llamadas a Azure), así que no hace lento el muro.
        var blob = Container.GetBlobClient(blobName);
        var sas = new BlobSasBuilder(BlobSasPermissions.Read, clock.GetUtcNow().AddMinutes(_options.ReadSasMinutes))
        {
            BlobContainerName = Container.Name,
            BlobName = blobName,
            Resource = "b",
        };
        return blob.GenerateSasUri(sas).ToString();
    }

    private bool TryGetOwnBlobName(string imageUrl, out string blobName)
    {
        blobName = string.Empty;
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) || !string.IsNullOrEmpty(uri.Query))
        {
            return false;
        }

        var containerUri = Container.Uri;
        var prefix = containerUri.AbsolutePath.TrimEnd('/') + "/";
        if (!Uri.Compare(uri, containerUri, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase).Equals(0)
            || !uri.AbsolutePath.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var name = uri.AbsolutePath[prefix.Length..];
        if (!GeneratedBlobName().IsMatch(name))
        {
            return false;
        }

        blobName = name;
        return true;
    }

    // Solo nombres generados por CreateUploadUrlAsync: <guid sin guiones>.<ext>
    [GeneratedRegex("^[0-9a-f]{32}\\.(jpg|png|webp)$")]
    private static partial Regex GeneratedBlobName();
}
