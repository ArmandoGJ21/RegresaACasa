using Microsoft.EntityFrameworkCore;
using RegresaACasa.Api.Models.Entities;

namespace RegresaACasa.Api.Data;

/// <summary>
/// Carga los datos de ejemplo de mock/db.json cuando la base está vacía (solo Development).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Pets.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        db.Pets.AddRange(
            new Pet
            {
                Id = Guid.NewGuid(),
                PetType = "Perro",
                Name = "Max",
                Breed = "Labrador",
                ColorDescription = "Miel con mancha blanca",
                Zone = "Centro",
                ContactInfo = "4491234567",
                ImageUrl = "https://storage.azure.com/foto.jpg",
                CreatedAt = now.AddHours(-1),
                Comments =
                [
                    new Comment
                    {
                        CommentId = Guid.NewGuid(),
                        UserName = "Ana",
                        Text = "Lo vi cerca del parque",
                        CreatedAt = now.AddMinutes(-30),
                    },
                ],
            },
            new Pet
            {
                Id = Guid.NewGuid(),
                PetType = "Gato",
                Name = "Luna",
                Breed = "Siames",
                ColorDescription = "Blanco con puntos grises",
                Zone = "Pulgas",
                ContactInfo = "4497654321",
                ImageUrl = "https://storage.azure.com/luna.jpg",
                CreatedAt = now.AddHours(-2),
            });

        await db.SaveChangesAsync();
    }
}
