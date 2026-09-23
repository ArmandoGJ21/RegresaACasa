using RegresaACasa.Api.Models.Dtos;

namespace RegresaACasa.Api.Services;

public interface IPetService
{
    Task<IReadOnlyList<PetResponse>> GetRecentAsync(CancellationToken ct = default);

    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct = default);
}
