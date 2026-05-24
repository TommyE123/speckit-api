using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SpecKitApi.Clients;
using SpecKitApi.Tests.Integration.Helpers;

namespace SpecKitApi.Tests.Integration;

public sealed class AlbumsEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AlbumsEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(Action<StubJsonPlaceholderClient>? configure = null)
    {
        var stub = new StubJsonPlaceholderClient();
        configure?.Invoke(stub);

        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove real IJsonPlaceholderClient registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IJsonPlaceholderClient));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddSingleton<IJsonPlaceholderClient>(stub);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetAlbums_ReturnsOkWithAlbumsAndPhotos()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/albums");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.True(root.GetArrayLength() > 0);
        var first = root[0];
        Assert.Equal(JsonValueKind.Array, first.GetProperty("photos").ValueKind);
    }

    [Fact]
    public async Task GetAlbums_FilterByUserId_ReturnsOnlyMatchingAlbums()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/albums?userId=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.True(root.GetArrayLength() > 0);
        foreach (var item in root.EnumerateArray())
        {
            Assert.Equal(1, item.GetProperty("album").GetProperty("userId").GetInt32());
        }
    }

    [Fact]
    public async Task GetAlbums_FilterByUserId_NoMatches_ReturnsEmptyArray()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/albums?userId=99");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetAlbums_InvalidUserId_ReturnsClientError()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/albums?userId=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("message", out _));
        Assert.DoesNotContain("StackTrace", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAlbums_ZeroUserId_ReturnsBadRequest()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/albums?userId=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task GetAlbums_NegativeUserId_ReturnsBadRequest()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/albums?userId=-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task GetAlbums_ServiceThrows_ReturnsInternalServerError()
    {
        var client = CreateClient(stub => stub.ThrowOnCall = true);
        var response = await client.GetAsync("/albums");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("message", out _));
        Assert.True(root.TryGetProperty("code", out _));
        Assert.True(root.TryGetProperty("correlationId", out _));
        Assert.DoesNotContain("StackTrace", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpRequestException", json);
    }

    [Fact]
    public async Task GetAlbums_ResponseContainsCorrelationIdHeader()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/albums");

        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task GetAlbums_InvalidUserId_ErrorBodyContainsCodeAndCorrelationId()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/albums?userId=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("message", out _));
        Assert.Equal("INVALID_PARAMETER", root.GetProperty("code").GetString());
        Assert.True(root.TryGetProperty("correlationId", out _));
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        Assert.Equal("healthy", doc.RootElement.GetProperty("status").GetString());
    }
}
