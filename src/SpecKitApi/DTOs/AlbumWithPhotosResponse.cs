namespace SpecKitApi.DTOs;

public sealed record AlbumWithPhotosResponse(
    AlbumResponse Album,
    IReadOnlyList<PhotoResponse> Photos
);
