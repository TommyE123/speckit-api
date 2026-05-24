using SpecKitApi.Models;

namespace SpecKitApi.Services;

/// <summary>
/// Defines the contract for the album service that combines albums with their photos.
/// </summary>
public interface IAlbumService
{
    /// <summary>
    /// Retrieves all albums combined with their associated photos.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="AlbumWithPhotos"/> instances.</returns>
    Task<IReadOnlyList<AlbumWithPhotos>> GetAlbumsWithPhotosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves albums with their associated photos filtered by user ID.
    /// Returns an empty list if no albums exist for the specified user.
    /// </summary>
    /// <param name="userId">The user ID to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of <see cref="AlbumWithPhotos"/> instances for the specified user.</returns>
    Task<IReadOnlyList<AlbumWithPhotos>> GetAlbumsWithPhotosByUserAsync(int userId, CancellationToken cancellationToken = default);
}
