using SpecKitApi.Clients;
using SpecKitApi.DTOs;
using SpecKitApi.Models;

namespace SpecKitApi.Services;

/// <summary>
/// Service that combines album and photo data from the JSONPlaceholder API.
/// </summary>
public sealed class AlbumService : IAlbumService
{
    private readonly IJsonPlaceholderClient _client;

    /// <summary>
    /// Initializes a new instance of <see cref="AlbumService"/>.
    /// </summary>
    /// <param name="client">The JSONPlaceholder HTTP client.</param>
    public AlbumService(IJsonPlaceholderClient client)
    {
        _client = client;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AlbumWithPhotos>> GetAlbumsWithPhotosAsync(
        CancellationToken cancellationToken = default
    )
    {
        var (albumDtos, photoDtos) = await FetchBothAsync(cancellationToken).ConfigureAwait(false);

        var photosByAlbumId = photoDtos
            .GroupBy(p => p.AlbumId)
            .ToDictionary(g => g.Key, g => g.Select(MapPhoto).ToList());

        return albumDtos
            .Select(albumDto =>
            {
                var album = MapAlbum(albumDto);
                var photos = photosByAlbumId.TryGetValue(album.Id, out var list)
                    ? (IReadOnlyList<Photo>)list
                    : Array.Empty<Photo>();
                return new AlbumWithPhotos(album, photos);
            })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AlbumWithPhotos>> GetAlbumsWithPhotosByUserAsync(
        int userId,
        CancellationToken cancellationToken = default
    )
    {
        var all = await GetAlbumsWithPhotosAsync(cancellationToken).ConfigureAwait(false);
        return all.Where(a => a.Album.UserId == userId).ToList();
    }

    private async Task<(
        IReadOnlyList<AlbumDto> Albums,
        IReadOnlyList<PhotoDto> Photos
    )> FetchBothAsync(CancellationToken cancellationToken)
    {
        var albumsTask = _client.GetAlbumsAsync(cancellationToken);
        var photosTask = _client.GetPhotosAsync(cancellationToken);
        await Task.WhenAll(albumsTask, photosTask).ConfigureAwait(false);
        return (await albumsTask, await photosTask);
    }

    private static Album MapAlbum(AlbumDto dto) => new(dto.Id, dto.UserId, dto.Title);

    private static Photo MapPhoto(PhotoDto dto) =>
        new(dto.Id, dto.AlbumId, dto.Title, dto.Url, dto.ThumbnailUrl);
}
