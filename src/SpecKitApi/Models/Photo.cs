namespace SpecKitApi.Models;

/// <summary>
/// Internal domain model representing a photo.
/// </summary>
/// <param name="Id">The photo identifier.</param>
/// <param name="AlbumId">The identifier of the album this photo belongs to.</param>
/// <param name="Title">The photo title.</param>
/// <param name="ImageUrl">The full-size image URL.</param>
/// <param name="ThumbnailUrl">The thumbnail image URL.</param>
public sealed record Photo(int Id, int AlbumId, string Title, string ImageUrl, string ThumbnailUrl);
