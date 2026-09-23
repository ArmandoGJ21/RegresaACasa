using System.ComponentModel.DataAnnotations;

namespace RegresaACasa.Api.Models.Dtos;

// Los nombres se serializan en snake_case (pet_type, image_url, ...) por la
// política global configurada en Program.cs.

public record PetResponse(
    string Id,
    string PetType,
    string? Name,
    string? Breed,
    string ColorDescription,
    string Zone,
    string ContactInfo,
    string ImageUrl);

public class CreatePetRequest
{
    [Required(ErrorMessage = RequiredMessage)]
    [StringLength(30)]
    public string? PetType { get; set; }

    [StringLength(60)]
    public string? Name { get; set; }

    [StringLength(60)]
    public string? Breed { get; set; }

    [Required(ErrorMessage = RequiredMessage)]
    [StringLength(200)]
    public string? ColorDescription { get; set; }

    [Required(ErrorMessage = RequiredMessage)]
    [StringLength(80)]
    public string? Zone { get; set; }

    [Required(ErrorMessage = RequiredMessage)]
    [StringLength(80)]
    public string? ContactInfo { get; set; }

    [Required(ErrorMessage = RequiredMessage)]
    [Url(ErrorMessage = "El campo '{0}' debe ser una URL válida")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    internal const string RequiredMessage = "El campo '{0}' es requerido";
}
