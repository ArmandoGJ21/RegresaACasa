using Microsoft.EntityFrameworkCore;
using RegresaACasa.Api.Data;
using RegresaACasa.Api.Models;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Models.Entities;

namespace RegresaACasa.Api.Services;

public class PetService(AppDbContext db, TimeProvider clock) : IPetService
{
    private const int FeedSize = 100;

    public async Task<IReadOnlyList<PetResponse>> GetRecentAsync(CancellationToken ct = default)
    {
        var pets = await db.Pets
            .AsNoTracking()
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt)
            .Take(FeedSize)
            .ToListAsync(ct);

        return pets.Select(p => p.ToResponse()).ToList();
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct = default)
    {
        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            PetType = request.PetType!.Trim(),
            Name = request.Name?.Trim(),
            Breed = request.Breed?.Trim(),
            ColorDescription = request.ColorDescription!.Trim(),
            Zone = request.Zone!.Trim(),
            ContactInfo = request.ContactInfo!.Trim(),
            ImageUrl = request.ImageUrl!.Trim(),
            CreatedAt = clock.GetUtcNow().UtcDateTime,
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);
        return pet.ToResponse();
    }

    public async Task<CommentResponse?> AddCommentAsync(Guid petId, CreateCommentRequest request, CancellationToken ct = default)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
        {
            return null;
        }

        var comment = new Comment
        {
            CommentId = Guid.NewGuid(),
            PetId = petId,
            UserName = request.UserName!.Trim(),
            Text = request.Text!.Trim(),
            CreatedAt = clock.GetUtcNow().UtcDateTime,
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);
        return comment.ToResponse();
    }
}
