using System.Text.Json.Serialization;

namespace SpecKitApi.DTOs;

/// <summary>
/// Data Transfer Object representing an album as returned by the JSONPlaceholder API.
/// </summary>
public sealed class AlbumDto
{
    /// <summary>Gets the album identifier.</summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>Gets the identifier of the user who owns the album.</summary>
    [JsonPropertyName("userId")]
    public int UserId { get; init; }

    /// <summary>Gets the album title.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
}
