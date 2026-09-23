using RegresaACasa.Api.Models.Dtos;

namespace RegresaACasa.Api.Services;

public interface IImageUploadService
{
    bool IsConfigured { get; }

    /// <summary>
    /// Genera una URL firmada (SAS) de solo escritura para que la app suba la foto
    /// directo a Azure Blob Storage, sin pasar el archivo por la API.
    /// </summary>
    Task<ImageUploadResponse> CreateUploadUrlAsync(string contentType, CancellationToken ct = default);
}
