using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Services;

namespace RegresaACasa.Api.Controllers;

/// <summary>
/// Entrega URLs firmadas (SAS) para que la app suba la foto directo a Azure Blob Storage
/// (paso 1 del flujo). La app nunca recibe la llave de la cuenta de almacenamiento.
/// Limitado por IP y con tope diario global para evitar que llenen el Storage.
/// </summary>
[ApiController]
[Route("api/v1/uploads")]
[Produces("application/json")]
[EnableRateLimiting(RateLimitPolicies.Uploads)]
public class UploadsController(IImageStorageService images) : ControllerBase
{
    [HttpPost("images")]
    [ProducesResponseType<ImageUploadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ImageUploadResponse>> CreateImageUpload(CreateImageUploadRequest request, CancellationToken ct)
    {
        if (!images.IsConfigured)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ErrorResponse("El almacenamiento de imágenes no está configurado"));
        }

        return Ok(await images.CreateUploadUrlAsync(request.ContentType!, ct));
    }
}
