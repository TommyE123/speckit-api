---
description: "Task list for JSONPlaceholder Core Data Layer"
---

# Tasks: JSONPlaceholder Core Data Layer

**Feature Branch**: `001-jsonplaceholder-core`

**Input**: Design documents from `specs/001-jsonplaceholder-core/`

**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅

**Tests**: Test tasks are MANDATORY per Principle III (Test-First, NON-NEGOTIABLE). Tests MUST be written and confirmed failing before implementation begins (Red-Green-Refactor). Do not skip or defer test tasks.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Exact file paths are included in all descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solution and project scaffolding. No business logic — just a compilable skeleton.

- [x] T001 Create solution file `SpecKitApi.sln` at repository root with `dotnet new sln -n SpecKitApi`
- [x] T002 Create source project `src/SpecKitApi/SpecKitApi.csproj` targeting `net10.0` and add NuGet packages: `Microsoft.Extensions.Http`, `Microsoft.Extensions.Http.Polly` via `dotnet add package`
- [x] T003 Create test project `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` targeting `net10.0` and add NuGet packages: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `Moq`, `coverlet.collector` via `dotnet add package`
- [x] T004 Add `tests/SpecKitApi.Tests` → `src/SpecKitApi` project reference and add both projects to `SpecKitApi.sln`
- [x] T005 [P] Create `src/SpecKitApi/appsettings.json` with `JsonPlaceholderOptions:BaseUrl` set to `https://jsonplaceholder.typicode.com`
- [x] T006 [P] Create `src/SpecKitApi/Options/JsonPlaceholderOptions.cs` — sealed POCO with `SectionName` constant and `BaseUrl` string property

**Checkpoint**: `dotnet build` succeeds with zero warnings and zero errors on the empty project.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: All type definitions — DTOs, domain models, and interface stubs — that every subsequent phase depends on. No logic; just shapes.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T007 [P] Create `src/SpecKitApi/DTOs/AlbumDto.cs` — sealed record/class with `[JsonPropertyName]` attributes: `Id` (int), `UserId` (int), `Title` (string) per data-model.md
- [x] T008 [P] Create `src/SpecKitApi/DTOs/PhotoDto.cs` — sealed record/class with `[JsonPropertyName]` attributes: `Id`, `AlbumId`, `Title`, `Url`, `ThumbnailUrl` (all `[JsonPropertyName]` annotated) per data-model.md
- [x] T009 [P] Create `src/SpecKitApi/Models/Album.cs` — sealed record/class: `Id` (int), `UserId` (int), `Title` (string) per data-model.md
- [x] T010 [P] Create `src/SpecKitApi/Models/Photo.cs` — sealed record/class: `Id` (int), `AlbumId` (int), `Title` (string), `ImageUrl` (string), `ThumbnailUrl` (string) per data-model.md
- [x] T011 [P] Create `src/SpecKitApi/Models/AlbumWithPhotos.cs` — sealed record/class: `Album` (Album), `Photos` (IReadOnlyList<Photo>) per data-model.md
- [x] T012 [P] Create `src/SpecKitApi/Clients/IJsonPlaceholderClient.cs` — interface with `GetAlbumsAsync(CancellationToken)` → `Task<IReadOnlyList<AlbumDto>>` and `GetPhotosAsync(CancellationToken)` → `Task<IReadOnlyList<PhotoDto>>`
- [x] T013 [P] Create `src/SpecKitApi/Services/IAlbumService.cs` — interface with `GetAlbumsWithPhotosAsync(CancellationToken)` → `Task<IReadOnlyList<AlbumWithPhotos>>` and `GetAlbumsWithPhotosByUserAsync(int userId, CancellationToken)` → `Task<IReadOnlyList<AlbumWithPhotos>>`

**Checkpoint**: Foundation ready — `dotnet build` succeeds. All types and interfaces exist. User story implementation can now begin.

---

## Phase 3: User Story 3 — Structured Data Representation (Priority: P3) 🎯 MVP Prerequisite

> **Note**: Although P3 in the spec, this is implemented first because the DTO and model types are already defined in Phase 2. This phase validates the mapping contract with tests before the service logic is layered on in US1.

**Goal**: Prove that DTOs deserialise correctly from raw JSON and map into domain models with all fields intact (including the `Url` → `ImageUrl` rename).

**Independent Test**: Assert that a JSON snippet representative of the `/albums` and `/photos` responses deserialises into `AlbumDto`/`PhotoDto` with all field values preserved, and that the manual mapping expressions produce `Album`/`Photo`/`AlbumWithPhotos` values where every field matches. All three acceptance scenarios in US3 are covered.

### Tests for User Story 3 (write first — confirm failing ✅)

> **Write these tests FIRST. Confirm they FAIL before writing any implementation.**

- [x] T014 [P] [US3] Write DTO deserialization tests in `tests/SpecKitApi.Tests/DTOs/AlbumDtoTests.cs` — assert `AlbumDto` deserialises `{ "id":1, "userId":2, "title":"t" }` to correct field values
- [x] T015 [P] [US3] Write DTO deserialization tests in `tests/SpecKitApi.Tests/DTOs/PhotoDtoTests.cs` — assert `PhotoDto` deserialises all five fields including `thumbnailUrl` → `ThumbnailUrl` and `url` → `Url`
- [x] T016 [P] [US3] Write domain model structural tests in `tests/SpecKitApi.Tests/Models/AlbumWithPhotosTests.cs` — assert `AlbumWithPhotos` invariant: every `Photo` in `Photos` shares `AlbumId` with parent `Album.Id`

### Implementation for User Story 3

- [x] T017 [US3] Verify `AlbumDto` and `PhotoDto` fields compile and JSON attributes are correct in `src/SpecKitApi/DTOs/` (update from T007/T008 stubs if needed)
- [x] T018 [US3] Verify `Album`, `Photo`, `AlbumWithPhotos` constructors/properties are complete in `src/SpecKitApi/Models/` (update from T009/T010/T011 stubs if needed)

**Checkpoint**: T014–T016 all pass. Data shape contract is verified. US3 acceptance scenarios satisfied.

---

## Phase 4: User Story 1 — Fetch Albums with Associated Photos (Priority: P1)

**Goal**: A caller can invoke `IAlbumService.GetAlbumsWithPhotosAsync()` and receive every album populated with exactly its correct photos. All three US1 acceptance scenarios are satisfied. No live HTTP calls in tests.

**Independent Test**: Mock `IJsonPlaceholderClient` with known album/photo data; assert each `AlbumWithPhotos.Photos` contains only photos whose `AlbumId` matches the parent album, that no albums or photos are missing or duplicated, and that an exception from the client propagates to the caller.

### Tests for User Story 1 (write first — confirm failing ✅)

> **Write these tests FIRST. Confirm they FAIL before writing any implementation.**

- [x] T019 [P] [US1] Write `JsonPlaceholderClientTests` in `tests/SpecKitApi.Tests/Clients/JsonPlaceholderClientTests.cs` with 6 test cases per plan.md Phase 2a — use `MockHttpMessageHandler` (hand-rolled `DelegatingHandler`): correct deserialization for albums and photos, correct endpoint URIs, and exception propagation on HTTP 500
- [x] T020 [P] [US1] Write `AlbumServiceTests` (combine scenarios) in `tests/SpecKitApi.Tests/Services/AlbumServiceTests.cs` with 6 combine/mapping test cases per plan.md Phase 2b rows 1–5 and mapping row: correct photo assignment, no cross-album leakage, empty albums, empty photos, `PhotoDto.Url` → `Photo.ImageUrl`, full DTO field mapping

### Implementation for User Story 1

- [x] T021 [US1] Implement `src/SpecKitApi/Clients/JsonPlaceholderClient.cs` — sealed class implementing `IJsonPlaceholderClient` using injected `HttpClient`, `GetFromJsonAsync<List<AlbumDto>>("/albums")` and `GetFromJsonAsync<List<PhotoDto>>("/photos")`, returning empty list on null per plan.md Architecture A1
- [x] T022 [US1] Implement `AlbumService.GetAlbumsWithPhotosAsync` in `src/SpecKitApi/Services/AlbumService.cs` — fetch both via `Task.WhenAll`, build `Dictionary<int, List<Photo>>` keyed by `albumId`, map DTOs to domain models inline, return `IReadOnlyList<AlbumWithPhotos>` per plan.md Architecture A3/A4

**Checkpoint**: T019–T020 all pass. `GetAlbumsWithPhotosAsync` is fully functional and independently testable. US1 acceptance scenarios satisfied.

---

## Phase 5: User Story 2 — Filter Combined Results by User ID (Priority: P2)

**Goal**: A caller can invoke `IAlbumService.GetAlbumsWithPhotosByUserAsync(userId)` and receive only that user's albums with their photos. Empty results for unknown/zero user IDs — no exceptions.

**Independent Test**: Reuse the mocked client from US1 tests; call `GetAlbumsWithPhotosByUserAsync` with a known user ID and assert only that user's albums appear; call with an unknown ID and assert empty result.

### Tests for User Story 2 (write first — confirm failing ✅)

> **Write these tests FIRST. Confirm they FAIL before writing any implementation.**

- [x] T023 [P] [US2] Extend `AlbumServiceTests` in `tests/SpecKitApi.Tests/Services/AlbumServiceTests.cs` with 4 filter test cases per plan.md Phase 2b rows 6–9: filter returns only matching user's albums, unknown user ID returns empty, user ID with no albums returns empty, filter preserves correct photos per album

### Implementation for User Story 2

- [x] T024 [US2] Implement `AlbumService.GetAlbumsWithPhotosByUserAsync` in `src/SpecKitApi/Services/AlbumService.cs` — call `GetAlbumsWithPhotosAsync()` then `.Where(a => a.Album.UserId == userId).ToList()`, return empty list (not error) when no match per FR-006 and plan.md A3

**Checkpoint**: T023 passes. `GetAlbumsWithPhotosByUserAsync` is functional. US2 acceptance scenarios satisfied. All 10 `AlbumServiceTests` pass.

---

## Phase 6: DI Registration & Configuration

**Purpose**: Wire the typed HTTP client, Polly resilience policies, and service into the DI container so the project is ready for feature 002 endpoints.

- [x] T025 Create `src/SpecKitApi/Extensions/ServiceCollectionExtensions.cs` — static extension method `AddJsonPlaceholderServices(IServiceCollection, IConfiguration)` that: binds `JsonPlaceholderOptions` from config, registers `IJsonPlaceholderClient`/`JsonPlaceholderClient` typed client with base address from options, attaches Polly 3-retry exponential backoff (1s/2s/4s) and 10-second timeout per plan.md Architecture A2, and registers `IAlbumService`/`AlbumService` as scoped per plan.md Phase 4b

**Checkpoint**: `dotnet build` succeeds. DI extension is ready; `Program.cs` will call it in feature 002.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Code quality, documentation, and final validation.

- [x] T026 [P] Add `/// <summary>` XML doc comments to all public interfaces (`IJsonPlaceholderClient`, `IAlbumService`) and their methods in `src/SpecKitApi/Clients/` and `src/SpecKitApi/Services/`
- [x] T027 [P] Add XML doc comments to all domain model and DTO types in `src/SpecKitApi/Models/` and `src/SpecKitApi/DTOs/`
- [x] T028 Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` and `<Nullable>enable</Nullable>` in `src/SpecKitApi/SpecKitApi.csproj`; fix any nullable warnings introduced
- [x] T029 Enable `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `src/SpecKitApi/SpecKitApi.csproj`; resolve all warnings to achieve clean build
- [x] T030 Run `dotnet test --configuration Release` and confirm: all tests pass, zero failures, zero skipped, total run time under 5 seconds (SC-004)
- [x] T031 Run quickstart.md validation — execute `dotnet restore`, `dotnet build --configuration Release`, `dotnet test --configuration Release --no-build` on a clean workspace and confirm all steps complete without error

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — **BLOCKS all user story phases**
- **Phase 3 (US3 — DTOs/Models)**: Depends on Phase 2 — types are already defined; tests validate the shapes
- **Phase 4 (US1 — Combine)**: Depends on Phase 2 — can start after foundation (Phase 3 optional but recommended first)
- **Phase 5 (US2 — Filter)**: Depends on Phase 4 (`GetAlbumsWithPhotosAsync` must exist first)
- **Phase 6 (DI)**: Depends on Phase 4 + Phase 5 — all service logic must be implemented
- **Phase 7 (Polish)**: Depends on Phases 1–6

### User Story Dependencies

- **US3 (P3)**: After Phase 2 — validates type shapes; no logic dependency
- **US1 (P1)**: After Phase 2 — core service logic; independent
- **US2 (P2)**: After US1 (P1) — calls `GetAlbumsWithPhotosAsync` internally

### Within Each User Story

1. Write tests → confirm all fail
2. Implement to make tests pass (Red → Green)
3. Refactor if needed (keep tests green)
4. Verify checkpoint before moving on

### Parallel Opportunities

- T007–T013 (all Foundational types) can all be written in parallel
- T014, T015, T016 (US3 tests) can be written in parallel
- T019, T020 (US1 tests) can be written in parallel
- T026, T027 (XML doc comments) can be written in parallel
- US3 and US1 phases can be worked in parallel once Foundation is complete

---

## Parallel Example: Phase 2 (Foundation)

```
# All type definitions are independent files — write in parallel:
T007: src/SpecKitApi/DTOs/AlbumDto.cs
T008: src/SpecKitApi/DTOs/PhotoDto.cs
T009: src/SpecKitApi/Models/Album.cs
T010: src/SpecKitApi/Models/Photo.cs
T011: src/SpecKitApi/Models/AlbumWithPhotos.cs
T012: src/SpecKitApi/Clients/IJsonPlaceholderClient.cs
T013: src/SpecKitApi/Services/IAlbumService.cs
```

## Parallel Example: Phase 4 (US1 Tests)

```
# Both test classes are independent files — write in parallel:
T019: tests/SpecKitApi.Tests/Clients/JsonPlaceholderClientTests.cs
T020: tests/SpecKitApi.Tests/Services/AlbumServiceTests.cs (combine cases)
```

---

## Implementation Strategy

### MVP First (US1 Only — Phases 1–4)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 4: US1 (fetch + combine) — core value of the feature
4. **STOP and VALIDATE**: `dotnet test` — all US1 tests pass
5. Deliver US1 as the working increment

### Incremental Delivery

1. Phase 1 + Phase 2 → skeleton compiles
2. Phase 3 (US3) → type shapes validated (optional, can be done alongside US1)
3. Phase 4 (US1) → combine logic working, all US1 tests green
4. Phase 5 (US2) → filter logic working, all US2 tests green
5. Phase 6 (DI) → ready for feature 002 endpoints
6. Phase 7 (Polish) → production-quality code, zero warnings

---

## Notes

- `[P]` tasks operate on different files with no shared in-flight dependencies
- `[US1]`, `[US2]`, `[US3]` labels map tasks to user stories in spec.md for full traceability
- No live network calls during `dotnet test` — all HTTP is mocked via `IJsonPlaceholderClient` stub
- `System.Text.Json` only — Newtonsoft.Json is prohibited by constitution
- Base URL sourced from `appsettings.json` — never hardcoded (constitution mandate)
- No `Endpoints/` folder, no `Program.cs` — deferred to feature 002
- Each checkpoint is a valid stopping point; tests must be green before advancing
