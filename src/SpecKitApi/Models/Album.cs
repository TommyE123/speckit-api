namespace SpecKitApi.Models;

/// <summary>
/// Internal domain model representing an album.
/// </summary>
/// <param name="Id">The album identifier.</param>
/// <param name="UserId">The identifier of the user who owns the album.</param>
/// <param name="Title">The album title.</param>
public sealed record Album(int Id, int UserId, string Title);
