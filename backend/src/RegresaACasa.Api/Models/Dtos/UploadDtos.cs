using System.ComponentModel.DataAnnotations;

namespace RegresaACasa.Api.Models.Dtos;

public class CreateImageUploadRequest
{
    [Required(ErrorMessage = CreatePetRequest.RequiredMessage)]
    [RegularExpression("^image/(jpeg|png|webp)$", ErrorMessage = "El campo '{0}' debe ser image/jpeg, image/png o image/webp")]
    public string? ContentType { get; set; }
}

public record ImageUploadResponse(string UploadUrl, string ImageUrl, DateTimeOffset ExpiresAt);
