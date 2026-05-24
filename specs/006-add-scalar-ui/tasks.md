---

description: "Task list for Add Scalar OpenAPI UI"
---

# Tasks: Add Scalar OpenAPI UI

**Input**: Design documents from `specs/006-add-scalar-ui/`

**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/openapi-scalar.md ✅, quickstart.md ✅

**Tests**: A targeted integration test (`ScalarUiIntegrationTests.cs`) is required for non-Development Scalar behavior (`/scalar` → `404`) in addition to full-suite regression verification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing. This feature is purely additive — no business logic, service, or data-access changes (FR-009).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no shared dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- All file paths are relative to the repository root

---

## Phase 1: Setup (Project Configuration)

**Purpose**: Add the single new NuGet dependency required by the feature. This is a project-file-only change and does not modify application code.

- [X] T001 Add `<PackageReference Include="Scalar.AspNetCore" Version="2.*" />` to `src/SpecKitApi/SpecKitApi.csproj` inside the existing `<ItemGroup>` block, then run `dotnet restore SpecKitApi.slnx` from the repository root to confirm the package resolves successfully

---

## Phase 2: Foundational (Blocking Prerequisite)

**Purpose**: Register the OpenAPI document generation service in the DI container. This MUST be in place before middleware routing or endpoint metadata annotations can be rendered into the OpenAPI document.

**⚠️ CRITICAL**: US1 middleware wiring (T003) depends on this service registration completing first.

- [X] T002 Register the named OpenAPI document in `src/SpecKitApi/Program.cs`: add `builder.Services.AddOpenApi("v1");` on a new line immediately after `builder.Services.AddJsonPlaceholderServices(...)` and before `var app = builder.Build();`

**Checkpoint**: DI registration is in place — user story implementation can now proceed.

---

## Phase 3: User Story 1 — Launch Interactive API Documentation (Priority: P1) 🎯 MVP

**Goal**: A developer runs the application locally, the browser opens automatically to `/scalar`, and a fully interactive Scalar UI is presented. The OpenAPI JSON document is available at `/openapi/v1.json` in all environments; the Scalar UI is mounted only in Development.

**Independent Test**: Run `dotnet run --project src/SpecKitApi/SpecKitApi.csproj` (Development profile). Verify the browser opens to `/scalar`, the Scalar UI renders, and `GET /openapi/v1.json` returns OpenAPI JSON. Then run with `ASPNETCORE_ENVIRONMENT=Production` and verify `/scalar` returns `404 Not Found` while `/openapi/v1.json` remains reachable.

### Implementation for User Story 1

- [X] T003 [US1] Wire OpenAPI and Scalar middleware in `src/SpecKitApi/Program.cs`: after `app.UseMiddleware<CorrelationIdMiddleware>()` and before `app.MapAlbums()`, add `app.MapOpenApi();` unconditionally so `/openapi/v1.json` is reachable in all environments (satisfies FR-001), then add `if (app.Environment.IsDevelopment()) { app.MapScalarApiReference(); }` to mount the Scalar UI at `/scalar` in Development only (satisfies FR-002, FR-003); the default `MapScalarApiReference()` call with no arguments routes to `/scalar` and references `/openapi/v1.json` automatically

- [X] T004 [P] [US1] Update `src/SpecKitApi/Properties/launchSettings.json`: in the `"SpecKitApi (Development)"` profile, change `"launchUrl": "albums"` to `"launchUrl": "scalar"` so that `dotnet run` opens the browser at `/scalar` automatically on startup (satisfies FR-004)

**Checkpoint**: User Story 1 is fully functional — Scalar UI accessible in Development, 404 in Production, OpenAPI JSON available everywhere, browser opens to `/scalar` on launch.

---

## Phase 4: User Story 2 — Test Endpoints via Scalar UI (Priority: P2)

**Goal**: A developer uses the Scalar UI to interactively test `GET /albums` and `GET /albums?userId={id}`. The OpenAPI document describes the `userId` query parameter as an optional integer and documents response codes `200`, `400`, and `500` for the `/albums` endpoint.

**Independent Test**: Open Scalar UI at `/scalar`. Verify `GET /albums` is listed with a human-readable summary. Execute `GET /albums` with no parameters and confirm a JSON array is returned. Enter a valid integer `userId` and execute; confirm filtered results. Enter `abc` as `userId` and execute; confirm `400 Bad Request` with error body is displayed. Verify the operation shows documented response codes `200`, `400`, and `500`.

### Implementation for User Story 2

- [X] T005 [US2] Add OpenAPI metadata to the `/albums` endpoint in `src/SpecKitApi/Endpoints/AlbumsEndpoints.cs`: chain the following onto `app.MapGet("/albums", ...)` — `.WithName("GetAlbums")`, `.WithSummary("Get all albums with photos, optionally filtered by user")`, `.WithDescription("Returns all albums with their associated photos. Supply the optional userId query parameter (positive integer) to filter results to a specific user.")`, `.Produces<IReadOnlyList<AlbumWithPhotosResponse>>(200)`, `.ProducesProblem(400)`, `.ProducesProblem(500)`, and endpoint OpenAPI parameter transformation to mark `userId` as optional integer despite runtime `string?` binding for manual validation (satisfies FR-005, FR-006, FR-007)

**Checkpoint**: User Story 2 is fully testable — `/albums` shows enriched docs in Scalar UI with parameter description and response code table.

---

## Phase 5: User Story 3 — Health Endpoint Visible in Documentation (Priority: P3)

**Goal**: The `GET /health` endpoint appears as a documented, executable operation in the Scalar UI alongside the albums endpoints, giving operators a single place to verify both API surface and liveness.

**Independent Test**: Open Scalar UI at `/scalar`. Locate `GET /health` in the endpoint list; verify it shows a human-readable summary and is individually executable. Execute `GET /health` from the UI; confirm `200 OK` with body `{ "status": "healthy" }` is displayed.

### Implementation for User Story 3

- [X] T006 [US3] Add OpenAPI metadata to the `/health` endpoint in `src/SpecKitApi/Endpoints/AlbumsEndpoints.cs`: chain `.WithName("GetHealth")`, `.WithSummary("Service liveness check")`, `.WithDescription("Returns 200 OK with { \"status\": \"healthy\" } when the service is running.")`, and `.Produces<object>(200)` onto the `app.MapGet("/health", ...)` call (satisfies FR-005); edit this file sequentially after T005 to avoid conflicts since both tasks modify the same file

**Checkpoint**: All three user stories are independently functional — Scalar UI presents all endpoints with full documentation.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Write the non-Development Scalar integration test, run full regression, and complete quickstart validation to satisfy FR-008 and the Definition of Done in `quickstart.md`.

- [X] T009 Create a new file `tests/SpecKitApi.Tests/Integration/ScalarUiIntegrationTests.cs` containing an xUnit integration test that uses `WebApplicationFactory<Program>` to boot the application with `ASPNETCORE_ENVIRONMENT=Production` and asserts that `GET /scalar` returns `404 Not Found`; this test guards FR-003 (Scalar unmapped outside Development) and SC-005 (Production `/scalar` → 404) and addresses analysis gaps C1 and U1 identified in the plan

- [X] T007 Run `dotnet test SpecKitApi.slnx` from the repository root and confirm all tests pass — including the new `ScalarUiIntegrationTests` and the existing `AlbumsEndpointsIntegrationTests` — with zero modifications to existing test code (satisfies FR-008); a clean test run here is the automated gate for feature correctness

- [X] T008 [P] Manually execute all `quickstart.md` verification scenarios: (1) `dotnet run` opens browser to `/scalar` within the SC-001 timing threshold and the UI renders interactively; (2) `GET /openapi/v1.json` returns OpenAPI JSON with summaries for `/albums` and `/health`; (3) all three endpoints are individually executable from the Scalar UI and return correct status codes and bodies; (4) running with `ASPNETCORE_ENVIRONMENT=Production` returns `404` for `/scalar` while `/openapi/v1.json` remains reachable at `200` (satisfies SC-001 through SC-007)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1 — T001)**: No dependencies. Can start immediately.
- **Foundational (Phase 2 — T002)**: Depends on T001 (package must resolve before dotnet build). Blocks T003, T005, T006.
- **US1 (Phase 3 — T003, T004)**: T003 depends on T002. T004 (launchSettings) has no code dependency on T002/T003 — can run in parallel with T003.
- **US2 (Phase 4 — T005)**: Depends on T002 (AddOpenApi must be registered). Can be written while T003 is in progress; Scalar needed to visually verify results.
- **US3 (Phase 5 — T006)**: Same dependency as US2. T005 and T006 modify the same file (`AlbumsEndpoints.cs`) — execute T006 sequentially after T005.
- **Polish (Phase 6 — T009, T007, T008)**: Depends on all preceding phases complete.

### User Story Dependencies

| Story | Depends On | Notes |
|---|---|---|
| US1 (P1) | Phase 1 + Phase 2 complete | Core infrastructure for UI delivery |
| US2 (P2) | Phase 2 complete (T002) | Metadata renders via AddOpenApi; Scalar (T003) needed to visually verify |
| US3 (P3) | Phase 2 complete (T002) | Same file as US2 — execute T006 after T005 to avoid conflict |

### Task-Level Dependency Chain

```
T001 → T002 → T003
                     ↘
T004 (parallel)       → T009 → T007 → T008
                     ↗
T005 → T006
```

### Parallel Opportunities

- **T003 ‖ T004**: Different files (`Program.cs` vs `launchSettings.json`) — safe to execute simultaneously by two developers
- **T007 ‖ T008**: Independent verification tasks — safe to run in parallel once T009 is complete
- **T005 → T006**: Same file (`AlbumsEndpoints.cs`) — execute sequentially to avoid conflicts

---

## Parallel Example: User Story 1

```powershell
# Two developers can tackle US1 tasks simultaneously:

# Developer A — Program.cs middleware wiring (T003):
# Add MapOpenApi() + conditional MapScalarApiReference() to src/SpecKitApi/Program.cs

# Developer B — launchSettings update (T004):
# Change launchUrl from "albums" to "scalar" in src/SpecKitApi/Properties/launchSettings.json
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Add NuGet package (T001)
2. Complete Phase 2: Register AddOpenApi service (T002)
3. Complete Phase 3: Wire middleware + update launch URL (T003, T004)
4. **STOP and VALIDATE**: Run the app, confirm Scalar UI loads at `/scalar`, confirm `/openapi/v1.json` returns JSON, confirm Production returns 404 for `/scalar`
5. Run `dotnet test SpecKitApi.slnx` — all tests should pass at this point

### Incremental Delivery

1. Complete Phase 1 + Phase 2 → Foundation ready
2. Complete Phase 3 (US1) → Scalar UI live, `/openapi/v1.json` available → **Validate independently → Demo/Deploy**
3. Complete Phase 4 (US2) → `/albums` fully documented with parameter and response codes → **Validate independently**
4. Complete Phase 5 (US3) → `/health` documented → **Validate independently**
5. Complete Phase 6 → integration guardrail + regression + quickstart confirmed → **Feature complete**

### Parallel Team Strategy

With two developers available after Phase 1 + Phase 2:

- Developer A: T003 (Program.cs middleware)
- Developer B: T004 (launchSettings) then T005 (albums metadata)

Single developer: follow T001 → T002 → T003 → T004 → T005 → T006 → T009 → T007 → T008 sequentially.

---

## File Change Summary

| File | Task(s) | Change Type |
|---|---|---|
| `src/SpecKitApi/SpecKitApi.csproj` | T001 | Add `Scalar.AspNetCore` PackageReference |
| `src/SpecKitApi/Program.cs` | T002, T003 | Add `AddOpenApi("v1")`, `MapOpenApi()`, conditional `MapScalarApiReference()` |
| `src/SpecKitApi/Properties/launchSettings.json` | T004 | Change `launchUrl` from `"albums"` to `"scalar"` |
| `src/SpecKitApi/Endpoints/AlbumsEndpoints.cs` | T005, T006 | Chain OpenAPI metadata onto `/albums` and `/health` mappings |
| `tests/SpecKitApi.Tests/Integration/ScalarUiIntegrationTests.cs` | T009 | **New file**: non-Development `/scalar` → `404` integration test |

**No changes to**: services, models, DTOs, middleware, clients, or data-access code.

---

## Notes

- [P] tasks = different files, no shared write dependencies — safe for parallel execution
- [Story] label maps each task to a specific user story for traceability
- FR-009 is enforced by design — all changes are additive metadata/wiring only
- T005 requires overriding the inferred `string` type for `userId` to `integer` via `.WithOpenApi()` transformer, since the handler binds `string? userId` for manual validation (see research.md Decision 3)
- T009 creates a brand-new file `ScalarUiIntegrationTests.cs` — it does NOT modify `AlbumsEndpointsIntegrationTests.cs`
- Scalar's default route is `/scalar` when called as `app.MapScalarApiReference()` with no arguments — no custom route configuration required
- Commit after each phase or logical group using feature branch `006-add-scalar-ui`
- Stop at the Phase 3 checkpoint to validate US1 independently before proceeding to US2/US3
