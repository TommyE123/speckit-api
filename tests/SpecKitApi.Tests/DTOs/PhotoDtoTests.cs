using System.Text.Json;
using SpecKitApi.DTOs;

namespace SpecKitApi.Tests.DTOs;

public class PhotoDtoTests
{
    private const string SampleJson = """
        {
            "albumId": 1,
            "id": 1,
            "title": "accusamus beatae ad facilis",
            "url": "https://via.placeholder.com/600/92c952",
            "thumbnailUrl": "https://via.placeholder.com/150/92c952"
        }
        """;

    [Fact]
    public void PhotoDto_Deserializes_Id_Correctly()
    {
        var dto = JsonSerializer.Deserialize<PhotoDto>(SampleJson);
        Assert.NotNull(dto);
        Assert.Equal(1, dto.Id);
    }

    [Fact]
    public void PhotoDto_Deserializes_AlbumId_Correctly()
    {
        var dto = JsonSerializer.Deserialize<PhotoDto>(SampleJson);
        Assert.NotNull(dto);
        Assert.Equal(1, dto.AlbumId);
    }

    [Fact]
    public void PhotoDto_Deserializes_Title_Correctly()
    {
        var dto = JsonSerializer.Deserialize<PhotoDto>(SampleJson);
        Assert.NotNull(dto);
        Assert.Equal("accusamus beatae ad facilis", dto.Title);
    }

    [Fact]
    public void PhotoDto_Deserializes_Url_Correctly()
    {
        var dto = JsonSerializer.Deserialize<PhotoDto>(SampleJson);
        Assert.NotNull(dto);
        Assert.Equal("https://via.placeholder.com/600/92c952", dto.Url);
    }

    [Fact]
    public void PhotoDto_Deserializes_ThumbnailUrl_Correctly()
    {
        var dto = JsonSerializer.Deserialize<PhotoDto>(SampleJson);
        Assert.NotNull(dto);
        Assert.Equal("https://via.placeholder.com/150/92c952", dto.ThumbnailUrl);
    }

    [Fact]
    public void PhotoDto_AllFields_DefaultToEmpty_WhenMissing()
    {
        var json = """{"id":1,"albumId":2}""";
        var dto = JsonSerializer.Deserialize<PhotoDto>(json);
        Assert.NotNull(dto);
        Assert.Equal(string.Empty, dto.Title);
        Assert.Equal(string.Empty, dto.Url);
        Assert.Equal(string.Empty, dto.ThumbnailUrl);
    }
}
