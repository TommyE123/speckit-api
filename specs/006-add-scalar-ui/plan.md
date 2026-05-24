# Implementation Plan: Add Scalar OpenAPI UI

**Branch**: `006-add-scalar-ui` | **Date**: 2026-05-24 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/006-add-scalar-ui/spec.md`

## Summary

Add Scalar as the OpenAPI interactive UI for the SpecKitApi .NET 10 Minimal API project. The
implementation installs `Scalar.AspNetCore`, registers `AddOpenApi("v1")` in the DI container,
maps `/openapi/v1.json` unconditionally in all environments, and mounts the Scalar UI at
`/scalar` in Development only. Browser launch is updated to `/scalar`. Endpoint metadata
(`.WithSummary`, `.WithDescription`, `.Produces`, `.ProducesProblem`) is added to the `/albums`
and `/health` endpoints. A targeted integration test verifies that `/scalar` returns `404` in
non-Development environments. No business logic, service, or data-access code is changed.

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**: `Microsoft.AspNetCore.OpenApi` (built-in, .NET 10), `Scalar.AspNetCore` (new — single new package)

**Storage**: N/A — no persistence layer involved in this feature

**Testing**: xUnit + `WebApplicationFactory<Program>` integration tests

**Target Platform**: ASP.NET Core 10 Minimal API, local developer workstation + any environment

**Project Type**: Web service (REST Minimal API)

**Performance Goals**: No new performance requirements — documentation surface only

**Constraints**: Scalar UI MUST NOT be accessible outside Development; OpenAPI JSON MUST be reachable in all environments

**Scale/Scope**: 5 files changed, 9 tasks, purely additive (no logic changes)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design — all gates pass.*

| Principle | Status | Notes |
|---|---|---|
| **I. API-First Design** | ✅ PASS | Contract defined in `contracts/openapi-scalar.md` before implementation |
| **II. Spec-Driven Development** | ✅ PASS | `spec.md` complete with user stories, FRs, and success criteria; full workflow followed |
| **III. Test-First (NON-NEGOTIABLE)** | ✅ PASS | T009 (integration test for non-Dev `/scalar` → 404) written before implementation; existing suite covers regressions |
| **IV. Observability & Structured Logging** | ✅ PASS | No new endpoints; existing correlation-ID middleware and exception handler remain untouched; health endpoint preserved |
| **V. Simplicity & YAGNI** | ✅ PASS | Single new package (`Scalar.AspNetCore`); built-in ASP.NET Core OpenAPI generation used; no custom transformers or abstractions introduced |

**Post-design re-check**: All five gates continue to pass. The design is additive and introduces no new abstractions, layers, or complexity beyond the minimum required by the spec.

## Project Structure

### Documentation (this feature)

```text
specs/006-add-scalar-ui/
├── plan.md                      # This file
├── spec.md                      # Feature specification (complete)
├── research.md                  # Phase 0: 5 decisions, no open questions
├── data-model.md                # Phase 1: 4 configuration entities
├── quickstart.md                # Phase 1: step-by-step verification guide
├── contracts/
│   └── openapi-scalar.md        # Interface contracts for /openapi/v1.json and /scalar
├── checklists/
│   └── requirements.md          # Requirements checklist
└── tasks.md                     # 9 tasks (T001–T009) in 6 phases
```

### Source Code (repository root)

```text
src/
└── SpecKitApi/
    ├── SpecKitApi.csproj                    # T001: add Scalar.AspNetCore PackageReference
    ├── Program.cs                           # T002: AddOpenApi("v1"); T003: MapOpenApi() + conditional MapScalarApiReference()
    ├── Properties/
    │   └── launchSettings.json              # T004: launchUrl "albums" → "scalar"
    └── Endpoints/
        └── AlbumsEndpoints.cs               # T005: /albums metadata; T006: /health metadata

tests/
└── SpecKitApi.Tests/
    └── Integration/
        ├── AlbumsEndpointsIntegrationTests.cs   # Existing — must continue to pass (T007)
        └── ScalarUiIntegrationTests.cs           # T009: new — /scalar → 404 in non-Development
```

**Structure Decision**: Single-project layout (Option 1). All changes are additive within the
existing `src/SpecKitApi/` project. No new folders or projects are introduced. Test assertion
added to `tests/SpecKitApi.Tests/Integration/` alongside existing integration tests.

## Implementation Phases

### Phase 1 — Setup (T001)

**Goal**: Add the only new NuGet dependency.

| Task | File | Change |
|---|---|---|
| T001 | `src/SpecKitApi/SpecKitApi.csproj` | Add `<PackageReference Include="Scalar.AspNetCore" Version="2.*" />` in existing `<ItemGroup>`; run `dotnet restore SpecKitApi.slnx` |

**Unblocks**: T002 (package must resolve before build)

---

### Phase 2 — Foundational DI Registration (T002)

**Goal**: Register the named OpenAPI document in the DI container. Blocks all middleware wiring and metadata rendering.

| Task | File | Change |
|---|---|---|
| T002 | `src/SpecKitApi/Program.cs` | Add `builder.Services.AddOpenApi("v1");` immediately after `builder.Services.AddJsonPlaceholderServices(...)` and before `var app = builder.Build();` |

**Unblocks**: T003, T005, T006

---

### Phase 3 — User Story 1: Launch Interactive API Documentation (T003, T004)

**Goal**: Scalar UI accessible at `/scalar` in Development; `/openapi/v1.json` available in all environments; browser opens to `/scalar` on `dotnet run`.

| Task | File | Change |
|---|---|---|
| T003 | `src/SpecKitApi/Program.cs` | After `app.UseMiddleware<CorrelationIdMiddleware>()` and before `app.MapAlbums()`: add `app.MapOpenApi();` unconditionally, then `if (app.Environment.IsDevelopment()) { app.MapScalarApiReference(); }` |
| T004 [P] | `src/SpecKitApi/Properties/launchSettings.json` | Change `"launchUrl"` value in the Development profile from `"albums"` to `"scalar"` |

**Checkpoint**: Run `dotnet run`; browser opens `/scalar`; `/openapi/v1.json` returns JSON; `ASPNETCORE_ENVIRONMENT=Production` → `/scalar` returns 404.

---

### Phase 4 — User Story 2: /albums Endpoint Documentation (T005)

**Goal**: `/albums` endpoint fully documented with summary, description, `userId` parameter (integer, optional), and response codes 200/400/500.

| Task | File | Change |
|---|---|---|
| T005 | `src/SpecKitApi/Endpoints/AlbumsEndpoints.cs` | Chain onto `app.MapGet("/albums", ...)`: `.WithName("GetAlbums")`, `.WithSummary("Get all albums with photos, optionally filtered by user")`, `.WithDescription("Returns all albums with their associated photos. Supply the optional userId query parameter (positive integer) to filter results to a specific user.")`, `.Produces<IReadOnlyList<AlbumWithPhotosResponse>>(200)`, `.ProducesProblem(400)`, `.ProducesProblem(500)`, `.WithOpenApi(op => { var p = op.Parameters.First(x => x.Name == "userId"); p.Description = "Optional positive integer. Omit to return all albums."; p.Schema.Type = "integer"; p.Required = false; return op; })` |

**Note**: The `.WithOpenApi()` transformer overrides the inferred `string` type to `integer` since the handler binds `string? userId` for manual validation (see research.md Decision 3 and tasks.md T005 note).

---

### Phase 5 — User Story 3: /health Endpoint Documentation (T006)

**Goal**: `/health` endpoint documented with summary, description, and 200 response code in the OpenAPI document.

| Task | File | Change |
|---|---|---|
| T006 | `src/SpecKitApi/Endpoints/AlbumsEndpoints.cs` | Chain onto `app.MapGet("/health", ...)`: `.WithName("GetHealth")`, `.WithSummary("Service liveness check")`, `.WithDescription("Returns 200 OK with { \"status\": \"healthy\" } when the service is running.")`, `.Produces<object>(200)` |

**Checkpoint**: Scalar UI shows all three endpoints with full documentation.

---

### Phase 6 — Polish & Cross-Cutting Concerns (T009, T007, T008)

**Goal**: Integration guardrail for non-Development Scalar behavior, regression confirmation, and quickstart validation.

| Task | File | Change |
|---|---|---|
| T009 | `tests/SpecKitApi.Tests/Integration/ScalarUiIntegrationTests.cs` | New file: integration test booting app with `ASPNETCORE_ENVIRONMENT=Production`; asserts `GET /scalar` returns `404 Not Found` (covers FR-003/SC-005, analysis gaps C1 and U1) |
| T007 | — | Run `dotnet test SpecKitApi.slnx`; confirm all existing tests pass with zero modifications to test code (satisfies FR-008) |
| T008 [P] | — | Manual verification: all `quickstart.md` scenarios — browser launch, OpenAPI JSON, Scalar UI executability, Production 404 behavior |

---

## Task Dependency Chain

```
T001 → T002 → T003
                    ↘
T004 (parallel)      → T009 → T007 → T008
                    ↗
T005 → T006
```

### Parallel Opportunities

- **T003 ‖ T004**: Different files (`Program.cs` vs `launchSettings.json`) — safe to execute in parallel
- **T007 ‖ T008**: Independent verification tasks — safe to run in parallel once T009 completes
- **T005 → T006**: Same file (`AlbumsEndpoints.cs`) — execute sequentially

## File Change Summary

| File | Task(s) | Change Type |
|---|---|---|
| `src/SpecKitApi/SpecKitApi.csproj` | T001 | Add `Scalar.AspNetCore` PackageReference |
| `src/SpecKitApi/Program.cs` | T002, T003 | Add `AddOpenApi("v1")`, `MapOpenApi()`, conditional `MapScalarApiReference()` |
| `src/SpecKitApi/Properties/launchSettings.json` | T004 | `launchUrl` `"albums"` → `"scalar"` |
| `src/SpecKitApi/Endpoints/AlbumsEndpoints.cs` | T005, T006 | Chain OpenAPI metadata onto `/albums` and `/health` mappings |
| `tests/SpecKitApi.Tests/Integration/ScalarUiIntegrationTests.cs` | T009 | New file: non-Development `/scalar` → `404` integration test |

**No changes to**: services, models, DTOs, middleware, clients, data-access code.

## Complexity Tracking

> No Constitution violations to justify. The design satisfies all five Core Principles as-is.

*(Table intentionally empty — no violations introduced.)*
