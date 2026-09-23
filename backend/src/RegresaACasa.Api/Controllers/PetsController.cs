using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Services;

namespace RegresaACasa.Api.Controllers;

/// <summary>
/// Controlador del muro de publicaciones y del cuestionario de reporte.
/// </summary>
[ApiController]
[Route("api/v1/pets")]
[Produces("application/json")]
public class PetsController(IPetService pets, IImageStorageService images) : ControllerBase
{
    /// <summary>Lista las publicaciones más recientes.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PetResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PetResponse>>> GetAll(CancellationToken ct)
    {
        return Ok(await pets.GetRecentAsync(ct));
    }

    /// <summary>Crea una publicación. La foto ya debe estar subida a Azure (image_url).</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [ProducesResponseType<PetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PetResponse>> Create(CreatePetRequest request, CancellationToken ct)
    {
        if (!images.IsAcceptedImageUrl(request.ImageUrl!))
        {
            return BadRequest(new ErrorResponse("El campo 'image_url' debe ser una foto subida con /api/v1/uploads/images"));
        }

        var pet = await pets.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, pet);
    }
}
