using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Services;

namespace RegresaACasa.Api.Controllers;

/// <summary>
/// Controlador de comentarios anidados en una publicación.
/// </summary>
[ApiController]
[Route("api/v1/pets/{id}/comments")]
[Produces("application/json")]
public class CommentsController(IPetService pets) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Writes)]
    [ProducesResponseType<CommentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CommentResponse>> Create(string id, CreateCommentRequest request, CancellationToken ct)
    {
        var comment = Guid.TryParse(id, out var petId)
            ? await pets.AddCommentAsync(petId, request, ct)
            : null;

        if (comment is null)
        {
            return NotFound(new ErrorResponse("No existe una publicación con ese id"));
        }

        return StatusCode(StatusCodes.Status201Created, comment);
    }
}
