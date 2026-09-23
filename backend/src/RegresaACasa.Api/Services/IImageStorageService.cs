using RegresaACasa.Api.Models.Dtos;

namespace RegresaACasa.Api.Services;

/// <summary>
/// Fotos en un contenedor PRIVADO de Azure Blob Storage: nadie lee ni escribe
/// sin una URL firmada (SAS) emitida por la API.
/// </summary>
public interface IImageStorageService
{
    bool IsConfigured { get; }

    /// <summary>
    /// URL SAS de solo escritura para que la app suba UNA foto directo a Azure,
    /// sin pasar el archivo por la API.
    /// </summary>
    Task<ImageUploadResponse> CreateUploadUrlAsync(string contentType, CancellationToken ct = default);

    /// <summary>
    /// true si la URL apunta a una foto de nuestro contenedor (subida con CreateUploadUrlAsync).
    /// Sin Azure configurado (desarrollo) acepta cualquier URL.
    /// </summary>
    bool IsAcceptedImageUrl(string imageUrl);

    /// <summary>
    /// Convierte la URL guardada en BD en una URL de lectura temporal. Las URLs ajenas
    /// al contenedor (datos de ejemplo) se devuelven sin cambios.
    /// </summary>
    string ToReadUrl(string imageUrl);
}
