using System.ComponentModel.DataAnnotations;

namespace RegresaACasa.Api.Models.Dtos;

/// <summary>Entrada de POST /api/v1/uploads/images: el tipo de la foto que se va a subir.</summary>
public class CreateImageUploadRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [RegularExpression("^image/(jpeg|png|webp)$", ErrorMessage = "El campo '{0}' debe ser image/jpeg, image/png o image/webp")]
    public string? ContentType { get; set; }
}

/// <summary>URL firmada para subir la foto (upload_url) y la dirección final de la foto (image_url).</summary>
public record ImageUploadResponse(string UploadUrl, string ImageUrl, DateTimeOffset ExpiresAt);
