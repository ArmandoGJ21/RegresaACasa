using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace RegresaACasa.Api.Tests;

/// <summary>Los límites anti-abuso responden 429 con el formato de error del contrato.</summary>
public class RateLimitTests
{
    private sealed class LowLimitsFactory : ApiFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseSetting("RateLimits:UploadsPerMinutePerIp", "2");
            builder.UseSetting("RateLimits:WritesPerMinutePerIp", "1");
        }
    }

    [Fact]
    public async Task Uploads_OverLimit_Returns429()
    {
        using var factory = new LowLimitsFactory();
        var client = factory.CreateClient();

        for (var i = 0; i < 2; i++)
        {
            var allowed = await client.PostAsJsonAsync("/api/v1/uploads/images", new { content_type = "image/jpeg" });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, allowed.StatusCode);
        }

        var rejected = await client.PostAsJsonAsync("/api/v1/uploads/images", new { content_type = "image/jpeg" });

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        var error = await rejected.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(error.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Comments_OverLimit_Returns429()
    {
        using var factory = new LowLimitsFactory();
        var client = factory.CreateClient();
        var comment = new { user_name = "Ana", text = "Hola" };

        await client.PostAsJsonAsync("/api/v1/pets/123/comments", comment);
        var rejected = await client.PostAsJsonAsync("/api/v1/pets/123/comments", comment);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }
}
