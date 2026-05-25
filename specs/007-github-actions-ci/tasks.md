---

description: "Task list for GitHub Actions CI Build Workflow"
---

# Tasks: GitHub Actions CI Build Workflow

**Input**: Design documents from `specs/007-github-actions-ci/`

**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, quickstart.md ✅

**Tests**: TDD is EXEMPT for this feature — the sole deliverable is a YAML configuration file with no unit-testable application logic (Principle III exemption confirmed in plan.md). Acceptance validation is by live GitHub Actions execution against spec.md acceptance scenarios.

**Organization**: Tasks are grouped by phase. All tasks produce changes to the single deliverable file `.github/workflows/build.yml`.

## Format: `[ID] [P?] [Story?] Description with file path`

- **[P]**: Can run in parallel (different files/concerns, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3)
- All file paths are relative to the repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the workflow directory structure and base workflow skeleton that all user stories build upon.

- [X] T001 Create directory `.github/workflows/` at repository root (FR-010)
- [X] T002 Create `.github/workflows/build.yml` with workflow name (`Build`), `on:` trigger block, and `build` job definition with `runs-on: ubuntu-latest` (FR-010)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared runner setup steps that MUST exist before any user story build logic can function.

**⚠️ CRITICAL**: No user story work can begin until these steps are in place.

- [X] T003 Add `Checkout` step using `actions/checkout@v6.0.2` (pinned patch version) to `.github/workflows/build.yml` (FR-018)
- [X] T004 Add `Setup .NET` step (`actions/setup-dotnet@v5.2.0` with `dotnet-version: '10.0.x'`) to `.github/workflows/build.yml` (FR-003)
- [X] T005 Add workflow-level `env:` block with `DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true`, `DOTNET_CLI_TELEMETRY_OPTOUT: true`, and `NUGET_PACKAGES: ${{ github.workspace }}/.nuget/packages` to `.github/workflows/build.yml` (FR-019)

**Checkpoint**: Foundation ready — all user story phases can now proceed in sequence.

---

## Phase 3: User Story 1 — Developer Pushes Code to Main (Priority: P1) 🎯 MVP

**Goal**: Deliver a working CI pipeline triggered by pushes to `main` that restores, builds, and tests the solution — the core protection against broken code reaching `main`.

**Independent Test**: Push a commit to `main` and verify the Actions tab shows a `Build` run that completes the `Restore`, `Build`, and `Test` steps in sequence (spec.md acceptance scenarios 1–3 for US1).

### Implementation for User Story 1

- [X] T006 [US1] Add `push` trigger with `branches: [ main ]` to the `on:` block in `.github/workflows/build.yml` (FR-001)
- [X] T007 [US1] Add `Restore` step (`run: dotnet restore`) to `.github/workflows/build.yml` (FR-004)
- [X] T008 [US1] Add `Build` step (`run: dotnet build --configuration Release --no-restore --warnaserror`) to `.github/workflows/build.yml` (FR-005, FR-008; `--warnaserror` required by spec; `Directory.Build.props` also enforces `TreatWarningsAsErrors=true` at the MSBuild level — the CLI flag is redundant but mandatory per FR-008)
- [X] T009 [US1] Add `Test` step (`run: dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage" --logger trx --results-directory ./TestResults`) to `.github/workflows/build.yml` (FR-006, FR-007, FR-012)

**Checkpoint**: User Story 1 fully functional — push to `main` runs restore → build → test and fails correctly on a test failure or build warning.

---

## Phase 4: User Story 2 — Developer Opens a Pull Request to Main (Priority: P2)

**Goal**: Extend the workflow to trigger on PRs, publish inline test results via Check Run (same-repo PRs) or Job Summary (fork PRs), generate and upload a code coverage report, write a coverage summary to the Actions job summary, and post a sticky PR comment with the coverage summary.

**Independent Test**: Open a pull request targeting `main` and verify: (1) the workflow triggers, (2) the PR Checks section shows a `Test Results` Check Run (same-repo) or the Actions Job Summary shows test results (fork PR), (3) `test-results` and `coverage-report` artifacts appear on the run summary, (4) a sticky PR comment with coverage summary is posted (spec.md acceptance scenarios 1–2 for US2).

### Implementation for User Story 2

- [X] T010 [US2] Add `pull_request` trigger with `branches: [ main ]` to the `on:` block in `.github/workflows/build.yml` (FR-002)
- [X] T011 [US2] Add `permissions` block (`contents: read`, `checks: write`, `actions: read`, `pull-requests: write`) to the `build` job in `.github/workflows/build.yml` (required by `dorny/test-reporter@v3.0.0` for Check Run creation on non-fork PRs; `pull-requests: write` required for sticky PR comment; FR-017, FR-021)
- [X] T012 [US2] Add `Upload Test Results` step (`actions/upload-artifact@v7.0.1`, `name: test-results`, `path: TestResults/`, `if: always()`) to `.github/workflows/build.yml` (FR-015, FR-016)
- [X] T013 [US2] Add `Publish Test Results` step (`dorny/test-reporter@v3.0.0`, `name: Test Results`, `path: TestResults/**/*.trx`, `reporter: dotnet-trx`, `if: always()`) immediately after T012 in `.github/workflows/build.yml` (FR-017; `fail-on-error: true` default fails workflow if any test failed; `fail-on-empty: true` default fails if no TRX files found; Job Summary written for all PRs including forks)
- [X] T014 [US2] Add `Generate Coverage Report` step (`danielpalme/ReportGenerator-GitHub-Action@5.5.10`, `reports: '**/coverage.cobertura.xml'`, `targetdir: 'coveragereport'`, `assemblyfilters: '-*.Tests*'`, `verbosity: 'Warning'`, `reporttypes: 'HtmlInline;Cobertura;MarkdownSummaryGithub'`, `if: always()`) to `.github/workflows/build.yml` (FR-013, FR-015; glob required because XPlat Code Coverage writes to a random GUID subdirectory)
- [X] T015 [US2] Add `Write Coverage to Job Summary` step (`run: cat coveragereport/SummaryGithub.md >> $GITHUB_STEP_SUMMARY`, `if: always()`) immediately after T014 in `.github/workflows/build.yml` (FR-020)
- [X] T016 [US2] Add `Upload Coverage Report` step (`actions/upload-artifact@v7.0.1`, `name: coverage-report`, `path: coveragereport/`, `retention-days: 14`, `if: always()`) to `.github/workflows/build.yml` (FR-014, FR-015)
- [X] T017 [US2] Add `Post Coverage Summary PR Comment` step (`marocchino/sticky-pull-request-comment@v3.0.4`, `if: github.event_name == 'pull_request'`, `recreate: true`, `path: coveragereport/SummaryGithub.md`) to `.github/workflows/build.yml` (FR-022)

**Checkpoint**: User Stories 1 AND 2 fully functional — push and PR triggers both work, inline test results visible on PRs, both artifacts downloadable from run summary, sticky PR coverage comment posted.

---

## Phase 5: User Story 3 — Developer Benefits from Cached NuGet Packages (Priority: P3)

**Goal**: Insert the NuGet cache step between `Setup .NET` and `Restore` so subsequent workflow runs skip re-downloading packages from NuGet.org, reducing CI feedback time.

**Independent Test**: Trigger two consecutive workflow runs on the same branch with no `.csproj` changes. In the second run, the `Cache NuGet packages` step should show `Cache restored from key: Linux-nuget-<hash>` and the `Restore` step should complete faster than the first run (spec.md acceptance scenarios 1–2 for US3; SC-006).

### Implementation for User Story 3

- [X] T018 [US3] Insert `Cache NuGet packages` step (`actions/cache@v4`, `path: ${{ env.NUGET_PACKAGES }}`, `key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}`, `restore-keys: ${{ runner.os }}-nuget-`) between the `Setup .NET` and `Restore` steps in `.github/workflows/build.yml` (FR-009; uses `${{ env.NUGET_PACKAGES }}` env var for path consistency with FR-019; keyed on csproj hash because no `packages.lock.json` exists)

**Checkpoint**: All three user stories fully functional — push, PR, and NuGet caching all work correctly.

---

## Phase 6: Polish & Cross-Cutting Validation

**Purpose**: Final syntax validation and acceptance against quickstart.md scenarios before merge.

- [X] T019 [P] Validate `.github/workflows/build.yml` syntax: confirm the file is complete and well-formed; run `actionlint` if available to catch GitHub Actions-specific errors (SC-001)
- [X] T020 [P] Verify baseline is green: run `dotnet test` from repository root and confirm all existing tests pass with zero failures; a green local run is the prerequisite for SC-003 (FR-007)
- [ ] T021 Run live GitHub Actions validation — push the feature branch and open a PR to `main`, then confirm: all 12 named steps appear in the Actions run log (SC-007), the `test-results` and `coverage-report` artifacts are downloadable from the run summary (FR-014, FR-016), the PR check status updates on new commits (SC-002), inline test results appear via Check Run (same-repo PR) or Job Summary (fork PR) (FR-017), and a sticky PR comment with coverage summary is posted (FR-022)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **User Stories (Phase 3–5)**: All depend on Foundational phase; execute in priority order (P1 → P2 → P3) since each phase adds to the same file
- **Polish (Phase 6)**: Depends on all user story phases completing

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — no dependencies on US2 or US3
- **User Story 2 (P2)**: Can start after US1 — adds PR trigger, permissions, and all post-test reporting steps; US2 steps must come after the US1 `Test` step in the YAML
- **User Story 3 (P3)**: Can start after Foundational — inserts the cache step between `Setup .NET` and `Restore` (independent of US2 reporting steps)

### Within Each User Story

- Workflow steps must be authored in the order they execute in the job (top-to-bottom YAML)
- US1: push trigger → Restore → Build → Test
- US2: pull_request trigger + permissions → Upload Test Results → Publish Test Results → Generate Coverage Report → Write Coverage to Job Summary → Upload Coverage Report → Post Coverage Summary PR Comment
- US3: Cache NuGet packages (inserted between Setup .NET and Restore)

### Parallel Opportunities

- T019 and T020 (Polish phase) are independent read-only operations and can run simultaneously
- US3 (T018) is logically independent of US2 (T010–T017) — if multiple developers were editing different sections of the file, they could work in parallel and merge; in a single-developer context, execute sequentially

---

## Implementation Strategy

### Remaining Work (1 open task)

1. **T021** — Live GitHub Actions validation via push + PR to `main` per quickstart.md

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (create directory + skeleton file)
2. Complete Phase 2: Foundational (Checkout + Setup .NET + env vars)
3. Complete Phase 3: User Story 1 (push trigger + restore + build + test)
4. **STOP and VALIDATE**: Push to `main` and verify green CI run
5. Proceed to US2 if MVP is confirmed working

### Incremental Delivery

1. Complete Setup + Foundational → skeleton with shared steps
2. Add User Story 1 → core push-triggered pipeline → push to `main` to validate (MVP!)
3. Add User Story 2 → PR trigger + all reporting steps → open PR to validate
4. Add User Story 3 → NuGet cache step → run twice to confirm cache hit
5. Each story adds value without breaking the previous stories

---

## File Change Summary

| File | Tasks | Change Type |
|---|---|---|
| `.github/workflows/build.yml` | T001–T021 | **New file**: CI workflow (sole deliverable) |

**No changes to**: source code, test files, project files, `Directory.Build.props`, or any other existing file (FR-011 hard constraint).

---

## Requirements Coverage

| Requirement | Description | Satisfied by | Status |
|---|---|---|---|
| FR-001 | Trigger on push to `main` | T006 | ✅ Done |
| FR-002 | Trigger on PR to `main` | T010 | ✅ Done |
| FR-003 | `actions/setup-dotnet@v5.2.0`, .NET `10.0.x` | T004 | ✅ Done |
| FR-004 | `dotnet restore` as distinct named step | T007 | ✅ Done |
| FR-005 | `dotnet build --configuration Release --no-restore` | T008 | ✅ Done |
| FR-006 | `dotnet test --configuration Release --no-build` | T009 | ✅ Done |
| FR-007 | Fail if any test fails | T009, T020 | ✅ Done |
| FR-008 | Treat build warnings as errors (`--warnaserror`) | T008 | ✅ Done |
| FR-009 | Cache NuGet packages; `path: ${{ env.NUGET_PACKAGES }}` | T018 | ✅ Done |
| FR-010 | File at `.github/workflows/build.yml` | T001, T002 | ✅ Done |
| FR-011 | No changes to source or test files | All tasks — workflow file only | ✅ Done |
| FR-012 | XPlat Code Coverage + TRX + `./TestResults` | T009 | ✅ Done |
| FR-013 | `danielpalme/ReportGenerator-GitHub-Action@5.5.10` with extended options | T014 | ✅ Done |
| FR-014 | Upload coverage report artifact; `retention-days: 14` | T016 | ✅ Done |
| FR-015 | Post-test steps with `if: always()`; failures fail workflow | T012, T013, T014, T015, T016 | ✅ Done |
| FR-016 | Upload TRX test results as artifact | T012 | ✅ Done |
| FR-017 | `dorny/test-reporter@v3.0.0` inline PR reporting | T011, T013 | ✅ Done |
| FR-018 | `actions/checkout@v6.0.2` pinned patch version | T003 | ✅ Done |
| FR-019 | Workflow-level env vars (`DOTNET_SKIP_FIRST_TIME_EXPERIENCE`, `DOTNET_CLI_TELEMETRY_OPTOUT`, `NUGET_PACKAGES`) | T005 | ✅ Done |
| FR-020 | `Write Coverage to Job Summary` step immediately after Generate Coverage Report | T015 | ✅ Done |
| FR-021 | `pull-requests: write` permission on job | T011 | ✅ Done |
| FR-022 | Sticky PR comment via `marocchino/sticky-pull-request-comment@v3.0.4` | T017 | ✅ Done |
| FR-023 | `coverlet.collector` referenced in test project | Already present at v10.0.1 — no task needed | ✅ Done |

---

## Notes

- `TreatWarningsAsErrors=true` is already active via `Directory.Build.props` — the `Build` step will fail on any warning without any CLI flag (research.md Item 3)
- The `.slnx` solution format is natively supported by .NET 9+ CLI — `dotnet restore / build / test` auto-discovers it at the repo root
- XPlat Code Coverage writes `coverage.cobertura.xml` into a random GUID subdirectory under `./TestResults/` — the `**/coverage.cobertura.xml` glob in T014 is mandatory (research.md Item 7)
- `dorny/test-reporter@v3.0.0` requires `permissions: { checks: write, actions: read }` (T011) for Check Run creation on same-repo PRs; fork PRs receive a read-only token from GitHub and fall back to Job Summary — no `continue-on-error` needed (research.md Item 10)
- `coverlet.collector@10.0.1` is already present in `SpecKitApi.Tests.csproj` — FR-023 satisfied without a code-change task
- Commit on feature branch `007-github-actions-ci` after each phase checkpoint is verified
