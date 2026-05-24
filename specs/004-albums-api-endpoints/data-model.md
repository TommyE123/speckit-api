# Data Model: Albums API Endpoints

**Feature**: `004-albums-api-endpoints` | **Phase**: 1 — Design & Contracts

---

## Existing Domain Models (no modification)

These records live in `src/SpecKitApi/Models/` and are built in feature 001. They must not be exposed directly at the API boundary (constitution Principle I).

### `Album`
```csharp
// src/SpecKitApi/Models/Album.cs
public sealed record Album(int Id, int UserId, string Title);
```

| Field | Type | Constraints |
|---|---|---|
| `Id` | `int` | Positive integer; unique across all albums |
| `UserId` | `int` | Positive integer; foreign key to an owner user |
| `Title` | `string` | Non-null; may be empty |

### `Photo`
```csharp
// src/SpecKitApi/Models/Photo.cs
public sealed record Photo(int Id, int AlbumId, string Title, string ImageUrl, string ThumbnailUrl);
```

| Field | Type | Constraints |
|---|---|---|
| `Id` | `int` | Positive integer; unique across all photos |
| `AlbumId` | `int` | Positive integer; foreign key to owning album |
| `Title` | `string` | Non-null |
| `ImageUrl` | `string` | Absolute HTTP URL |
| `ThumbnailUrl` | `string` | Absolute HTTP URL |

### `AlbumWithPhotos`
```csharp
// src/SpecKitApi/Models/AlbumWithPhotos.cs
public sealed record AlbumWithPhotos(Album Album, IReadOnlyList<Photo> Photos);
```

The join entity produced by `IAlbumService`. It is the internal representation that gets mapped to response DTOs at the endpoint boundary.

---

## New Response DTOs (API boundary layer)

These records live in `src/SpecKitApi/DTOs/` and form the wire format returned by `GET /albums`. They mirror the domain models 1:1 today but are deliberately separate so that internal model changes cannot silently alter the API contract.

### `AlbumResponse`
```csharp
// src/SpecKitApi/DTOs/AlbumResponse.cs
public sealed record AlbumResponse(int Id, int UserId, string Title);
```

| JSON key | Type | Source |
|---|---|---|
| `id` | `number` | `Album.Id` |
| `userId` | `number` | `Album.UserId` |
| `title` | `string` | `Album.Title` |

### `PhotoResponse`
```csharp
// src/SpecKitApi/DTOs/PhotoResponse.cs
public sealed record PhotoResponse(int Id, int AlbumId, string Title, string ImageUrl, string ThumbnailUrl);
```

| JSON key | Type | Source |
|---|---|---|
| `id` | `number` | `Photo.Id` |
| `albumId` | `number` | `Photo.AlbumId` |
| `title` | `string` | `Photo.Title` |
| `imageUrl` | `string` | `Photo.ImageUrl` |
| `thumbnailUrl` | `string` | `Photo.ThumbnailUrl` |

### `AlbumWithPhotosResponse`
```csharp
// src/SpecKitApi/DTOs/AlbumWithPhotosResponse.cs
public sealed record AlbumWithPhotosResponse(
    AlbumResponse Album,
    IReadOnlyList<PhotoResponse> Photos);
```

| JSON key | Type | Source |
|---|---|---|
| `album` | `object` | Mapped `AlbumResponse` |
| `photos` | `array` | Mapped `PhotoResponse[]` |

**Mapping** (in `AlbumsEndpoints.cs`):
```csharp
var response = results.Select(awp => new AlbumWithPhotosResponse(
    new AlbumResponse(awp.Album.Id, awp.Album.UserId, awp.Album.Title),
    awp.Photos.Select(p => new PhotoResponse(
        p.Id, p.AlbumId, p.Title, p.ImageUrl, p.ThumbnailUrl)).ToList()
)).ToList();
```

---

## New Error Model

### `ErrorResponse`
```csharp
// src/SpecKitApi/Models/ErrorResponse.cs
public sealed record ErrorResponse(string Message, string Code, string CorrelationId);
```

| JSON key | Type | Notes |
|---|---|---|
| `message` | `string` | Human-readable; MUST NOT contain stack trace, exception type, or internal path |
| `code` | `string` | Stable machine-readable error code (see Code Values table below) |
| `correlationId` | `string` | Request trace ID; sourced from `HttpContext.Items["CorrelationId"]` set by `CorrelationIdMiddleware` (FR-015, SC-008) |

**Code Values**:

| Value | HTTP status | Trigger |
|---|---|---|
| `"INVALID_PARAMETER"` | 400 | `userId` is present but not a valid positive integer |
| `"INTERNAL_ERROR"` | 500 | Unhandled exception from the service or client layer |

Used by:
- `GET /albums` handler → HTTP 400 on invalid `userId`: `new ErrorResponse("userId must be a positive integer.", "INVALID_PARAMETER", correlationId)`
- Top-level `UseExceptionHandler` → HTTP 500: `new ErrorResponse("An unexpected error occurred.", "INTERNAL_ERROR", correlationId)`

**Accessing `correlationId` in handlers**: `CorrelationIdMiddleware` stores the resolved ID in `HttpContext.Items["CorrelationId"]`. Both endpoint handlers and the global exception handler read it via:
```csharp
var correlationId = httpContext.Items["CorrelationId"]?.ToString() ?? httpContext.TraceIdentifier;
```

**System.Text.Json serialisation note**: Record primary-constructor parameters serialise with their parameter name, which is PascalCase by default (`"Message"`, `"Code"`, `"CorrelationId"`). To emit camelCase (`"message"`, `"code"`, `"correlationId"`), configure `JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase` in `Program.cs`:
```csharp
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
```

---

## Configuration Model (existing, no change)

### `JsonPlaceholderOptions`
```csharp
// src/SpecKitApi/Options/JsonPlaceholderOptions.cs
public sealed class JsonPlaceholderOptions
{
    public const string SectionName = "JsonPlaceholderOptions";
    public string BaseUrl { get; init; } = string.Empty;
}
```

Bound from `appsettings.json`:
```json
{
  "JsonPlaceholderOptions": {
    "BaseUrl": "https://jsonplaceholder.typicode.com"
  }
}
```

---

## Entity Relationships

```
AlbumWithPhotosResponse
 ├── Album: AlbumResponse          (1)
 └── Photos: PhotoResponse[]       (0..*)

AlbumResponse  ←maps from→  Album     ←source→  AlbumDto (JSONPlaceholder API)
PhotoResponse  ←maps from→  Photo     ←source→  PhotoDto (JSONPlaceholder API)
```

**Layer boundary summary**:

| Layer | Types | Direction |
|---|---|---|
| External API (JSONPlaceholder) | `AlbumDto`, `PhotoDto` | Inbound via `IJsonPlaceholderClient` |
| Domain / Service | `Album`, `Photo`, `AlbumWithPhotos` | Internal only |
| API Response (this feature) | `AlbumResponse`, `PhotoResponse`, `AlbumWithPhotosResponse`, `ErrorResponse` | Outbound via Minimal API endpoints |

---

## State Transitions

No server-side state mutations in this feature. All endpoints are read-only:
- `GET /albums` — reads; no write
- `GET /health` — static response; no read or write
