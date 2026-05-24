using System.Text.Json;
using SpecKitApi.DTOs;

namespace SpecKitApi.Tests.DTOs;

public class AlbumDtoTests
{
    [Fact]
    public void AlbumDto_Deserializes_Id_Correctly()
    {
        var json = """{"id":1,"userId":2,"title":"test album"}""";
        var dto = JsonSerializer.Deserialize<AlbumDto>(json);
        Assert.NotNull(dto);
        Assert.Equal(1, dto.Id);
    }

    [Fact]
    public void AlbumDto_Deserializes_UserId_Correctly()
    {
        var json = """{"id":1,"userId":2,"title":"test album"}""";
        var dto = JsonSerializer.Deserialize<AlbumDto>(json);
        Assert.NotNull(dto);
        Assert.Equal(2, dto.UserId);
    }

    [Fact]
    public void AlbumDto_Deserializes_Title_Correctly()
    {
        var json = """{"id":1,"userId":2,"title":"test album"}""";
        var dto = JsonSerializer.Deserialize<AlbumDto>(json);
        Assert.NotNull(dto);
        Assert.Equal("test album", dto.Title);
    }

    [Fact]
    public void AlbumDto_Deserializes_AllFields_FromTypicalApiResponse()
    {
        var json = """{"userId":1,"id":5,"title":"eaque aut omnis a"}""";
        var dto = JsonSerializer.Deserialize<AlbumDto>(json);
        Assert.NotNull(dto);
        Assert.Equal(5, dto.Id);
        Assert.Equal(1, dto.UserId);
        Assert.Equal("eaque aut omnis a", dto.Title);
    }

    [Fact]
    public void AlbumDto_Title_DefaultsToEmptyString_WhenMissing()
    {
        var json = """{"id":1,"userId":2}""";
        var dto = JsonSerializer.Deserialize<AlbumDto>(json);
        Assert.NotNull(dto);
        Assert.Equal(string.Empty, dto.Title);
    }
}
