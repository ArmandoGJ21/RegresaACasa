using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Models.Entities;

namespace RegresaACasa.Api.Models;

public static class PetMappings
{
    /// <param name="toImageUrl">Transforma la image_url guardada en la que ve el cliente (URL firmada).</param>
    public static PetResponse ToResponse(this Pet pet, Func<string, string> toImageUrl) => new(
        pet.Id.ToString(),
        pet.PetType,
        pet.Name,
        pet.Breed,
        pet.ColorDescription,
        pet.Zone,
        pet.ContactInfo,
        toImageUrl(pet.ImageUrl));
}
