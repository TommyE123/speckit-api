using Microsoft.AspNetCore.Http;
using SpecKitApi.DTOs;
using SpecKitApi.Models;
using SpecKitApi.Services;

namespace SpecKitApi.Endpoints;

public static class AlbumsEndpoints
{
    public static IEndpointRouteBuilder MapAlbums(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

        app.MapGet(
            "/albums",
            async (string? userId, IAlbumService service, HttpContext httpContext) =>
            {
                if (userId is not null)
                {
                    if (!int.TryParse(userId, out var parsedId) || parsedId <= 0)
                    {
                        var correlationId =
                            httpContext.Items["CorrelationId"]?.ToString()
                            ?? httpContext.TraceIdentifier;
                        return Results.Json(
                            new ErrorResponse(
                                "userId must be a positive integer.",
                                "INVALID_PARAMETER",
                                correlationId
                            ),
                            statusCode: 400
                        );
                    }

                    var filtered = await service.GetAlbumsWithPhotosByUserAsync(parsedId);
                    return Results.Ok(MapToResponse(filtered));
                }

                var all = await service.GetAlbumsWithPhotosAsync();
                return Results.Ok(MapToResponse(all));
            }
        );

        return app;
    }

    private static IReadOnlyList<AlbumWithPhotosResponse> MapToResponse(
        IReadOnlyList<Models.AlbumWithPhotos> albums
    ) =>
        albums
            .Select(a => new AlbumWithPhotosResponse(
                new AlbumResponse(a.Album.Id, a.Album.UserId, a.Album.Title),
                a.Photos.Select(p => new PhotoResponse(
                        p.Id,
                        p.AlbumId,
                        p.Title,
                        p.ImageUrl,
                        p.ThumbnailUrl
                    ))
                    .ToList()
            ))
            .ToList();
}
