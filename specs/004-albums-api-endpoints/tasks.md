# Tasks: Albums API Endpoints

**Spec**: `specs/004-albums-api-endpoints/spec.md`
**Branch**: `004-albums-api-endpoints`
**Generated**: 2026-05-24

---

## Overview

Upgrade the existing `src/SpecKitApi` class library to an ASP.NET Core Web application (`Microsoft.NET.Sdk.Web`) and wire the existing `AlbumService` and `JsonPlaceholderClient` into `GET /albums` and `GET /health` Minimal API endpoints. Add `CorrelationIdMiddleware`, a global exception handler, integration tests in the existing test project, and a README update. All 31 existing unit tests must remain green.

---

## Task List

### T01 — Upgrade SpecKitApi.csproj to the Web SDK

**Requires**: nothing  
**Files touched**:
- `src/SpecKitApi/SpecKitApi.csproj` *(modify)*

**What to do**:

1. Change `<Project Sdk="Microsoft.NET.Sdk">` to `<Project Sdk="Microsoft.NET.Sdk.Web">`.
2. Remove the explicit `Microsoft.Extensions.Http` package reference — `Microsoft.NET.Sdk.Web` includes it transitively.
3. Keep `Microsoft.Extensions.Http.Resilience`.
4. Add `<GenerateDocumentationFile>false</GenerateDocumentationFile>` (or keep existing — the Web SDK does not require it; if already present leave it).
5. Run `dotnet build SpecKitApi.slnx` — must exit 0 with zero warnings and zero errors.

**Acceptance**: `dotnet build SpecKitApi.slnx` passes.

---

### T02 — Add response DTOs and ErrorResponse model

**Requires**: T01  
**Files touched**:
- `src/SpecKitApi/Models/ErrorResponse.cs` *(create)*
- `src/SpecKitApi/DTOs/AlbumResponse.cs` *(create)*
- `src/SpecKitApi/DTOs/PhotoResponse.cs` *(create)*
- `src/SpecKitApi/DTOs/AlbumWithPhotosResponse.cs` *(create)*

**What to do**:

1. Create `src/SpecKitApi/Models/ErrorResponse.cs`:
   ```csharp
   namespace SpecKitApi.Models;

   public sealed record ErrorResponse(string Message);
   ```

2. Create `src/SpecKitApi/DTOs/PhotoResponse.cs`:
   ```csharp
   namespace SpecKitApi.DTOs;

   public sealed record PhotoResponse(int Id, int AlbumId, string Title, string Url, string ThumbnailUrl);
   ```

3. Create `src/SpecKitApi/DTOs/AlbumResponse.cs`:
   ```csharp
   namespace SpecKitApi.DTOs;

   public sealed record AlbumResponse(int Id, int UserId, string Title);
   ```

4. Create `src/SpecKitApi/DTOs/AlbumWithPhotosResponse.cs`:
   ```csharp
   namespace SpecKitApi.DTOs;

   public sealed record AlbumWithPhotosResponse(int Id, int UserId, string Title, IReadOnlyList<PhotoResponse> Photos);
   ```

5. Run `dotnet build` — zero warnings, zero errors.

**Acceptance**: `dotnet build SpecKitApi.slnx` passes with all four new types present.

---

### T03 — Create Program.cs and AlbumsEndpoints

**Requires**: T02  
**Files touched**:
- `src/SpecKitApi/Program.cs` *(create)*
- `src/SpecKitApi/Endpoints/AlbumsEndpoints.cs` *(create)*

**What to do**:

1. Create `src/SpecKitApi/Endpoints/AlbumsEndpoints.cs` — an `IEndpointRouteBuilder` extension method `MapAlbums()`:
   - **`GET /health`** (FR-005, SC-001): returns `Results.Ok()` (HTTP 200).
   - **`GET /albums`** (FR-001, FR-002, FR-003, FR-004):
     - Declare `userId` as `string?` so the handler controls the 400 body (see research Q4).
     - If `userId is not null` and either `!int.TryParse(userId, out var parsedId) || parsedId <= 0`, return `Results.Json(new ErrorResponse("userId must be a positive integer."), statusCode: 400)`.
     - Otherwise call `IAlbumService`:
       - No `userId` → `await service.GetAlbumsWithPhotosAsync()`
       - Valid `userId` → `await service.GetAlbumsWithPhotosByUserAsync(parsedId)`
     - Map the `IReadOnlyList<AlbumWithPhotos>` result to `IReadOnlyList<AlbumWithPhotosResponse>` using `Select` projections.
     - Return `Results.Ok(response)`.
     - Do **not** add `try/catch` — the global exception handler (T04) covers unhandled exceptions.
   - Inject `IAlbumService` and `ILogger<AlbumsEndpoints>` (or a named category) via route handler parameters (constructor injection is not available in Minimal API lambdas; use `[FromServices]` or rely on automatic DI resolution).

2. Create `src/SpecKitApi/Program.cs`:
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   builder.Services.AddJsonPlaceholderServices(builder.Configuration);

   var app = builder.Build();

   // Middleware order: correlation first, then exception handler, then endpoints
   app.UseMiddleware<CorrelationIdMiddleware>();  // added in T04
   app.UseExceptionHandler(...);                 // added in T04

   app.MapAlbums();

   app.Run();

   public partial class Program { }  // required for WebApplicationFactory<Program> in integration tests
   ```
   - Use the `using` directives for `SpecKitApi.Extensions`, `SpecKitApi.Endpoints`, `SpecKitApi.Middleware`.
   - Stub out the `UseMiddleware` and `UseExceptionHandler` calls with `// TODO: T04` comments so the project compiles before T04.

3. Run `dotnet build` — zero warnings, zero errors.

**Acceptance**: `dotnet build SpecKitApi.slnx` passes; `GET /health` and `GET /albums` routes are registered.

---

### T04 — Add CorrelationIdMiddleware and global exception handler

**Requires**: T03  
**Files touched**:
- `src/SpecKitApi/Middleware/CorrelationIdMiddleware.cs` *(create)*
- `src/SpecKitApi/Program.cs` *(update)*

**What to do**:

1. Create `src/SpecKitApi/Middleware/CorrelationIdMiddleware.cs`:
   - Read `X-Correlation-ID` request header; fall back to `HttpContext.TraceIdentifier`.
   - Echo the value in the `X-Correlation-ID` response header.
   - Open `ILogger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId })` around `await _next(context)`.
   - Register as `app.UseMiddleware<CorrelationIdMiddleware>()` in `Program.cs`.

2. In `Program.cs`, add `app.UseExceptionHandler(exceptionApp => { exceptionApp.Run(async ctx => { ... await ctx.Response.WriteAsJsonAsync(new ErrorResponse("An unexpected error occurred.")); }); })` **after** `UseMiddleware<CorrelationIdMiddleware>()` and **before** `app.MapAlbums()`. The handler must:
   - Set `ctx.Response.StatusCode = 500` and `Content-Type: application/json`.
   - Log the exception with the logger (correlation ID already in scope from the middleware).
   - Write `new ErrorResponse("An unexpected error occurred.")` as JSON — never the raw exception message.

3. Remove the `// TODO: T04` stubs from `Program.cs`.

4. Run `dotnet build` — zero warnings, zero errors.

**Acceptance**: 
- Triggering an unhandled exception in a test stub returns HTTP 500 with `{ "message": "An unexpected error occurred." }` and no stack trace.
- Log entries include a `CorrelationId` field consistent across all entries for a request.

---

### T05 — Add Microsoft.AspNetCore.Mvc.Testing to the test project

**Requires**: T01  
**Files touched**:
- `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` *(modify)*

**What to do**:

1. Add the following package reference to `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`:
   ```xml
   <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />
   ```
   Use the latest stable version compatible with `net10.0`.

2. Run `dotnet restore` then `dotnet build` — zero warnings, zero errors.

3. Run `dotnet test tests/SpecKitApi.Tests` — all 31 existing tests must still pass.

**Acceptance**: `dotnet test tests/SpecKitApi.Tests` reports 31 passed, 0 failed.

---

### T06 — Write integration tests (Red → Green)

**Requires**: T03, T04, T05  
**Files touched**:
- `tests/SpecKitApi.Tests/Integration/Helpers/StubJsonPlaceholderClient.cs` *(create)*
- `tests/SpecKitApi.Tests/Integration/AlbumsEndpointsIntegrationTests.cs` *(create)*

**What to do**:

**`Integration/Helpers/StubJsonPlaceholderClient.cs`**  
Implement `IJsonPlaceholderClient` with:
- Configurable list of `AlbumDto` to return from `GetAlbumsAsync()`.
- Configurable list of `PhotoDto` to return from `GetPhotosAsync()`.
- A `ThrowOnCall` flag: when `true`, throw `HttpRequestException("Stub failure")` from both methods.
- Use a deterministic dataset: 3 albums (userId 1, 1, 2) each with 2 photos for happy-path tests; empty lists for empty-result tests.

Use `WebApplicationFactory<Program>` with `ConfigureTestServices` to replace `IJsonPlaceholderClient` with the stub (and remove `IHttpClientFactory`-related registrations if needed).

**`Integration/AlbumsEndpointsIntegrationTests.cs`**  
Write the following tests using `xUnit` and `WebApplicationFactory<Program>`:

1. `GetAlbums_ReturnsOkWithAlbumsAndPhotos` — `GET /albums` → HTTP 200; body is a non-empty JSON array; each album has a non-null `photos` collection (FR-001, SC-002).
2. `GetAlbums_FilterByUserId_ReturnsOnlyMatchingAlbums` — `GET /albums?userId=1` → HTTP 200; only albums with `userId == 1` are present (FR-002, SC-003).
3. `GetAlbums_FilterByUserId_NoMatches_ReturnsEmptyArray` — `GET /albums?userId=99` → HTTP 200; body is `[]` (FR-002).
4. `GetAlbums_InvalidUserId_ReturnsClientError` — `GET /albums?userId=abc` → HTTP 400; body contains a `message` field; no stack trace (FR-003, SC-004).
5. `GetAlbums_ZeroUserId_ReturnsBadRequest` — `GET /albums?userId=0` → HTTP 400; body contains a `message` field (FR-003).
6. `GetAlbums_NegativeUserId_ReturnsBadRequest` — `GET /albums?userId=-1` → HTTP 400; body contains a `message` field (FR-003).
7. `GetAlbums_ServiceThrows_ReturnsInternalServerError` — stub configured to throw; `GET /albums` → HTTP 500; body contains `message` field; no stack trace, exception type name, or internal path (FR-004, FR-006, SC-007).
8. `GetAlbums_ResponseContainsCorrelationIdHeader` — `GET /albums` → response contains `X-Correlation-ID` header (FR-007, SC-006).
9. `GetHealth_ReturnsOk` — `GET /health` → HTTP 200 (FR-005, SC-001).

All tests must use the stub — **no live network calls**.

**Acceptance**: `dotnet test` passes all new integration tests plus all 31 existing unit tests.

---

### T07 — Verify all existing unit tests still pass

**Requires**: T01  
**Files touched**: none  

**What to do**:

Run `dotnet test tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj --filter "Category!=Integration"` (or just `dotnet test tests/SpecKitApi.Tests`) from the repo root. All 31 pre-existing tests must pass without modification.

If any test fails due to the SDK upgrade or new files (e.g., namespace collisions), fix the root cause — do **not** modify existing test source files.

**Acceptance**: `dotnet test tests/SpecKitApi.Tests` reports 31 passed, 0 failed (excluding integration tests).

---

### T08 — Update README with curl examples

**Requires**: T03  
**Files touched**:
- `README.md` *(update)*

**What to do**:

Add a **Getting Started** section to `README.md` (before or after any existing sections) covering:

1. Prerequisites (`.NET 10 SDK`).
2. How to run the API:
   ```bash
   dotnet run --project src/SpecKitApi
   ```
3. Example curl commands:
   ```bash
   # Health check
   curl http://localhost:5000/health

   # All albums with photos
   curl http://localhost:5000/albums

   # Albums filtered by user ID
   curl "http://localhost:5000/albums?userId=1"
   ```
4. Brief description of expected response shape (no need to reproduce full JSON).

**Acceptance**: `README.md` contains the curl examples; the API can be started following the documented steps.

---

## Dependency Order

```
T01
├── T02
│   └── T03
│       └── T04
├── T05
│   └── (T06 waits for T03+T04+T05)
└── T07

T06 (needs T03 + T04 + T05)
T08 (needs T03)
```

Safe parallel execution after T01: T02, T05, and T07 can start immediately after T01.  
T03 starts after T02. T04 starts after T03. T06 starts after T03+T04+T05 are all done. T08 starts after T03.

---

## Requirements Coverage

| Task | FRs covered |
|------|-------------|
| T01  | FR-008, FR-010, FR-011 (foundation — SDK upgrade) |
| T02  | FR-006 (ErrorResponse contract), FR-009 (DTO layer) |
| T03  | FR-001, FR-002, FR-003, FR-005, FR-009 |
| T04  | FR-004, FR-006, FR-007 |
| T05  | FR-011, FR-012 (test infrastructure) |
| T06  | FR-001..FR-007, FR-011, FR-012 |
| T07  | FR-011 |
| T08  | SC-001 (documentation) |
