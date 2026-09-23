using System.Text.Json.Serialization;

namespace RegresaACasa.Api.Models.Dtos;

/// <summary>
/// Formato de error del contrato: { "success": false, "message": "..." }.
/// </summary>
public record ErrorResponse(string Message)
{
    [JsonPropertyOrder(-1)]
    public bool Success => false;
}
