using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using RegresaACasa.Api.Configuration;

namespace RegresaACasa.Api.Tests;

/// <summary>Restricciones de las variables AzureBlob__* (ver backend/.env.example).</summary>
public class AzureBlobOptionsValidatorTests
{
    private const string RealAccount =
        "DefaultEndpointsProtocol=https;AccountName=regresaacasa;AccountKey=abc123==;EndpointSuffix=core.windows.net";

    private static bool IsValid(string? connectionString, string environment) =>
        new AzureBlobOptionsValidator(new FakeEnvironment(environment))
            .Validate(null, new AzureBlobOptions { ConnectionString = connectionString })
            .Succeeded;

    [Theory]
    [InlineData(null, "Development", true)]
    [InlineData(null, "Production", false)]
    [InlineData("UseDevelopmentStorage=true", "Development", true)]
    [InlineData("UseDevelopmentStorage=true", "Production", false)]
    [InlineData(RealAccount, "Production", true)]
    [InlineData("DefaultEndpointsProtocol=https;AccountName=regresaacasa", "Development", false)]
    [InlineData("DefaultEndpointsProtocol=http;AccountName=a;AccountKey=b", "Production", false)]
    [InlineData("esto no es una cadena", "Development", false)]
    public void ConnectionString_Rules(string? connectionString, string environment, bool expected)
    {
        Assert.Equal(expected, IsValid(connectionString, environment));
    }

    [Theory]
    [InlineData("pet-images", true)]
    [InlineData("abc", true)]
    [InlineData("ab", false)]
    [InlineData("Pet-Images", false)]
    [InlineData("pet--images", false)]
    [InlineData("-pet-images", false)]
    [InlineData("pet_images", false)]
    public void ContainerName_FollowsAzureNamingRules(string containerName, bool expected)
    {
        var options = new AzureBlobOptions { ContainerName = containerName };
        Assert.Equal(expected, Validator.TryValidateObject(options, new ValidationContext(options), null, true));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(60, true)]
    [InlineData(61, false)]
    public void SasExpiryMinutes_Between1And60(int minutes, bool expected)
    {
        var options = new AzureBlobOptions { SasExpiryMinutes = minutes };
        Assert.Equal(expected, Validator.TryValidateObject(options, new ValidationContext(options), null, true));
    }

    private sealed class FakeEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "RegresaACasa.Api";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
