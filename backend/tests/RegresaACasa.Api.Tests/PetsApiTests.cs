using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RegresaACasa.Api.Tests;

/// <summary>
/// Verifica que la API cumple el contrato de docs/api-contract.md
/// (rutas, snake_case y formato de errores). Corre con la base en memoria.
/// </summary>
public class PetsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static object ValidPet(string zone = "Centro") => new
    {
        pet_type = "Perro",
        name = "Max",
        breed = "Labrador",
        color_description = "Miel con mancha blanca",
        zone,
        contact_info = "4491234567",
        image_url = "https://storage.azure.com/foto.jpg",
    };

    [Fact]
    public async Task GetPets_ReturnsListInSnakeCase()
    {
        var response = await _client.GetAsync("/api/v1/pets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pets = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, pets.ValueKind);
        var first = pets.EnumerateArray().First();
        Assert.True(first.TryGetProperty("pet_type", out _));
        Assert.True(first.TryGetProperty("color_description", out _));
        Assert.True(first.TryGetProperty("image_url", out _));
        Assert.False(first.TryGetProperty("comments", out _));
    }

    [Fact]
    public async Task CreatePet_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/pets", ValidPet());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var pet = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrEmpty(pet.GetProperty("id").GetString()));
        Assert.Equal("Perro", pet.GetProperty("pet_type").GetString());
        Assert.Equal("4491234567", pet.GetProperty("contact_info").GetString());
    }

    [Fact]
    public async Task CreatePet_WithoutZone_Returns400ContractError()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/pets", ValidPet(zone: null!));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(error.GetProperty("success").GetBoolean());
        Assert.Equal("El campo 'zone' es requerido", error.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ImageUpload_WithoutAzureConfigured_Returns503()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/uploads/images", new { content_type = "image/jpeg" });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
