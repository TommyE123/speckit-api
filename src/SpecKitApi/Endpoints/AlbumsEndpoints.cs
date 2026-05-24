using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using SpecKitApi.DTOs;
using SpecKitApi.Models;
using SpecKitApi.Services;

namespace SpecKitApi.Endpoints;

public static class AlbumsEndpoints
{
    public static IEndpointRouteBuilder MapAlbums(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("GetHealth")
            .WithSummary("Service liveness check")
            .WithDescription(
                "Returns 200 OK with { \"status\": \"healthy\" } when the service is running."
            )
            .Produces<object>(200);

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
            )
            .WithName("GetAlbums")
            .WithSummary("Get all albums with photos, optionally filtered by user")
            .WithDescription(
                "Returns all albums with their associated photos. Supply the optional userId query parameter (positive integer) to filter results to a specific user."
            )
            .Produces<IReadOnlyList<AlbumWithPhotosResponse>>(200)
            .ProducesProblem(400)
            .ProducesProblem(500)
            .AddOpenApiOperationTransformer(
                (operation, _, _) =>
                {
                    var parameters = operation.Parameters;
                    if (parameters is not null)
                    {
                        for (var i = 0; i < parameters.Count; i++)
                        {
                            var parameter = parameters[i];
                            if (!string.Equals(parameter.Name, "userId", StringComparison.Ordinal))
                            {
                                continue;
                            }

                            parameters[i] = new OpenApiParameter
                            {
                                Name = parameter.Name,
                                In = parameter.In,
                                Description =
                                    "Optional positive integer. Omit to return all albums.",
                                Required = false,
                                Schema = new OpenApiSchema { Type = JsonSchemaType.Integer },
                            };
                            break;
                        }
                    }

                    return Task.CompletedTask;
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
