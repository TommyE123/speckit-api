namespace SpecKitApi.DTOs;

public sealed record PhotoResponse(int Id, int AlbumId, string Title, string ImageUrl, string ThumbnailUrl);
