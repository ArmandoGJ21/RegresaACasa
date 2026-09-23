namespace RegresaACasa.Api.Models.Entities;

/// <summary>
/// Comentario sobre una publicación (tabla COMMENTS).
/// El <see cref="CommentId"/> existe en BD pero no se expone en la API.
/// </summary>
public class Comment
{
    public Guid CommentId { get; set; }
    public Guid PetId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Pet? Pet { get; set; }
}
