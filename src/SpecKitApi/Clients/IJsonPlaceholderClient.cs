using SpecKitApi.DTOs;

namespace SpecKitApi.Clients;

/// <summary>
/// Defines the contract for fetching data from the JSONPlaceholder external API.
/// </summary>
public interface IJsonPlaceholderClient
{
    /// <summary>
    /// Fetches all albums from the JSONPlaceholder API.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="AlbumDto"/> instances.</returns>
    Task<IReadOnlyList<AlbumDto>> GetAlbumsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all photos from the JSONPlaceholder API.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="PhotoDto"/> instances.</returns>
    Task<IReadOnlyList<PhotoDto>> GetPhotosAsync(CancellationToken cancellationToken = default);
}
