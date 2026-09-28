using RegresaACasa.Api.Models;
using RegresaACasa.Api.Models.Dtos;

namespace RegresaACasa.Api.Services;

/// <summary>Lógica de negocio de las publicaciones (muro y cuestionario).</summary>
public interface IPetService
{
    /// <summary>Las publicaciones más recientes, con la foto lista para verse (URL firmada).</summary>
    Task<IReadOnlyList<PetResponse>> GetRecentAsync(CancellationToken ct = default);

    /// <summary>Guarda una publicación. Falla si la foto no se subió a nuestro almacenamiento.</summary>
    Task<Result<PetResponse>> CreateAsync(CreatePetRequest request, CancellationToken ct = default);
}
