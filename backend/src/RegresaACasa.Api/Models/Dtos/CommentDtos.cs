using System.ComponentModel.DataAnnotations;

namespace RegresaACasa.Api.Models.Dtos;

public record CommentResponse(string UserName, string Text, DateTime CreatedAt);

public class CreateCommentRequest
{
    [Required(ErrorMessage = CreatePetRequest.RequiredMessage)]
    [StringLength(60)]
    public string? UserName { get; set; }

    [Required(ErrorMessage = CreatePetRequest.RequiredMessage)]
    [StringLength(500)]
    public string? Text { get; set; }
}
