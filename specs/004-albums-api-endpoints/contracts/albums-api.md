# API Contract: Albums API Endpoints

**Feature**: `004-albums-api-endpoints` | **Version**: `1.0.0` | **Date**: 2026-05-24

**Base URL**: `http://localhost:5000` (development default; configurable via launch settings)

**Protocol**: HTTP/1.1 and HTTP/2  
**Content-Type**: `application/json` for all request and response bodies  
**Authentication**: None (public endpoints)

---

## Endpoints

### `GET /health`

Returns a 200 OK response when the API process is running and accepting requests.

**Use cases**: Load balancer probes, container readiness/liveness checks, deployment smoke tests.

#### Request

```
GET /health HTTP/1.1
```

No query parameters. No request body.

#### Response — 200 OK

```
HTTP/1.1 200 OK
Content-Length: 0
```

No response body. The empty 200 is the signal.

**SLA**: Must respond within 500 ms of startup (SC-001).

---

### `GET /albums`

Returns all albums, each with its full associated photos collection. Optionally filtered to a single user.

#### Request

```
GET /albums HTTP/1.1
GET /albums?userId=1 HTTP/1.1
```

**Query parameters**:

| Parameter | Type | Required | Description |
|---|---|---|---|
| `userId` | integer (positive) | No | When supplied, returns only albums belonging to this user. Omit to return all albums. |

#### Response — 200 OK (all albums)

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
X-Correlation-ID: {correlationId}
```

```json
[
  {
    "album": {
      "id": 1,
      "userId": 1,
      "title": "quidem molestiae enim"
    },
    "photos": [
      {
        "id": 1,
        "albumId": 1,
        "title": "accusamus beatae ad facilis cum similique qui sunt",
        "imageUrl": "https://via.placeholder.com/600/92c952",
        "thumbnailUrl": "https://via.placeholder.com/150/92c952"
      },
      {
        "id": 2,
        "albumId": 1,
        "title": "reprehenderit est deserunt velit ipsam",
        "imageUrl": "https://via.placeholder.com/600/771796",
        "thumbnailUrl": "https://via.placeholder.com/150/771796"
      }
    ]
  }
]
```

**Notes**:
- Array is empty (`[]`) when no albums exist.
- Every album object contains a `photos` array that is never `null`; it may be empty if the album has no associated photos.

#### Response — 200 OK (filtered by `userId`, no matching albums)

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
X-Correlation-ID: {correlationId}
```

```json
[]
```

An empty array — not a 404 — is returned when the user ID is valid but owns no albums.

#### Response — 400 Bad Request (invalid `userId`)

Returned when `userId` is present but cannot be parsed as a positive integer. Covers: `userId=abc`, `userId=0`, `userId=-1`, `userId=1.5`.

```
HTTP/1.1 400 Bad Request
Content-Type: application/json; charset=utf-8
X-Correlation-ID: {correlationId}
```

```json
{
  "message": "userId must be a positive integer.",
  "code": "INVALID_PARAMETER",
  "correlationId": "{correlationId}"
}
```

#### Response — 500 Internal Server Error (upstream failure)

Returned when the upstream JSONPlaceholder service is unavailable or returns an error that cannot be recovered.

```
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
X-Correlation-ID: {correlationId}
```

```json
{
  "message": "An unexpected error occurred.",
  "code": "INTERNAL_ERROR",
  "correlationId": "{correlationId}"
}
```

**MUST NOT** contain: stack traces, exception type names, internal file paths, or any implementation detail.

---

## Response Headers

| Header | Applies to | Description |
|---|---|---|
| `Content-Type` | All JSON responses | `application/json; charset=utf-8` |
| `X-Correlation-ID` | All responses | Echoes the value from the request `X-Correlation-ID` header, or a server-generated unique ID if the header was absent |

---

## Error Response Schema

All non-2xx responses return the same `ErrorResponse` shape:

```json
{
  "message": "string — human-readable description; no internal details",
  "code": "string — stable machine-readable error code",
  "correlationId": "string — request trace identifier"
}
```

| Field | Type | Always present | Notes |
|---|---|---|---|
| `message` | `string` | Yes | Safe, generic description of what went wrong |
| `code` | `string` | Yes | Stable machine-readable code for programmatic error handling |
| `correlationId` | `string` | Yes | Echoes the correlation ID for the request (from `X-Correlation-ID` header or generated) |

**Error Code Values**:

| Code | HTTP Status | Trigger |
|---|---|---|
| `INVALID_PARAMETER` | 400 | `userId` is present but cannot be parsed as a positive integer |
| `INTERNAL_ERROR` | 500 | Unhandled exception from the service or client layer |

---

## Behaviour Specifications

### Unknown query parameters

Additional query parameters beyond `userId` are silently ignored. Example: `GET /albums?userId=1&format=csv` is treated as `GET /albums?userId=1`.

### `userId` validation

| Value | Result |
|---|---|
| absent | All albums returned (200) |
| valid positive integer (e.g., `1`) | Filtered albums returned (200); empty array if none found |
| `0` | 400 — must be positive |
| negative integer (e.g., `-5`) | 400 — must be positive |
| floating-point string (e.g., `1.5`) | 400 — not parseable as integer |
| non-numeric string (e.g., `abc`) | 400 — not parseable as integer |

### Concurrency

Standard ASP.NET Core async I/O; requests are handled concurrently. No throttling or rate limiting in scope for this feature.

### Large payloads

Up to 100 albums × 50 photos each (5 000 photo objects) must be handled without error. No streaming or pagination is required for this feature.

---

## JSON Property Naming

All JSON keys use **camelCase** (enforced via `JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase` in `Program.cs`).

---

## Versioning

This is the initial `1.0.0` version. No versioning prefix (`/v1/`) is used — the API is not yet public. A versioning strategy MUST be introduced before the first public release.
