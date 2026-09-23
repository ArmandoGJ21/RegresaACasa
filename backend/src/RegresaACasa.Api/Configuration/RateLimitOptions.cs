using System.ComponentModel.DataAnnotations;

namespace RegresaACasa.Api.Configuration;

/// <summary>
/// Sección "RateLimits" (variables RateLimits__*). Frena que alguien llene el Storage
/// o la BD con peticiones automáticas. Al superarse la API responde 429.
/// </summary>
public class RateLimitOptions
{
    public const string SectionName = "RateLimits";

    /// <summary>URLs de subida de fotos por IP por minuto.</summary>
    [Range(1, 1000)]
    public int UploadsPerMinutePerIp { get; set; } = 5;

    /// <summary>Tope GLOBAL de fotos por día (todas las IPs). Protege el Storage aunque el ataque venga de muchas IPs.</summary>
    [Range(1, 100_000)]
    public int UploadsPerDay { get; set; } = 200;

    /// <summary>Publicaciones y comentarios por IP por minuto.</summary>
    [Range(1, 1000)]
    public int WritesPerMinutePerIp { get; set; } = 20;
}

public static class RateLimitPolicies
{
    public const string Uploads = "uploads";
    public const string Writes = "writes";
}
