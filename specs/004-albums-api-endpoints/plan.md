# Implementation Plan: Albums API Endpoints

**Branch**: `004-albums-api-endpoints` | **Date**: 2026-05-24 | **Spec**: `specs/004-albums-api-endpoints/spec.md`

**Input**: Feature specification from `specs/004-albums-api-endpoints/spec.md`

## Summary

Wire the existing `AlbumService` and `JsonPlaceholderClient` (built in feature 001) into a .NET 10 ASP.NET Core web application using the Minimal API pattern. The feature adds two endpoints — `GET /albums` (with optional `?userId` filter) and `GET /health` — together with a `CorrelationIdMiddleware` for structured log enrichment, a top-level exception handler for safe structured 500 responses, and integration tests backed by an in-process `WebApplicationFactory` stub. All 31 existing unit tests remain unchanged and must continue to pass.

## Technical Context

| Item | Value |
|---|---|
| Language / Framework | .NET 10 (C# 13), ASP.NET Core Minimal APIs |
| SDK change required | `SpecKitApi.csproj` must change from `Microsoft.NET.Sdk` → `Microsoft.NET.Sdk.Web` to activate the web host |
| Existing services | `IAlbumService` / `AlbumService`, `IJsonPlaceholderClient` / `JsonPlaceholderClient`, `ServiceCollectionExtensions.AddJsonPlaceholderServices` — **no modification required** |
| Serialization | `System.Text.Json` only (constitution-mandated; Newtonsoft.Json prohibited) |
| Testing — unit | xUnit v3 + Moq; 31 existing tests in `tests/SpecKitApi.Tests/` — must not regress |
| Testing — integration | `Microsoft.AspNetCore.Mvc.Testing` added to existing `SpecKitApi.Tests` project; `WebApplicationFactory<Program>` with `ConfigureTestServices` for stubbing `IJsonPlaceholderClient` |
| Configuration | `appsettings.json` → `JsonPlaceholderOptions:BaseUrl` already present; no new config values needed |
| Target platform | Single-process HTTP server; standard ASP.NET Core defaults; no clustering, distributed tracing, or load-balancing in scope |
| Performance goals | Standard ASP.NET Core defaults; `GET /health` MUST respond within 500 ms (SC-001); no explicit RPS target |
| Constraints | No MVC controllers; `System.Text.Json` only; no hardcoded URLs; `userId` validation controlled by handler (not framework binder) to ensure consistent `ErrorResponse` shape |
| New source files | `Program.cs`, `Endpoints/AlbumsEndpoints.cs`, `Middleware/CorrelationIdMiddleware.cs`, `Models/ErrorResponse.cs`, `DTOs/AlbumResponse.cs`, `DTOs/PhotoResponse.cs`, `DTOs/AlbumWithPhotosResponse.cs` |

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate (Phase 0 entry)

| Principle | Applicability | Status |
|---|---|---|
| I. API-First Design | REST contract for `GET /albums` and `GET /health` MUST be in `specs/004-albums-api-endpoints/contracts/` before coding begins | ✅ PASS — `contracts/albums-api.md` generated in Phase 1 below |
| II. Spec-Driven Development | `spec.md` complete; all five user stories carry acceptance scenarios, FRs, and measurable success criteria; zero `NEEDS CLARIFICATION` tokens | ✅ PASS |
| III. Test-First (NON-NEGOTIABLE) | Integration tests written first and confirmed failing; service-layer unit tests already green; TDD sequence enforced in tasks | ✅ PASS — sequence mandated in tasks |
| IV. Observability & Structured Logging | `CorrelationIdMiddleware` enriches every log entry with `CorrelationId`; top-level exception handler prevents silent failures; `GET /health` present | ✅ PASS |
| V. Simplicity & YAGNI | No new abstraction layers beyond spec requirements; response DTOs map 1:1 to domain models; no caching, repository pattern, or speculative generics introduced | ✅ PASS |

### Quality Gates

| Gate | Status | Notes |
|---|---|---|
| Spec Gate | ✅ PASS | `spec.md` complete; no `NEEDS CLARIFICATION` tokens |
| Plan Gate | ✅ PASS | All five constitution principles satisfied |
| Contract Gate | ✅ PASS | `specs/004-albums-api-endpoints/contracts/albums-api.md` generated in Phase 1 |
| Test Gate | ⏳ PENDING | Integration tests must be written first (Red) before endpoint code (Green) |
| Observability Gate | ⏳ PENDING | `CorrelationIdMiddleware` and exception handler implemented during tasks |

### Post-Design Re-check (Phase 1 exit)

All five principles remain satisfied after the Phase 1 design. The response DTO layer (Principle I) and the `contracts/albums-api.md` file close the only open gate from the pre-design check.

## Project Structure

### Documentation (this feature)

```text
specs/004-albums-api-endpoints/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── albums-api.md    # REST contract for GET /albums and GET /health
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/SpecKitApi/
├── Clients/
│   ├── IJsonPlaceholderClient.cs       # existing — no change
│   └── JsonPlaceholderClient.cs        # existing — no change
├── DTOs/
│   ├── AlbumDto.cs                     # existing — no change
│   ├── PhotoDto.cs                     # existing — no change
│   ├── AlbumResponse.cs                # NEW — API response DTO
│   ├── PhotoResponse.cs                # NEW — API response DTO
│   └── AlbumWithPhotosResponse.cs      # NEW — API response DTO
├── Endpoints/
│   └── AlbumsEndpoints.cs              # NEW — MapAlbums() IEndpointRouteBuilder extension
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # existing — no change
├── Middleware/
│   └── CorrelationIdMiddleware.cs      # NEW — reads X-Correlation-ID / TraceIdentifier,
│                                       #       sets response header, scopes ILogger
├── Models/
│   ├── Album.cs                        # existing — no change
│   ├── AlbumWithPhotos.cs              # existing — no change
│   ├── ErrorResponse.cs                # NEW — structured error record { Message }
│   └── Photo.cs                        # existing — no change
├── Options/
│   └── JsonPlaceholderOptions.cs       # existing — no change
├── Program.cs                          # NEW — WebApplication entry point
├── appsettings.json                    # existing — no change
└── SpecKitApi.csproj                   # MODIFY — SDK → Microsoft.NET.Sdk.Web;
                                        #           remove redundant Microsoft.Extensions.Http
                                        #           (included by Web SDK); keep Resilience pkg

tests/SpecKitApi.Tests/
├── Clients/
│   └── JsonPlaceholderClientTests.cs   # existing — no change
├── DTOs/
│   ├── AlbumDtoTests.cs                # existing — no change
│   └── PhotoDtoTests.cs                # existing — no change
├── Integration/
│   ├── AlbumsEndpointsIntegrationTests.cs  # NEW — WebApplicationFactory<Program> tests
│   └── Helpers/
│       └── StubJsonPlaceholderClient.cs    # NEW — deterministic in-process stub
├── Models/
│   └── AlbumWithPhotosTests.cs         # existing — no change
├── Services/
│   └── AlbumServiceTests.cs            # existing — no change
└── SpecKitApi.Tests.csproj             # MODIFY — add Microsoft.AspNetCore.Mvc.Testing
```

**Structure Decision**: Single web-service project (`src/SpecKitApi/`) with the new `Endpoints/` and `Middleware/` folders prescribed by the constitution's folder conventions. Integration tests live in an `Integration/` subfolder within the existing `tests/SpecKitApi.Tests/` project — no third project is introduced. This satisfies Principle V (YAGNI): `WebApplicationFactory<Program>` is importable via a single NuGet package without a dedicated project, and the existing test infrastructure (xUnit v3, Moq) is already in place.

## Complexity Tracking

No Constitution Check violations. The following non-obvious design decisions are documented for reviewers:

| Decision | Why this approach | Simpler alternative rejected because |
|---|---|---|
| `string?` for `userId` query param (not `int?`) | Lets the handler — not the framework binder — own the 400 response shape, guaranteeing consistent `ErrorResponse` JSON for `userId=abc`, `userId=0`, and `userId=1.5` (FR-003, FR-006) | `int?` binding lets ASP.NET Core return its own 400 body before the handler runs; overriding that body requires `IActionResultExecutor` hooks that add more complexity than switching to `string?` |
| `CorrelationIdMiddleware` (custom, no SDK) | No external dependency; `ILogger.BeginScope` enriches every log entry within the request pipeline natively | OpenTelemetry or Serilog would add a third-party dependency and exceed the spec's observability scope (FR-007 requires correlation ID on logs only) |
| Response DTOs (`AlbumResponse`, `PhotoResponse`, `AlbumWithPhotosResponse`) | Constitution Principle I mandates DTOs at API boundaries; prevents internal domain record changes from silently breaking the API contract | Returning domain models directly would couple the API wire format to the internal model; mapping code is three trivial `Select` projections |
| `UseExceptionHandler` lambda (not `/error` route) | Self-contained; no extra route registered in the endpoint table | `MapGet("/error", ...)` approach adds a discoverable route that callers could accidentally invoke directly |
