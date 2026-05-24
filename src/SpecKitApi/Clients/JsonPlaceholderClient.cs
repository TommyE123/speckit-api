using System.Net.Http.Json;
using SpecKitApi.DTOs;

namespace SpecKitApi.Clients;

/// <summary>
/// Typed HTTP client implementation for the JSONPlaceholder external API.
/// </summary>
public sealed class JsonPlaceholderClient : IJsonPlaceholderClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="JsonPlaceholderClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for JSONPlaceholder.</param>
    public JsonPlaceholderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<AlbumDto>> GetAlbumsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await _httpClient
            .GetFromJsonAsync<List<AlbumDto>>("/albums", cancellationToken)
            .ConfigureAwait(false);
        return result ?? [];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PhotoDto>> GetPhotosAsync(
        CancellationToken cancellationToken = default
    )
    {
        var result = await _httpClient
            .GetFromJsonAsync<List<PhotoDto>>("/photos", cancellationToken)
            .ConfigureAwait(false);
        return result ?? [];
    }
}
