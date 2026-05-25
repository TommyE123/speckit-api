# Implementation Plan: GitHub Actions CI Build Workflow

**Branch**: `007-github-actions-ci` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/007-github-actions-ci/spec.md`

## Summary

Add a single GitHub Actions workflow file (`.github/workflows/build.yml`) that provides continuous integration for the SpecKitApi .NET 10 Web API. The workflow triggers on every push and pull request targeting `main`, runs on `ubuntu-latest`, declares workflow-level environment variables (`DOTNET_SKIP_FIRST_TIME_EXPERIENCE`, `DOTNET_CLI_TELEMETRY_OPTOUT`, `NUGET_PACKAGES`) for telemetry opt-out and cache path consistency (FR-019), checks out the repository via `actions/checkout@v6.0.2` (FR-018), installs the .NET 10 SDK via `actions/setup-dotnet@v5.2.0`, caches NuGet packages at `${{ env.NUGET_PACKAGES }}` keyed on a hash of all `*.csproj` files (FR-009), then executes three individually named build steps — restore → build (Release, `--warnaserror`) → test (Release, with XPlat Code Coverage and TRX logging) — followed by six post-test reporting steps: upload TRX test results, publish test results inline on pull requests using `dorny/test-reporter@v3.0.0` (with Job Summary fallback for fork PRs), generate an HTML + Cobertura + Markdown coverage report (with `assemblyfilters: '-*.Tests*'` and `MarkdownSummaryGithub` output), write the coverage summary to the GitHub Actions job summary (FR-020), upload the report as a downloadable artifact with `retention-days: 14` (FR-014), and post a sticky PR comment with the coverage summary on pull request events (FR-022). The `--warnaserror` flag is required by FR-008 and the existing `Directory.Build.props` also enforces warnings-as-errors at the MSBuild level; no source code changes are required or permitted. `coverlet.collector@10.0.1` is already referenced in `SpecKitApi.Tests.csproj` (FR-023 satisfied).

## Technical Context

**Language/Version**: GitHub Actions YAML workflow syntax — orchestrates the .NET 10 / C# 13 toolchain

**Primary Dependencies**:
- `actions/checkout@v6.0.2` — checkout the repository at the triggering ref (FR-018)
- `actions/setup-dotnet@v5.2.0` — install .NET SDK `10.0.x` (latest patch) on the runner (FR-003)
- `actions/cache@v4` — persist and restore `${{ env.NUGET_PACKAGES }}` between workflow runs (FR-009)
- `actions/upload-artifact@v7.0.1` — upload TRX test results and generated coverage report as build artifacts (FR-014, FR-016)
- `danielpalme/ReportGenerator-GitHub-Action@5.5.10` — generate HTML + Cobertura + MarkdownSummaryGithub coverage report from `coverage.cobertura.xml` (FR-013)
- `dorny/test-reporter@v3.0.0` — publish test results inline on PRs via Check Run (non-fork) or GitHub Actions Job Summary (fork PR fallback) (FR-017)
- `marocchino/sticky-pull-request-comment@2.9.4` — post sticky PR comment with coverage summary (FR-022)

**Storage**: GitHub Actions Cache (ephemeral NuGet package store at `${{ github.workspace }}/.nuget/packages`; keyed on `*.csproj` file hash); GitHub Actions Artifacts (downloadable coverage report, `retention-days: 14`)

**Testing**: Acceptance via live GitHub Actions execution; inner test runner is xUnit v3 via `dotnet test`; coverage collected via `coverlet.collector@10.0.1` (XPlat Code Coverage data collector — already present in test project, FR-023 satisfied)

**Target Platform**: `ubuntu-latest` GitHub-hosted runner

**Project Type**: CI/CD infrastructure (pure YAML configuration — no runtime code)

**Performance Goals**: Warm-cache runs skip NuGet.org downloads; restore step is a cache hit on subsequent runs when no packages change (SC-006)

**Constraints**:
- No source code or test file changes (FR-011, hard constraint)
- Workflow file at `.github/workflows/build.yml` only (FR-010)
- `--warnaserror` required on `dotnet build` per FR-008; `Directory.Build.props` also enforces it (redundant but spec-required)
- `actions/checkout@v6.0.2` required per FR-018 (pinned patch version)
- No matrix builds, secrets, deployments, or environment-specific configuration in scope
- `permissions: { contents: read, checks: write, actions: read, pull-requests: write }` required on the job — `checks: write` for `dorny/test-reporter` Check Run creation on non-fork PRs; `pull-requests: write` for sticky PR comment; fork PRs receive read-only tokens from GitHub's security model and fall back to Job Summary via v3's graceful degradation

**Scale/Scope**: One workflow file, ~100 lines of YAML, one job, thirteen named steps

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Rationale |
|-----------|--------|-----------|
| I. API-First Design | ✅ EXEMPT | Infrastructure-only change — no API endpoints, contracts, or public surfaces added or modified. `contracts/` not applicable. |
| II. Spec-Driven Development | ✅ PASS | `specs/007-github-actions-ci/spec.md` is complete. All 23 functional requirements, 3 user stories with acceptance scenarios, and 7 success criteria are defined. No `NEEDS CLARIFICATION` tokens remain. |
| III. Test-First (TDD) | ✅ EXEMPT | The sole deliverable is a YAML configuration file. There is no unit-testable application logic. Validation is by live workflow execution against the acceptance scenarios defined in the spec. |
| IV. Observability & Structured Logging | ✅ EXEMPT | No service code is added or modified. No new endpoints exist. CI build visibility is inherent to GitHub Actions. |
| V. Simplicity & YAGNI | ✅ PASS | Single workflow, single job, no matrix, no speculative steps. The design is the minimum required to satisfy FR-001 through FR-023. |

**Post-Phase 1 re-check**: ✅ All five principles confirmed passing or exempt. No Complexity Tracking entries required.

## Project Structure

### Documentation (this feature)

```text
specs/007-github-actions-ci/
├── plan.md              ← This file
├── research.md          ← Phase 0 output (all unknowns resolved, RI-1 through RI-16)
├── data-model.md        ← N/A — no data entities in a CI workflow
├── quickstart.md        ← Phase 1 output (workflow usage reference)
├── contracts/           ← N/A — no API surface introduced
└── tasks.md             ← Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
.github/
└── workflows/
    └── build.yml        ← NEW: CI workflow (sole deliverable of this feature)

# All existing source files are unchanged:
src/
└── SpecKitApi/
    └── SpecKitApi.csproj

tests/
└── SpecKitApi.Tests/
    └── SpecKitApi.Tests.csproj   ← coverlet.collector@10.0.1 already present (FR-023)

SpecKitApi.slnx
Directory.Build.props    ← EXISTING: TreatWarningsAsErrors=true (relied upon, not modified)
```

**Structure Decision**: Infrastructure-only layout. A single new file is added under `.github/workflows/`. No directories are added to `src/` or `tests/`. No existing file is modified.

## Workflow Design

The workflow reflects all requirements from spec.md (including the 2026-05-25 clarification session) and research findings (RI-1 through RI-16):

1. **Workflow-level `env:` block** — declares `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true`, `DOTNET_CLI_TELEMETRY_OPTOUT=true`, and `NUGET_PACKAGES=${{ github.workspace }}/.nuget/packages` (FR-019). The `NUGET_PACKAGES` env var is referenced by `path: ${{ env.NUGET_PACKAGES }}` in the cache step (FR-009).

2. **`actions/checkout@v6.0.2`** — pinned patch version as explicitly required by FR-018.

3. **`actions/setup-dotnet@v5.2.0`** — pinned patch version as explicitly required by FR-003.

4. **`actions/cache@v4` with `path: ${{ env.NUGET_PACKAGES }}`** — uses the env var path (FR-009); key hashes all `*.csproj` files because no `packages.lock.json` exists (see RI-2).

5. **`--warnaserror` on `dotnet build`** — required by FR-008. `Directory.Build.props` also enforces it; the CLI flag is redundant at runtime but mandatory per spec.

6. **`permissions: { contents: read, checks: write, actions: read, pull-requests: write }`** — `checks: write` for `dorny/test-reporter` Check Run creation on non-fork PRs (FR-017, RI-10); `pull-requests: write` for sticky PR comment (FR-021, FR-022, RI-12). Fork PRs receive a read-only token regardless — v3.0.0 falls back to Job Summary gracefully.

7. **Extended `dotnet test` command** — adds `--collect:"XPlat Code Coverage"`, `--logger trx`, and `--results-directory ./TestResults` to capture both TRX results and Cobertura coverage XML (FR-012).

8. **Six post-test steps** (all with `if: always()` unless noted):
   - **Upload Test Results** — `actions/upload-artifact@v7.0.1` uploads `TestResults/` as `test-results` artifact (FR-016).
   - **Publish Test Results** — `dorny/test-reporter@v3.0.0` creates Check Run (non-fork) or writes Job Summary (fork). `fail-on-error: true` default fails if any test failed; `fail-on-empty: true` fails if no TRX files found (FR-017).
   - **Generate Coverage Report** — `danielpalme/ReportGenerator-GitHub-Action@5.5.10` with `assemblyfilters: '-*.Tests*'`, `verbosity: 'Warning'`, `reporttypes: 'HtmlInline;Cobertura;MarkdownSummaryGithub'` — produces `coveragereport/SummaryGithub.md` consumed by next two steps (FR-013).
   - **Write Coverage to Job Summary** — `cat coveragereport/SummaryGithub.md >> $GITHUB_STEP_SUMMARY`; placed immediately after Generate Coverage Report (FR-020).
   - **Upload Coverage Report** — `actions/upload-artifact@v7.0.1` with `retention-days: 14` (FR-014).
   - **Post Coverage Summary PR Comment** — `marocchino/sticky-pull-request-comment@2.9.4`, gated on `github.event_name == 'pull_request'` (not `if: always()`), `recreate: true`, reads `coveragereport/SummaryGithub.md` (FR-022).

### Final Workflow YAML

```yaml
# .github/workflows/build.yml
name: Build

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

env:
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true
  NUGET_PACKAGES: ${{ github.workspace }}/.nuget/packages

jobs:
  build:
    runs-on: ubuntu-latest

    permissions:
      contents: read
      checks: write
      actions: read
      pull-requests: write

    steps:
      - name: Checkout
        uses: actions/checkout@v6.0.2

      - name: Setup .NET
        uses: actions/setup-dotnet@v5.2.0
        with:
          dotnet-version: '10.0.x'

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ${{ env.NUGET_PACKAGES }}
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore --warnaserror

      - name: Test
        run: dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage" --logger trx --results-directory ./TestResults

      - name: Upload Test Results
        uses: actions/upload-artifact@v7.0.1
        if: always()
        with:
          name: test-results
          path: TestResults/

      - name: Publish Test Results
        uses: dorny/test-reporter@v3.0.0
        if: always()
        with:
          name: Test Results
          path: TestResults/**/*.trx
          reporter: dotnet-trx

      - name: Generate Coverage Report
        uses: danielpalme/ReportGenerator-GitHub-Action@5.5.10
        if: always()
        with:
          reports: '**/coverage.cobertura.xml'
          targetdir: 'coveragereport'
          assemblyfilters: '-*.Tests*'
          verbosity: 'Warning'
          reporttypes: 'HtmlInline;Cobertura;MarkdownSummaryGithub'

      - name: Write Coverage to Job Summary
        if: always()
        run: cat coveragereport/SummaryGithub.md >> $GITHUB_STEP_SUMMARY

      - name: Upload Coverage Report
        uses: actions/upload-artifact@v7.0.1
        if: always()
        with:
          name: coverage-report
          path: coveragereport/
          retention-days: 14

      - name: Post Coverage Summary PR Comment
        uses: marocchino/sticky-pull-request-comment@2.9.4
        if: github.event_name == 'pull_request'
        with:
          recreate: true
          path: coveragereport/SummaryGithub.md
```

## Requirements Coverage

| Requirement | Description | Satisfied by | Status |
|---|---|---|---|
| FR-001 | Trigger on push to `main` | `on: push` trigger | ✅ |
| FR-002 | Trigger on PR to `main` | `on: pull_request` trigger | ✅ |
| FR-003 | `actions/setup-dotnet@v5.2.0`, .NET `10.0.x` | Setup .NET step | ✅ |
| FR-004 | `dotnet restore` as distinct named step | Restore step | ✅ |
| FR-005 | `dotnet build --configuration Release --no-restore` | Build step | ✅ |
| FR-006 | `dotnet test --configuration Release --no-build` | Test step | ✅ |
| FR-007 | Fail if any test fails | `dotnet test` exit code | ✅ |
| FR-008 | Treat build warnings as errors (`--warnaserror`) | Build step flag | ✅ |
| FR-009 | Cache NuGet packages; `path: ${{ env.NUGET_PACKAGES }}` | Cache NuGet packages step | ✅ |
| FR-010 | File at `.github/workflows/build.yml` | Workflow file location | ✅ |
| FR-011 | No changes to source or test files | YAML-only change | ✅ |
| FR-012 | XPlat Code Coverage + TRX + `./TestResults` | Test step flags | ✅ |
| FR-013 | `danielpalme/ReportGenerator@5.5.10` with extended options | Generate Coverage Report step | ✅ |
| FR-014 | Upload coverage report artifact; `retention-days: 14` | Upload Coverage Report step | ✅ |
| FR-015 | Post-test steps with `if: always()`; failures fail workflow | Upload TRX, Publish Test Results, Generate Coverage Report, Write Job Summary, Upload Coverage Report | ✅ |
| FR-016 | Upload TRX test results as artifact | Upload Test Results step | ✅ |
| FR-017 | `dorny/test-reporter@v3.0.0` inline PR reporting | Publish Test Results step | ✅ |
| FR-018 | `actions/checkout@v6.0.2` pinned | Checkout step | ✅ |
| FR-019 | Workflow-level env vars (DOTNET_*, NUGET_PACKAGES) | `env:` block at workflow scope | ✅ |
| FR-020 | Write Coverage to Job Summary after Generate Coverage Report | Write Coverage to Job Summary step | ✅ |
| FR-021 | `pull-requests: write` permission | Job `permissions:` block | ✅ |
| FR-022 | Sticky PR comment via `marocchino/sticky-pull-request-comment@2.9.4` | Post Coverage Summary PR Comment step | ✅ |
| FR-023 | `coverlet.collector` referenced in test project | Already present at v10.0.1 | ✅ |

## Complexity Tracking

> No Constitution violations detected — this section is intentionally empty.

