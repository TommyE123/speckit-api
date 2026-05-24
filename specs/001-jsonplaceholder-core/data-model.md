# Data Model: JSONPlaceholder Core Data Layer

**Feature**: 001-jsonplaceholder-core  
**Phase**: 1 — Design & Contracts  
**Date**: 2026-05-23

---

## Overview

This feature involves two external-API shapes (DTOs) and three internal domain types. A thin mapping step inside `AlbumService` converts DTOs to domain models. No external types leak across layer boundaries (constitution Principle I / Technical Stack mandate).

---

## External Data Source

**Base URL**: `https://jsonplaceholder.typicode.com`

| Endpoint | Volume | Description |
|----------|--------|-------------|
| `GET /albums` | ~100 records | All albums across all users |
| `GET /photos` | ~5,000 records | All photos across all albums |

---

## DTOs (External API Shapes)

DTOs live in `src/SpecKitApi/DTOs/`. They mirror the JSON field names via `[JsonPropertyName]` attributes. They are **never** passed beyond the `AlbumService` mapping boundary.

### AlbumDto

Maps directly from the JSONPlaceholder `/albums` response.

| Field | C# Type | JSON Name | Constraints | Source |
|-------|---------|-----------|-------------|--------|
| `Id` | `int` | `id` | Positive integer, unique | JSONPlaceholder |
| `UserId` | `int` | `userId` | Positive integer | JSONPlaceholder |
| `Title` | `string` | `title` | Non-null | JSONPlaceholder |

**C# declaration**:
```csharp
public sealed class AlbumDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("userId")]
    public int UserId { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
}
```

### PhotoDto

Maps directly from the JSONPlaceholder `/photos` response.

| Field | C# Type | JSON Name | Constraints | Source |
|-------|---------|-----------|-------------|--------|
| `Id` | `int` | `id` | Positive integer, unique | JSONPlaceholder |
| `AlbumId` | `int` | `albumId` | Positive integer, FK to album | JSONPlaceholder |
| `Title` | `string` | `title` | Non-null | JSONPlaceholder |
| `Url` | `string` | `url` | Absolute URL string | JSONPlaceholder |
| `ThumbnailUrl` | `string` | `thumbnailUrl` | Absolute URL string | JSONPlaceholder |

**C# declaration**:
```csharp
public sealed class PhotoDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("albumId")]
    public int AlbumId { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; init; } = string.Empty;
}
```

---

## Domain Models (Internal)

Domain models live in `src/SpecKitApi/Models/`. They are the public surface of the service layer. Naming follows idiomatic C# (PascalCase properties, no JSON attributes).

### Album

Represents a named photo collection owned by a user.

| Property | C# Type | Nullable | Description |
|----------|---------|----------|-------------|
| `Id` | `int` | No | Unique album identifier |
| `UserId` | `int` | No | Owning user identifier |
| `Title` | `string` | No | Album title |

**Relationships**:
- One `Album` → many `Photo` (via `Photo.AlbumId == Album.Id`)
- Many `Album` → one user (via `Album.UserId`)

**Validation rules**:
- `Id > 0`
- `UserId > 0`
- `Title` is non-null (empty string acceptable from source)

### Photo

Represents a single image within an album.

| Property | C# Type | Nullable | Description |
|----------|---------|----------|-------------|
| `Id` | `int` | No | Unique photo identifier |
| `AlbumId` | `int` | No | Parent album identifier |
| `Title` | `string` | No | Photo title |
| `ImageUrl` | `string` | No | Full-size image URL |
| `ThumbnailUrl` | `string` | No | Thumbnail image URL |

**Validation rules**:
- `Id > 0`
- `AlbumId > 0` and MUST correspond to a valid `Album.Id`
- `ImageUrl` and `ThumbnailUrl` are non-null

### AlbumWithPhotos

The primary output structure of the service layer. Pairs an `Album` with its associated `Photo` collection.

| Property | C# Type | Nullable | Description |
|----------|---------|----------|-------------|
| `Album` | `Album` | No | The parent album |
| `Photos` | `IReadOnlyList<Photo>` | No | Photos belonging to this album |

**Invariants**:
- Every `Photo` in `Photos` satisfies `Photo.AlbumId == Album.Id`
- `Photos` is never null (empty list when no photos exist for the album)

---

## Mapping: DTO → Domain Model

Mapping is performed inside `AlbumService` during the combine step. No mapping library is used (YAGNI — Principle V).

### AlbumDto → Album

```
AlbumDto.Id      → Album.Id
AlbumDto.UserId  → Album.UserId
AlbumDto.Title   → Album.Title
```

### PhotoDto → Photo

```
PhotoDto.Id           → Photo.Id
PhotoDto.AlbumId      → Photo.AlbumId
PhotoDto.Title        → Photo.Title
PhotoDto.Url          → Photo.ImageUrl
PhotoDto.ThumbnailUrl → Photo.ThumbnailUrl
```

Note: `Url` → `ImageUrl` is the only non-trivial rename. This insulates callers from the JSONPlaceholder naming convention.

---

## Entities Reference Table

| Entity | Layer | Location | Purpose |
|--------|-------|----------|---------|
| `AlbumDto` | DTO | `src/SpecKitApi/DTOs/AlbumDto.cs` | Deserialise `/albums` JSON |
| `PhotoDto` | DTO | `src/SpecKitApi/DTOs/PhotoDto.cs` | Deserialise `/photos` JSON |
| `Album` | Domain | `src/SpecKitApi/Models/Album.cs` | Internal album representation |
| `Photo` | Domain | `src/SpecKitApi/Models/Photo.cs` | Internal photo representation |
| `AlbumWithPhotos` | Domain | `src/SpecKitApi/Models/AlbumWithPhotos.cs` | Combined service output |

---

## State Transitions

This feature has no state machine. `AlbumWithPhotos` is a read-only projection assembled on each service call. There is no persistence layer; data is fetched and combined in memory on demand.

---

## Edge Cases and Handling

| Scenario | Handling |
|----------|---------|
| Empty `/albums` response | Service returns empty `IReadOnlyList<AlbumWithPhotos>` |
| Empty `/photos` response | Each album's `Photos` list is empty |
| Photo references non-existent album ID | Photo is silently dropped (not attached to any album); logged at Debug level |
| Duplicate entries from source | `GroupBy` on album ID naturally deduplicates grouping keys; duplicate photos appear in the same album's list |
| Network timeout / HTTP error | `IJsonPlaceholderClient` throws; exception propagates to caller (no swallowing) |
| Malformed/partial JSON | `System.Text.Json` throws `JsonException`; propagates to caller |
