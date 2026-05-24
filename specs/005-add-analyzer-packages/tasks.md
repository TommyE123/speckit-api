# Tasks: Add Analyzer and Formatting Packages

**Spec**: `specs/005-add-analyzer-packages/spec.md`
**Plan**: `specs/005-add-analyzer-packages/plan.md`
**Branch**: `006-add-analyzer-packages`

**Input**: Design documents from `specs/005-add-analyzer-packages/`

**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅

**Tests**: No new test code is written for this feature. The existing xUnit test suite (`dotnet test SpecKitApi.slnx`) serves as the regression gate. Build-quality verification tasks (`dotnet build --no-incremental`) act as the acceptance mechanism for each user story (FR-006, SC-003).

**Organization**: Tasks are grouped by user story to enable independent delivery and verification.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no blocking dependencies between them)
- **[US1/US2/US3]**: Maps to the user story this task belongs to
- All tasks include exact file paths

---

## Phase 1: Setup (Shared Enforcement Infrastructure)

**Purpose**: Create the repository-level configuration files that underpin enforcement for all three user stories. All Phase 1 tasks must complete before any package additions begin.

- [ ] T001 Create `Directory.Build.props` at repository root with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` to apply shared build enforcement to both projects
- [ ] T002 [P] Remove redundant `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` from `src/SpecKitApi/SpecKitApi.csproj` (now inherited from `Directory.Build.props`)
- [ ] T003 [P] Create `.csharpierignore` at repository root with `**/obj/` and `**/bin/` exclusion patterns for generated MSBuild output directories
- [ ] T004 [P] Create `.editorconfig` at repository root with `root = true` and `generated_code = true` applied to `[**/obj/**]` glob to exclude generated files from Roslyn analyzer enforcement

---

## Phase 2: Foundational (Baseline Verification)

**Purpose**: Confirm the repository already builds and tests cleanly before introducing any packages. Establishes the starting quality baseline.

**⚠️ CRITICAL**: Both tasks must pass before any user story package additions begin.

- [ ] T005 [P] Run `dotnet build SpecKitApi.slnx --no-incremental` from repository root to confirm a clean zero-warning/zero-error baseline before adding packages
- [ ] T006 [P] Run `dotnet test SpecKitApi.slnx` from repository root to confirm all existing xUnit tests pass before making any changes

**Checkpoint**: Baseline is clean — package additions can now begin per-story.

---

## Phase 3: User Story 1 — Consistent Code Formatting Enforced (Priority: P1) 🎯 MVP

**Goal**: Add CSharpier.MsBuild to both projects, reformat all source files to CSharpier style in a single pass, and verify the solution builds with zero CSharpier diagnostics.

**Independent Test**: `dotnet build SpecKitApi.slnx --no-incremental` completes with zero CSharpier-related warnings or errors and zero total build output warnings.

### Implementation for User Story 1

- [ ] T007 [P] [US1] Add `CSharpier.MsBuild` 1.2.6 `<PackageReference>` with `<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>` and `<PrivateAssets>all</PrivateAssets>` to `src/SpecKitApi/SpecKitApi.csproj`
- [ ] T008 [P] [US1] Add `CSharpier.MsBuild` 1.2.6 `<PackageReference>` with identical `IncludeAssets` and `PrivateAssets=all` metadata to `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`
- [ ] T009 [US1] Run `dotnet restore` from repository root to resolve `CSharpier.MsBuild` 1.2.6 in both projects and verify the package appears in the restored dependency graph
- [ ] T010 [US1] Install CSharpier global tool via `dotnet tool install -g csharpier` if not present, then run `dotnet csharpier .` from repository root to reformat all source and test files in-place (respects `.csharpierignore`)
- [ ] T011 [US1] Run `dotnet build SpecKitApi.slnx --no-incremental` and fix any residual CSharpier formatting violations in `src/SpecKitApi/` and `tests/SpecKitApi.Tests/` .cs files — correct in source only, no `NoWarn` or `#pragma` suppression
- [ ] T012 [US1] Verify `dotnet build SpecKitApi.slnx --no-incremental` exits with zero CSharpier diagnostics and zero total warnings/errors (US1 acceptance gate — SC-001 partial)

**Checkpoint**: User Story 1 complete — CSharpier enforcement is active and all code is conformant.

---

## Phase 4: User Story 2 — Static Analysis Violations Resolved (Priority: P2)

**Goal**: Add all three Roslynator analyzer packages to both projects and fix every Roslynator diagnostic surfaced across the full codebase without using any suppression mechanism.

**Independent Test**: `dotnet build SpecKitApi.slnx --no-incremental` completes with zero Roslynator diagnostics and zero total warnings/errors after all violations are remediated. `dotnet test SpecKitApi.slnx` passes with 100% pass rate.

### Implementation for User Story 2

- [ ] T013 [P] [US2] Add `Roslynator.Analyzers` 4.15.0, `Roslynator.CodeAnalysis.Analyzers` 4.15.0, and `Roslynator.Formatting.Analyzers` 4.15.0 `<PackageReference>` entries each with `<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>` and `<PrivateAssets>all</PrivateAssets>` to `src/SpecKitApi/SpecKitApi.csproj`
- [ ] T014 [P] [US2] Add the same three Roslynator 4.15.0 `<PackageReference>` entries with identical metadata to `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`
- [ ] T015 [US2] Run `dotnet restore` to resolve all three Roslynator packages in both projects, then run `dotnet build SpecKitApi.slnx --no-incremental` to surface the full initial set of Roslynator diagnostics (catalogue all `RCS*` warnings before fixing)
- [ ] T016 [US2] Fix all Roslynator violations in `src/SpecKitApi/IJsonPlaceholderClient.cs` and `src/SpecKitApi/JsonPlaceholderClient.cs` — correct each `RCS*` diagnostic in source, no suppression
- [ ] T017 [US2] Fix all Roslynator violations in `src/SpecKitApi/IAlbumService.cs` and `src/SpecKitApi/AlbumService.cs` — correct each `RCS*` diagnostic in source, no suppression
- [ ] T018 [US2] Fix all Roslynator violations in `src/SpecKitApi/` DTO and model files (`AlbumDto.cs`, `AlbumResponse.cs`, `AlbumWithPhotosResponse.cs`, `PhotoDto.cs`, `PhotoResponse.cs`, `Album.cs`, `AlbumWithPhotos.cs`, `Photo.cs`, `ErrorResponse.cs`) — correct in source, no suppression
- [ ] T019 [US2] Fix all Roslynator violations in `src/SpecKitApi/AlbumsEndpoints.cs`, `src/SpecKitApi/ServiceCollectionExtensions.cs`, `src/SpecKitApi/CorrelationIdMiddleware.cs`, `src/SpecKitApi/JsonPlaceholderOptions.cs`, and `src/SpecKitApi/Program.cs` — correct in source, no suppression
- [ ] T020 [US2] Fix all Roslynator violations in `tests/SpecKitApi.Tests/` test and helper files (`JsonPlaceholderClientTests.cs`, `AlbumDtoTests.cs`, `PhotoDtoTests.cs`, `AlbumsEndpointsIntegrationTests.cs`, `AlbumServiceTests.cs`, `AlbumWithPhotosTests.cs`, `StubJsonPlaceholderClient.cs`, `SelfRegisteredExtensions.cs`) — correct in source, no suppression
- [ ] T021 [US2] Run `dotnet build SpecKitApi.slnx --no-incremental` and confirm zero Roslynator diagnostics (`RCS*`) and zero total warnings/errors across both projects (US2 build acceptance gate)
- [ ] T022 [US2] Run `dotnet test SpecKitApi.slnx` to confirm all existing tests pass at 100% after Roslynator remediation (US2 regression gate — SC-003 partial)

**Checkpoint**: User Story 2 complete — all Roslynator violations are resolved and tests remain green.

---

## Phase 5: User Story 3 — Code-Contract Annotations Available (Priority: P3)

**Goal**: Add JetBrains.Annotations to both projects so that nullability and flow-analysis attributes are available for use, with zero new build warnings or errors introduced.

**Independent Test**: `dotnet build SpecKitApi.slnx --no-incremental` exits with zero new warnings or errors introduced by `JetBrains.Annotations`. Package appears in the restored dependency graph for both projects.

### Implementation for User Story 3

- [ ] T023 [P] [US3] Add `JetBrains.Annotations` 2025.2.4 `<PackageReference>` with `<PrivateAssets>all</PrivateAssets>` to `src/SpecKitApi/SpecKitApi.csproj`
- [ ] T024 [P] [US3] Add `JetBrains.Annotations` 2025.2.4 `<PackageReference>` with `<PrivateAssets>all</PrivateAssets>` to `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`
- [ ] T025 [US3] Run `dotnet restore` to verify `JetBrains.Annotations` 2025.2.4 resolves in both projects and the package appears in both dependency graphs (SC-002)
- [ ] T026 [US3] Run `dotnet build SpecKitApi.slnx --no-incremental` and confirm zero new warnings or errors introduced by `JetBrains.Annotations` (US3 acceptance gate)

**Checkpoint**: User Story 3 complete — JetBrains.Annotations is resolvable in both projects with a clean build.

---

## Phase 6: Polish & Final Verification

**Purpose**: Cross-cutting quality checks and full-solution acceptance gates that confirm all success criteria are met end-to-end.

- [ ] T027 [P] Verify package parity — inspect `src/SpecKitApi/SpecKitApi.csproj` and `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` to confirm both files reference identical package IDs and exact version values for all five packages (SC-002, SC-006, FR-009)
- [ ] T028 [P] Verify zero suppression — confirm no `<NoWarn>` entries for CSharpier or Roslynator diagnostic IDs exist in any `.csproj` file, and no `#pragma warning disable` directives for `RCS*` or `CSharpier*` rules exist in any `.cs` file (SC-004, FR-005)
- [ ] T029 Run `dotnet build SpecKitApi.slnx --no-incremental` for the final full-solution clean-build acceptance gate — must exit with 0 errors and 0 warnings (SC-001)
- [ ] T030 Run `dotnet test SpecKitApi.slnx` for the final full test-suite acceptance gate — all tests must pass at 100% pass rate (SC-003)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No external dependencies — start immediately. T002, T003, T004 are parallel after T001 completes.
- **Foundational (Phase 2)**: Depends on Phase 1 completion. T005 and T006 run in parallel.
- **User Stories (Phases 3–5)**: All depend on Phase 2 completion. Stories can be worked in priority order (US1 → US2 → US3) or in parallel by separate developers.
- **Polish (Phase 6)**: Depends on all three user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 2. No dependency on US2 or US3.
- **User Story 2 (P2)**: Starts after Phase 2. Builds on the clean formatting baseline from US1 (recommended sequencing, not a hard dependency).
- **User Story 3 (P3)**: Starts after Phase 2. Fully independent of US1 and US2.

### Task-Level Dependency Chain

```
T001
 ├──> T002 [P]
 ├──> T003 [P]
 └──> T004 [P]
          │
          ▼ (all Phase 1 complete)
     T005 [P] ──┐
     T006 [P] ──┘
          │
          ▼ (baseline confirmed)

 US1:  T007 [P] ──┐
       T008 [P] ──┴──> T009 ──> T010 ──> T011 ──> T012
                                                      │
 US2:  T013 [P] ──┐                                  │
       T014 [P] ──┴──> T015 ──> T016 ──> T017        │
                               ──> T018 ──> T019      │ (recommended)
                               ──> T020 ──> T021 ──> T022
                                                      │
 US3:  T023 [P] ──┐                                  │
       T024 [P] ──┴──> T025 ──> T026                 │
                                  │                   │
                                  ▼ (all stories done)│
                          T027 [P] ──┐                │
                          T028 [P] ──┴──> T029 ──> T030
```

### Within Each User Story

- Package reference tasks (`T007`/`T008`, `T013`/`T014`, `T023`/`T024`) within a story are parallel (different files)
- Restore must follow package reference additions
- Violation-fixing tasks (`T016`–`T020`) can be parallelized across different file groups
- Build verification must follow all violation fixes

---

## Parallel Execution Examples

### Parallel Example: User Story 1

```powershell
# Both package additions are independent (different .csproj files):
# Terminal A:
#   Add CSharpier.MsBuild to src/SpecKitApi/SpecKitApi.csproj      (T007)
# Terminal B:
#   Add CSharpier.MsBuild to tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj  (T008)

# Then sequentially:
dotnet restore                   # T009
dotnet csharpier .               # T010
dotnet build SpecKitApi.slnx --no-incremental  # T011 + T012
```

### Parallel Example: User Story 2 (Violation Fixes)

```powershell
# Once T015 surfaces all diagnostics, remediation files are independent:
# Terminal A: Fix violations in IJsonPlaceholderClient.cs, JsonPlaceholderClient.cs  (T016)
# Terminal B: Fix violations in IAlbumService.cs, AlbumService.cs                   (T017)
# Terminal C: Fix violations in all DTO/model files                                  (T018)
# Terminal D: Fix violations in endpoints, middleware, options, Program.cs           (T019)
# Terminal E: Fix violations in all test files                                        (T020)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T004)
2. Complete Phase 2: Foundational baseline (T005–T006)
3. Complete Phase 3: User Story 1 — CSharpier formatting (T007–T012)
4. **STOP and VALIDATE**: Build passes with zero CSharpier warnings — US1 done ✅
5. Merge or demo before moving to US2/US3

### Incremental Delivery

1. Setup + Foundational → shared enforcement infrastructure ready
2. US1 complete → CSharpier formatting enforced, code is consistently styled ✅
3. US2 complete → all Roslynator diagnostics resolved, static quality enforced ✅
4. US3 complete → JetBrains.Annotations available for nullability annotations ✅
5. Each story adds a quality layer without breaking previous layers

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup (Phase 1) and Foundational (Phase 2) together
2. Once Foundational is confirmed clean:
   - Developer A: User Story 1 (CSharpier)
   - Developer B: User Story 3 (JetBrains.Annotations — lowest risk, can be done independently)
3. After US1 formatting is stable: Developer A moves to User Story 2 (Roslynator violations)
4. Stories merge independently in priority order

---

## Summary

| Phase | Tasks | Parallelizable | Story |
|---|---|---|---|
| Phase 1: Setup | T001–T004 | T002, T003, T004 after T001 | — |
| Phase 2: Foundational | T005–T006 | T005, T006 | — |
| Phase 3: US1 CSharpier | T007–T012 | T007, T008 | US1 |
| Phase 4: US2 Roslynator | T013–T022 | T013, T014; T016–T020 | US2 |
| Phase 5: US3 JetBrains | T023–T026 | T023, T024 | US3 |
| Phase 6: Polish | T027–T030 | T027, T028 | — |
| **Total** | **30 tasks** | **13 parallel opportunities** | |

### Acceptance Criteria Map

| Success Criterion | Verified by |
|---|---|
| SC-001: Zero errors and warnings on full build | T029 |
| SC-002: All five packages in both dependency graphs | T025, T027 |
| SC-003: 100% test-suite pass rate | T030 |
| SC-004: Zero NoWarn/pragma suppressions for CSharpier/Roslynator | T028 |
| SC-005: All pre-existing violations resolved | T021, T029 |
| SC-006: Exact version pins in both project files | T027 |
| SC-007: New violation causes build failure | Enforced by TreatWarningsAsErrors in T001 |
| SC-008: Generated files excluded without suppression | T003, T004 |
| SC-009: Same enforcement level in both projects | T013, T014, T027 |

---

## Notes

- `[P]` tasks modify different files with no shared state — safe to run in parallel
- `[US1/US2/US3]` labels trace each task to its user story for independent delivery
- The existing `<NoWarn>$(NoWarn);xUnit1051</NoWarn>` in `SpecKitApi.Tests.csproj` is **pre-approved** for an xUnit v3 diagnostic unrelated to this feature — preserve it throughout
- Roslynator violation counts are unknown until T015 runs; allocate additional remediation tasks if violations span files not listed in T016–T020
- CSharpier.MsBuild checks formatting at build time but does not reformat — always run `dotnet csharpier .` (T010) before the build verification step (T011–T012)
- Exact pinned versions (from research.md Decision 1): CSharpier.MsBuild=1.2.6, JetBrains.Annotations=2025.2.4, Roslynator.*=4.15.0
