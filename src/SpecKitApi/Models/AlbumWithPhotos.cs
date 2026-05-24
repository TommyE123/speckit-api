namespace SpecKitApi.Models;

/// <summary>
/// Internal domain model representing an album combined with its associated photos.
/// </summary>
/// <param name="Album">The album.</param>
/// <param name="Photos">The list of photos belonging to this album.</param>
public sealed record AlbumWithPhotos(Album Album, IReadOnlyList<Photo> Photos);
