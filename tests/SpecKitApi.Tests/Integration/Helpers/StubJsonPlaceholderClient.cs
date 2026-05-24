using SpecKitApi.Clients;
using SpecKitApi.DTOs;

namespace SpecKitApi.Tests.Integration.Helpers;

public sealed class StubJsonPlaceholderClient : IJsonPlaceholderClient
{
    public bool ThrowOnCall { get; set; }
    public IReadOnlyList<AlbumDto> Albums { get; set; } = DefaultAlbums;
    public IReadOnlyList<PhotoDto> Photos { get; set; } = DefaultPhotos;

    public static readonly IReadOnlyList<AlbumDto> DefaultAlbums =
    [
        new AlbumDto { Id = 1, UserId = 1, Title = "Album 1" },
        new AlbumDto { Id = 2, UserId = 1, Title = "Album 2" },
        new AlbumDto { Id = 3, UserId = 2, Title = "Album 3" }
    ];

    public static readonly IReadOnlyList<PhotoDto> DefaultPhotos =
    [
        new PhotoDto { Id = 1, AlbumId = 1, Title = "Photo 1a", Url = "https://example.com/1a.jpg", ThumbnailUrl = "https://example.com/1a_t.jpg" },
        new PhotoDto { Id = 2, AlbumId = 1, Title = "Photo 1b", Url = "https://example.com/1b.jpg", ThumbnailUrl = "https://example.com/1b_t.jpg" },
        new PhotoDto { Id = 3, AlbumId = 2, Title = "Photo 2a", Url = "https://example.com/2a.jpg", ThumbnailUrl = "https://example.com/2a_t.jpg" },
        new PhotoDto { Id = 4, AlbumId = 2, Title = "Photo 2b", Url = "https://example.com/2b.jpg", ThumbnailUrl = "https://example.com/2b_t.jpg" },
        new PhotoDto { Id = 5, AlbumId = 3, Title = "Photo 3a", Url = "https://example.com/3a.jpg", ThumbnailUrl = "https://example.com/3a_t.jpg" },
        new PhotoDto { Id = 6, AlbumId = 3, Title = "Photo 3b", Url = "https://example.com/3b.jpg", ThumbnailUrl = "https://example.com/3b_t.jpg" }
    ];

    public Task<IReadOnlyList<AlbumDto>> GetAlbumsAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowOnCall)
            throw new HttpRequestException("Stub failure");
        return Task.FromResult(Albums);
    }

    public Task<IReadOnlyList<PhotoDto>> GetPhotosAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowOnCall)
            throw new HttpRequestException("Stub failure");
        return Task.FromResult(Photos);
    }
}
