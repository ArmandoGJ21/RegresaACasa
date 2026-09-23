namespace RegresaACasa.Api.Models.Entities;

/// <summary>
/// Publicación de una mascota perdida (tabla PETS).
/// </summary>
public class Pet
{
    public Guid Id { get; set; }
    public string PetType { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Breed { get; set; }
    public string ColorDescription { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
