using SpecKitApi.Models;

namespace SpecKitApi.Tests.Models;

public class AlbumWithPhotosTests
{
    [Fact]
    public void AlbumWithPhotos_AllPhotos_ShareAlbumId_WithParentAlbum()
    {
        var album = new Album(Id: 1, UserId: 1, Title: "Test Album");
        var photos = new List<Photo>
        {
            new Photo(1, 1, "Photo 1", "https://example.com/1.jpg", "https://example.com/t1.jpg"),
            new Photo(2, 1, "Photo 2", "https://example.com/2.jpg", "https://example.com/t2.jpg"),
        };
        var albumWithPhotos = new AlbumWithPhotos(album, photos);

        Assert.All(albumWithPhotos.Photos, photo => Assert.Equal(albumWithPhotos.Album.Id, photo.AlbumId));
    }

    [Fact]
    public void AlbumWithPhotos_CanHaveEmptyPhotoList()
    {
        var album = new Album(Id: 1, UserId: 1, Title: "Empty Album");
        var albumWithPhotos = new AlbumWithPhotos(album, new List<Photo>());

        Assert.NotNull(albumWithPhotos.Photos);
        Assert.Empty(albumWithPhotos.Photos);
    }

    [Fact]
    public void AlbumWithPhotos_Album_IsAccessible()
    {
        var album = new Album(Id: 42, UserId: 7, Title: "My Album");
        var albumWithPhotos = new AlbumWithPhotos(album, Array.Empty<Photo>());

        Assert.Equal(42, albumWithPhotos.Album.Id);
        Assert.Equal(7, albumWithPhotos.Album.UserId);
        Assert.Equal("My Album", albumWithPhotos.Album.Title);
    }

    [Fact]
    public void AlbumWithPhotos_Photos_IsIReadOnlyList()
    {
        var album = new Album(1, 1, "Test");
        var albumWithPhotos = new AlbumWithPhotos(album, new List<Photo>());

        Assert.IsAssignableFrom<IReadOnlyList<Photo>>(albumWithPhotos.Photos);
    }
}
