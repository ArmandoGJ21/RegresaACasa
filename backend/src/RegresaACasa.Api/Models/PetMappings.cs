using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Models.Entities;

namespace RegresaACasa.Api.Models;

public static class PetMappings
{
    public static PetResponse ToResponse(this Pet pet) => new(
        pet.Id.ToString(),
        pet.PetType,
        pet.Name,
        pet.Breed,
        pet.ColorDescription,
        pet.Zone,
        pet.ContactInfo,
        pet.ImageUrl,
        pet.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => c.ToResponse())
            .ToList());

    public static CommentResponse ToResponse(this Comment comment) =>
        new(comment.UserName, comment.Text, comment.CreatedAt);
}
