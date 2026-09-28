using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Services;

namespace RegresaACasa.Api.Controllers;

/// <summary>
/// Muro de publicaciones y cuestionario de reporte. Solo traduce HTTP ⇄ PetService.
/// </summary>
[ApiController]
[Route("api/v1/pets")]
[Produces("application/json")]
public class PetsController(IPetService pets) : ControllerBase
{
    /// <summary>GET /api/v1/pets — las publicaciones más recientes.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PetResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PetResponse>>> GetAll(CancellationToken ct)
    {
        return Ok(await pets.GetRecentAsync(ct));
    }

    /// <summary>POST /api/v1/pets — crea una publicación; la foto ya debe estar subida (image_url).</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [ProducesResponseType<PetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PetResponse>> Create(CreatePetRequest request, CancellationToken ct)
    {
        var result = await pets.CreateAsync(request, ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : BadRequest(new ErrorResponse(result.Error!));
    }
}
