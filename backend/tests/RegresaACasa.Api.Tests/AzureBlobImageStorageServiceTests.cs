using Microsoft.Extensions.Options;
using RegresaACasa.Api.Configuration;
using RegresaACasa.Api.Services;

namespace RegresaACasa.Api.Tests;

/// <summary>
/// Contenedor privado: solo se aceptan fotos propias y se leen con URL firmada.
/// Generar SAS es un cálculo local, así que no hace falta una cuenta real.
/// </summary>
public class AzureBlobImageStorageServiceTests
{
    private static readonly string FakeKey = Convert.ToBase64String(new byte[64]);
    private const string OwnBlob = "https://cuenta.blob.core.windows.net/fotos/0123456789abcdef0123456789abcdef.jpg";

    private static AzureBlobImageStorageService Create(string? connectionString = null) => new(
        Options.Create(new AzureBlobOptions
        {
            ConnectionString = connectionString
                ?? $"DefaultEndpointsProtocol=https;AccountName=cuenta;AccountKey={FakeKey};EndpointSuffix=core.windows.net",
            ContainerName = "fotos",
        }),
        TimeProvider.System);

    [Theory]
    [InlineData(OwnBlob, true)]
    [InlineData("https://otra.blob.core.windows.net/fotos/0123456789abcdef0123456789abcdef.jpg", false)]
    [InlineData("https://cuenta.blob.core.windows.net/otro/0123456789abcdef0123456789abcdef.jpg", false)]
    [InlineData("https://cuenta.blob.core.windows.net/fotos/virus.exe", false)]
    [InlineData("https://cuenta.blob.core.windows.net/fotos/sub/0123456789abcdef0123456789abcdef.jpg", false)]
    [InlineData(OwnBlob + "?sig=x", false)]
    [InlineData("https://ejemplo.com/perro.jpg", false)]
    public void IsAcceptedImageUrl_OnlyOwnGeneratedBlobs(string url, bool expected)
    {
        Assert.Equal(expected, Create().IsAcceptedImageUrl(url));
    }

    [Fact]
    public void ToReadUrl_OwnBlob_ReturnsReadOnlySas()
    {
        var url = Create().ToReadUrl(OwnBlob);

        Assert.StartsWith(OwnBlob + "?", url);
        Assert.Contains("sp=r&", url);
        Assert.Contains("sig=", url);
    }

    [Fact]
    public void ToReadUrl_ForeignUrl_Unchanged()
    {
        Assert.Equal("https://storage.azure.com/foto.jpg", Create().ToReadUrl("https://storage.azure.com/foto.jpg"));
    }

    [Fact]
    public void WithoutAzure_AcceptsAnyUrlAndDoesNotSign()
    {
        var service = new AzureBlobImageStorageService(Options.Create(new AzureBlobOptions()), TimeProvider.System);

        Assert.True(service.IsAcceptedImageUrl("https://placehold.co/600x400/png"));
        Assert.Equal(OwnBlob, service.ToReadUrl(OwnBlob));
    }
}
