using RegresaACasa.Api.Models.Dtos;

namespace RegresaACasa.Api.Services;

public interface IPetService
{
    Task<IReadOnlyList<PetResponse>> GetRecentAsync(CancellationToken ct = default);

    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct = default);

    /// <returns>El comentario creado, o <c>null</c> si la publicación no existe.</returns>
    Task<CommentResponse?> AddCommentAsync(Guid petId, CreateCommentRequest request, CancellationToken ct = default);
}
