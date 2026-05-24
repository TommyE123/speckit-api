using System.Text.Json.Serialization;

namespace SpecKitApi.DTOs;

/// <summary>
/// Data Transfer Object representing a photo as returned by the JSONPlaceholder API.
/// </summary>
public sealed class PhotoDto
{
    /// <summary>Gets the photo identifier.</summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>Gets the identifier of the album this photo belongs to.</summary>
    [JsonPropertyName("albumId")]
    public int AlbumId { get; init; }

    /// <summary>Gets the photo title.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the full-size image URL.</summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    /// <summary>Gets the thumbnail image URL.</summary>
    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; init; } = string.Empty;
}
