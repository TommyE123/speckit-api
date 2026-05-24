# Quickstart: Add Scalar OpenAPI UI

**Feature**: `006-add-scalar-ui`

Use this quickstart to verify OpenAPI and Scalar behavior after implementation.

## Prerequisites

- .NET 10 SDK installed
- Restored NuGet dependencies

## 1) Restore and build

From repository root:

```powershell
dotnet restore
dotnet build SpecKitApi.slnx --no-incremental
```

Expected result:
- Restore/build succeed.
- No changes to business logic are required for docs/UI enablement.

## 2) Run in Development and validate UI/document routes

```powershell
dotnet run --project src/SpecKitApi/SpecKitApi.csproj
```

Manual checks:
- Browser opens to `/scalar` within 5 seconds of process start.
- `/scalar` loads interactive API docs.
- `/openapi/v1.json` returns OpenAPI JSON.
- OpenAPI content includes summaries for `GET /albums` and `GET /health`.
- `/albums` operation includes optional integer query parameter `userId`.
- Response code documentation includes `200`, `400`, and `500` for `/albums`.

## 3) Exercise endpoints from Scalar UI

From `/scalar`:
- Execute `GET /albums` without parameters and verify a JSON array response.
- Execute `GET /albums` with a valid `userId` and verify filtered results.
- Execute `GET /health` and verify `{ "status": "healthy" }`.

## 4) Validate non-Development behavior

Run with a non-Development environment profile (for example, Production).

Checks:
- `/scalar` returns `404 Not Found`.
- `/openapi/v1.json` is still reachable.

## 5) Run regression tests

```powershell
dotnet test SpecKitApi.slnx
```

Expected result:
- Existing test suite passes with no test modifications required for this feature.

## Definition of Done

The feature is complete when all of the following are true:
- `/scalar` is available only in Development.
- `/scalar` returns 404 outside Development.
- `/openapi/v1.json` is available in all environments.
- Endpoint docs include required summaries, parameter docs, and response codes.
- Development launch profile opens to `/scalar`.
- Existing tests pass.
