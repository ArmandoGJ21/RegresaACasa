using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Models.Dtos;

namespace RegresaACasa.Api.Services;

public class AzureBlobImageUploadService(IOptions<AzureBlobOptions> options, TimeProvider clock) : IImageUploadService
{
    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
    };

    private readonly AzureBlobOptions _options = options.Value;
    private bool _containerReady;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ConnectionString);

    public async Task<ImageUploadResponse> CreateUploadUrlAsync(string contentType, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Azure Blob Storage no está configurado.");
        }

        var container = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
        if (!_containerReady)
        {
            // En Azure el contenedor ya lo crea infra/storage.bicep; esto cubre Azurite.
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
            _containerReady = true;
        }

        var blobName = $"{Guid.NewGuid():N}{Extensions[contentType]}";
        var blob = container.GetBlobClient(blobName);
        var expiresAt = clock.GetUtcNow().AddMinutes(_options.SasExpiryMinutes);

        var sas = new BlobSasBuilder
        {
            BlobContainerName = container.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = expiresAt,
            ContentType = contentType,
        };
        sas.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        var uploadUri = blob.GenerateSasUri(sas);
        return new ImageUploadResponse(uploadUri.ToString(), blob.Uri.ToString(), expiresAt);
    }
}
