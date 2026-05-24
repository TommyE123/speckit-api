using Moq;
using SpecKitApi.Clients;
using SpecKitApi.DTOs;
using SpecKitApi.Models;
using SpecKitApi.Services;

namespace SpecKitApi.Tests.Services;

public class AlbumServiceTests
{
    private static AlbumService CreateService(IJsonPlaceholderClient client) => new AlbumService(client);

    private static IReadOnlyList<AlbumDto> TwoUserAlbums() =>
    [
        new AlbumDto { Id = 1, UserId = 1, Title = "Album 1" },
        new AlbumDto { Id = 2, UserId = 1, Title = "Album 2" },
        new AlbumDto { Id = 3, UserId = 2, Title = "Album 3" },
    ];

    private static IReadOnlyList<PhotoDto> PhotosForTwoAlbums() =>
    [
        new PhotoDto { Id = 1, AlbumId = 1, Title = "Photo 1A", Url = "https://example.com/1.jpg", ThumbnailUrl = "https://example.com/t1.jpg" },
        new PhotoDto { Id = 2, AlbumId = 1, Title = "Photo 1B", Url = "https://example.com/2.jpg", ThumbnailUrl = "https://example.com/t2.jpg" },
        new PhotoDto { Id = 3, AlbumId = 2, Title = "Photo 2A", Url = "https://example.com/3.jpg", ThumbnailUrl = "https://example.com/t3.jpg" },
        new PhotoDto { Id = 4, AlbumId = 3, Title = "Photo 3A", Url = "https://example.com/4.jpg", ThumbnailUrl = "https://example.com/t4.jpg" },
    ];

    [Fact]
    public async Task GetAlbumsWithPhotosAsync_AssignsCorrectPhotosToEachAlbum()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TwoUserAlbums());
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(PhotosForTwoAlbums());
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosAsync();

        var album1 = result.Single(a => a.Album.Id == 1);
        Assert.Equal(2, album1.Photos.Count);
        Assert.All(album1.Photos, p => Assert.Equal(1, p.AlbumId));
    }

    [Fact]
    public async Task GetAlbumsWithPhotosAsync_NoCrossAlbumLeakage()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TwoUserAlbums());
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(PhotosForTwoAlbums());
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosAsync();

        var album2 = result.Single(a => a.Album.Id == 2);
        Assert.Single(album2.Photos);
        Assert.Equal(2, album2.Photos[0].AlbumId);
    }

    [Fact]
    public async Task GetAlbumsWithPhotosAsync_ReturnsEmptyPhotos_ForAlbumWithNoPhotos()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new AlbumDto { Id = 99, UserId = 1, Title = "Empty Album" }
        ]);
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new PhotoDto { Id = 1, AlbumId = 1, Title = "Photo for different album", Url = "", ThumbnailUrl = "" }
        ]);
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosAsync();

        Assert.Single(result);
        Assert.Empty(result[0].Photos);
    }

    [Fact]
    public async Task GetAlbumsWithPhotosAsync_ReturnsEmpty_WhenNoAlbums()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AlbumDto>());
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(PhotosForTwoAlbums());
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAlbumsWithPhotosAsync_MapsPhotoDtoUrl_To_PhotoImageUrl()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new AlbumDto { Id = 1, UserId = 1, Title = "Album" }
        ]);
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new PhotoDto { Id = 1, AlbumId = 1, Title = "Photo", Url = "https://example.com/full.jpg", ThumbnailUrl = "https://example.com/thumb.jpg" }
        ]);
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosAsync();

        var photo = result.Single().Photos.Single();
        Assert.Equal("https://example.com/full.jpg", photo.ImageUrl);
        Assert.Equal("https://example.com/thumb.jpg", photo.ThumbnailUrl);
    }

    [Fact]
    public async Task GetAlbumsWithPhotosAsync_MapsAllDtoFields_Correctly()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new AlbumDto { Id = 5, UserId = 3, Title = "My Album" }
        ]);
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new PhotoDto { Id = 10, AlbumId = 5, Title = "My Photo", Url = "https://example.com/img.jpg", ThumbnailUrl = "https://example.com/thumb.jpg" }
        ]);
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosAsync();

        var awp = result.Single();
        Assert.Equal(5, awp.Album.Id);
        Assert.Equal(3, awp.Album.UserId);
        Assert.Equal("My Album", awp.Album.Title);
        var photo = awp.Photos.Single();
        Assert.Equal(10, photo.Id);
        Assert.Equal(5, photo.AlbumId);
        Assert.Equal("My Photo", photo.Title);
        Assert.Equal("https://example.com/img.jpg", photo.ImageUrl);
        Assert.Equal("https://example.com/thumb.jpg", photo.ThumbnailUrl);
    }

    [Fact]
    public async Task GetAlbumsWithPhotosByUserAsync_ReturnsOnlyMatchingUsersAlbums()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TwoUserAlbums());
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(PhotosForTwoAlbums());
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosByUserAsync(userId: 1);

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal(1, a.Album.UserId));
    }

    [Fact]
    public async Task GetAlbumsWithPhotosByUserAsync_ReturnsEmpty_ForUnknownUserId()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TwoUserAlbums());
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(PhotosForTwoAlbums());
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosByUserAsync(userId: 999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAlbumsWithPhotosByUserAsync_ReturnsEmpty_WhenNoAlbumsForUser()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<AlbumDto>());
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<PhotoDto>());
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosByUserAsync(userId: 1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAlbumsWithPhotosByUserAsync_PreservesCorrectPhotosPerAlbum()
    {
        var mockClient = new Mock<IJsonPlaceholderClient>();
        mockClient.Setup(c => c.GetAlbumsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(TwoUserAlbums());
        mockClient.Setup(c => c.GetPhotosAsync(It.IsAny<CancellationToken>())).ReturnsAsync(PhotosForTwoAlbums());
        var service = CreateService(mockClient.Object);

        var result = await service.GetAlbumsWithPhotosByUserAsync(userId: 1);

        var album1 = result.Single(a => a.Album.Id == 1);
        Assert.Equal(2, album1.Photos.Count);
        var album2 = result.Single(a => a.Album.Id == 2);
        Assert.Single(album2.Photos);
    }
}
