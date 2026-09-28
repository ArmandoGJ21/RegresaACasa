using Microsoft.EntityFrameworkCore;
using RegresaACasa.Api.Data;
using RegresaACasa.Api.Models.Dtos;
using RegresaACasa.Api.Services;

namespace RegresaACasa.Api.Tests;

/// <summary>
/// Reglas de negocio de PetService con una base en memoria y un almacenamiento falso
/// (acepta solo URLs que empiezan con https://fotos/ y las "firma" con ?firmada)
/// y un reloj que avanza un minuto en cada lectura.
/// </summary>
public class PetServiceTests
{
    private static (PetService Service, AppDbContext Db) Create()
    {
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        return (new PetService(db, new FakeImageStorage(), new SteppingClock()), db);
    }

    private static CreatePetRequest Request(string imageUrl) => new()
    {
        PetType = " Perro ",
        ColorDescription = "Miel",
        Zone = "Centro",
        ContactInfo = "4491234567",
        ImageUrl = imageUrl,
    };

    [Fact]
    public async Task CreateAsync_OwnImage_SavesTrimmedPetAndReturnsSignedUrl()
    {
        var (service, db) = Create();

        var result = await service.CreateAsync(Request("https://fotos/abc.jpg"));

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(db.Pets);
        Assert.Equal("Perro", saved.PetType);
        Assert.Equal("https://fotos/abc.jpg", saved.ImageUrl);
        Assert.Equal("https://fotos/abc.jpg?firmada", result.Value!.ImageUrl);
    }

    [Fact]
    public async Task CreateAsync_ForeignImage_FailsWithoutSaving()
    {
        var (service, db) = Create();

        var result = await service.CreateAsync(Request("https://otro-sitio.com/perro.jpg"));

        Assert.False(result.IsSuccess);
        Assert.Equal(PetService.ForeignImageError, result.Error);
        Assert.Empty(db.Pets);
    }

    [Fact]
    public async Task GetRecentAsync_NewestFirst()
    {
        var (service, _) = Create();
        await service.CreateAsync(Request("https://fotos/1.jpg"));
        await service.CreateAsync(Request("https://fotos/2.jpg"));

        var feed = await service.GetRecentAsync();

        Assert.Equal(["https://fotos/2.jpg?firmada", "https://fotos/1.jpg?firmada"], feed.Select(p => p.ImageUrl));
    }

    /// <summary>Cada lectura avanza un minuto: dos publicaciones nunca tienen la misma hora.</summary>
    private sealed class SteppingClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 9, 27, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now = _now.AddMinutes(1);
    }

    private sealed class FakeImageStorage : IImageStorageService
    {
        public bool IsConfigured => true;

        public Task<ImageUploadResponse> CreateUploadUrlAsync(string contentType, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public bool IsAcceptedImageUrl(string imageUrl) => imageUrl.StartsWith("https://fotos/", StringComparison.Ordinal);

        public string ToReadUrl(string imageUrl) => imageUrl + "?firmada";
    }
}
