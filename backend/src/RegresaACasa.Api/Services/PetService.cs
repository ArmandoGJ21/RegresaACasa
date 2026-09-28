using Microsoft.EntityFrameworkCore;
using RegresaACasa.Api.Data;
using RegresaACasa.Api.Models;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Models.Entities;

namespace RegresaACasa.Api.Services;

public class PetService(AppDbContext db, IImageStorageService images, TimeProvider clock) : IPetService
{
    /// <summary>Máximo de publicaciones que devuelve el muro.</summary>
    public const int FeedSize = 100;

    public const string ForeignImageError =
        "El campo 'image_url' debe ser una foto subida con /api/v1/uploads/images";

    public async Task<IReadOnlyList<PetResponse>> GetRecentAsync(CancellationToken ct = default)
    {
        var pets = await db.Pets
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(FeedSize)
            .ToListAsync(ct);

        return pets.Select(p => p.ToResponse(images.ToReadUrl)).ToList();
    }

    public async Task<Result<PetResponse>> CreateAsync(CreatePetRequest request, CancellationToken ct = default)
    {
        // Regla de negocio: solo fotos subidas con /api/v1/uploads/images (evita enlaces externos).
        var imageUrl = request.ImageUrl!.Trim();
        if (!images.IsAcceptedImageUrl(imageUrl))
        {
            return Result<PetResponse>.Failure(ForeignImageError);
        }

        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            PetType = request.PetType!.Trim(),
            Name = request.Name?.Trim(),
            Breed = request.Breed?.Trim(),
            ColorDescription = request.ColorDescription!.Trim(),
            Zone = request.Zone!.Trim(),
            ContactInfo = request.ContactInfo!.Trim(),
            ImageUrl = imageUrl,
            CreatedAt = clock.GetUtcNow().UtcDateTime,
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);
        return Result<PetResponse>.Success(pet.ToResponse(images.ToReadUrl));
    }
}
