using System.Net;
using System.Text;
using System.Text.Json;
using SpecKitApi.Clients;

namespace SpecKitApi.Tests.Clients;

/// <summary>
/// Hand-rolled DelegatingHandler stub for testing HttpClient without live HTTP.
/// </summary>
public class MockHttpMessageHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
    public HttpRequestMessage? LastRequest { get; private set; }

    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(_handler(request));
    }
}

public class JsonPlaceholderClientTests
{
    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var mockHandler = new MockHttpMessageHandler(handler);
        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
        };
        return httpClient;
    }

    private static HttpResponseMessage JsonResponse(object body)
    {
        var json = JsonSerializer.Serialize(body);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    [Fact]
    public async Task GetAlbumsAsync_ReturnsDeserializedAlbums()
    {
        var albums = new[] { new { id = 1, userId = 2, title = "Test Album" } };
        var httpClient = CreateHttpClient(_ => JsonResponse(albums));
        var client = new JsonPlaceholderClient(httpClient);

        var result = await client.GetAlbumsAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[0].UserId);
        Assert.Equal("Test Album", result[0].Title);
    }

    [Fact]
    public async Task GetAlbumsAsync_CallsCorrectEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;
        var httpClient = CreateHttpClient(req =>
        {
            capturedRequest = req;
            return JsonResponse(Array.Empty<object>());
        });
        var client = new JsonPlaceholderClient(httpClient);

        await client.GetAlbumsAsync();

        Assert.NotNull(capturedRequest);
        Assert.EndsWith("/albums", capturedRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetPhotosAsync_ReturnsDeserializedPhotos()
    {
        var photos = new[]
        {
            new { id = 1, albumId = 1, title = "Photo 1", url = "https://example.com/1.jpg", thumbnailUrl = "https://example.com/t1.jpg" }
        };
        var httpClient = CreateHttpClient(_ => JsonResponse(photos));
        var client = new JsonPlaceholderClient(httpClient);

        var result = await client.GetPhotosAsync();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(1, result[0].AlbumId);
        Assert.Equal("Photo 1", result[0].Title);
        Assert.Equal("https://example.com/1.jpg", result[0].Url);
        Assert.Equal("https://example.com/t1.jpg", result[0].ThumbnailUrl);
    }

    [Fact]
    public async Task GetPhotosAsync_CallsCorrectEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;
        var httpClient = CreateHttpClient(req =>
        {
            capturedRequest = req;
            return JsonResponse(Array.Empty<object>());
        });
        var client = new JsonPlaceholderClient(httpClient);

        await client.GetPhotosAsync();

        Assert.NotNull(capturedRequest);
        Assert.EndsWith("/photos", capturedRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetAlbumsAsync_ReturnsEmptyList_WhenApiReturnsEmptyArray()
    {
        var httpClient = CreateHttpClient(_ => JsonResponse(Array.Empty<object>()));
        var client = new JsonPlaceholderClient(httpClient);

        var result = await client.GetAlbumsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAlbumsAsync_ThrowsHttpRequestException_OnServerError()
    {
        var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new JsonPlaceholderClient(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAlbumsAsync());
    }
}
