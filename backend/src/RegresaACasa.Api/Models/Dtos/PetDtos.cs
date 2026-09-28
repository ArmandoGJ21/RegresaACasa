using System.ComponentModel.DataAnnotations;

namespace RegresaACasa.Api.Models.Dtos;

// Los nombres se serializan en snake_case (pet_type, image_url, ...) por la
// política configurada en Extensions/ServiceCollectionExtensions.cs (AddApiControllers).

/// <summary>Publicación tal como la ve la app (salida de GET y POST /api/v1/pets).</summary>

public record PetResponse(
    string Id,
    string PetType,
    string? Name,
    string? Breed,
    string ColorDescription,
    string Zone,
    string ContactInfo,
    string ImageUrl);

/// <summary>Cuestionario de reporte (entrada de POST /api/v1/pets). Las reglas se validan antes de llegar al controlador.</summary>
public class CreatePetRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    [StringLength(30)]
    public string? PetType { get; set; }

    [StringLength(60)]
    public string? Name { get; set; }

    [StringLength(60)]
    public string? Breed { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [StringLength(200)]
    public string? ColorDescription { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [StringLength(80)]
    public string? Zone { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [StringLength(80)]
    public string? ContactInfo { get; set; }

    [Required(ErrorMessage = ValidationMessages.Required)]
    [Url(ErrorMessage = "El campo '{0}' debe ser una URL válida")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }
}
