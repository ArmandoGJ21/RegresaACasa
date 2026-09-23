using Microsoft.AspNetCore.Mvc;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Services;

namespace RegresaACasa.Api.Controllers;

/// <summary>
/// Entrega URLs firmadas (SAS) para que la app suba la foto directo a Azure Blob Storage
/// (paso 1 del flujo). La app nunca recibe la llave de la cuenta de almacenamiento.
/// </summary>
[ApiController]
[Route("api/v1/uploads")]
[Produces("application/json")]
public class UploadsController(IImageUploadService uploads) : ControllerBase
{
    [HttpPost("images")]
    [ProducesResponseType<ImageUploadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ImageUploadResponse>> CreateImageUpload(CreateImageUploadRequest request, CancellationToken ct)
    {
        if (!uploads.IsConfigured)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ErrorResponse("El almacenamiento de imágenes no está configurado"));
        }

        return Ok(await uploads.CreateUploadUrlAsync(request.ContentType!, ct));
    }
}
