# Implementation Plan: GitHub Actions CI Build Workflow

**Branch**: `007-github-actions-ci` | **Date**: 2026-05-25 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/007-github-actions-ci/spec.md`

## Summary

Add a single GitHub Actions workflow file (`.github/workflows/build.yml`) that provides continuous integration for the SpecKitApi .NET 10 Web API. The workflow triggers on every push and pull request targeting `main`, runs on `ubuntu-latest`, checks out the repository via `actions/checkout@v6.0.2` (FR-018), installs the .NET 10 SDK via `actions/setup-dotnet@v5.2.0`, caches NuGet packages keyed on a hash of all `*.csproj` files, then executes three individually named build steps — restore → build (Release, `--warnaserror`) → test (Release, with XPlat Code Coverage and TRX logging) — followed by four post-test reporting steps: upload TRX test results as a downloadable artifact, publish test results inline on pull requests using `dorny/test-reporter@v3.0.0` (with Job Summary fallback for fork PRs), generate an HTML + Cobertura coverage report, and upload the report as a downloadable artifact using `actions/upload-artifact@v7.0.1`. The `--warnaserror` flag is required by FR-008 and the existing `Directory.Build.props` also enforces warnings-as-errors at the MSBuild level; no source code changes are required or permitted.

## Technical Context

**Language/Version**: GitHub Actions YAML workflow syntax — orchestrates the .NET 10 / C# 13 toolchain

**Primary Dependencies**:
- `actions/checkout@v6.0.2` — checkout the repository at the triggering ref (FR-018)
- `actions/setup-dotnet@v5.2.0` — install .NET SDK `10.0.x` (latest patch) on the runner (FR-003)
- `actions/cache@v4` — persist and restore `~/.nuget/packages` between workflow runs
- `actions/upload-artifact@v7.0.1` — upload TRX test results and generated coverage report as build artifacts (FR-014, FR-016)
- `danielpalme/ReportGenerator-GitHub-Action@5.5.10` — generate HTML + Cobertura coverage report from `coverage.cobertura.xml`
- `dorny/test-reporter@v3.0.0` — publish test results inline on PRs via Check Run (non-fork) or GitHub Actions Job Summary (fork PR fallback)

**Storage**: GitHub Actions Cache (ephemeral NuGet package store; keyed on `*.csproj` file hash); GitHub Actions Artifacts (downloadable coverage report, retained per repo default)

**Testing**: Acceptance via live GitHub Actions execution; inner test runner is xUnit v3 via `dotnet test`; coverage collected via `coverlet.collector` (XPlat Code Coverage data collector)

**Target Platform**: `ubuntu-latest` GitHub-hosted runner

**Project Type**: CI/CD infrastructure (pure YAML configuration — no runtime code)

**Performance Goals**: Warm-cache runs skip NuGet.org downloads; restore step is a cache hit on subsequent runs when no packages change (SC-006)

**Constraints**:
- No source code or test file changes (FR-011, hard constraint)
- Workflow file at `.github/workflows/build.yml` only (FR-010)
- `--warnaserror` required on `dotnet build` per FR-008; `Directory.Build.props` also enforces it (redundant but spec-required)
- `actions/checkout@v6.0.2` required per FR-018 (pinned patch version)
- No matrix builds, secrets, deployments, or environment-specific configuration in scope
- `permissions: { checks: write, actions: read }` required on the job for `dorny/test-reporter` Check Run creation on non-fork PRs; fork PRs receive read-only tokens from GitHub's security model and fall back to Job Summary via v3's graceful degradation

**Scale/Scope**: One workflow file, ~75 lines of YAML, one job, ten named steps

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Rationale |
|-----------|--------|-----------|
| I. API-First Design | ✅ EXEMPT | Infrastructure-only change — no API endpoints, contracts, or public surfaces added or modified. `contracts/` not applicable. |
| II. Spec-Driven Development | ✅ PASS | `specs/007-github-actions-ci/spec.md` is complete. All 18 functional requirements, 3 user stories with acceptance scenarios, and 7 success criteria are defined. No `NEEDS CLARIFICATION` tokens remain. |
| III. Test-First (TDD) | ✅ EXEMPT | The sole deliverable is a YAML configuration file. There is no unit-testable application logic. Validation is by live workflow execution against the acceptance scenarios defined in the spec (push to `main` triggers all steps; a failing test fails the workflow; a build warning fails the workflow). |
| IV. Observability & Structured Logging | ✅ EXEMPT | No service code is added or modified. No new endpoints exist. CI build visibility is inherent to GitHub Actions. |
| V. Simplicity & YAGNI | ✅ PASS | Single workflow, single job, no matrix, no speculative steps. The design is the minimum required to satisfy FR-001 through FR-018. |

**Post-Phase 1 re-check**: ✅ All five principles confirmed passing or exempt. No Complexity Tracking entries required.

## Project Structure

### Documentation (this feature)

```text
specs/007-github-actions-ci/
├── plan.md              ← This file
├── research.md          ← Phase 0 output (all unknowns resolved)
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
    └── SpecKitApi.Tests.csproj

SpecKitApi.slnx
Directory.Build.props    ← EXISTING: TreatWarningsAsErrors=true (relied upon, not modified)
```

**Structure Decision**: Infrastructure-only layout. A single new file is added under `.github/workflows/`. No directories are added to `src/` or `tests/`. No existing file is modified.

## Workflow Design

The workflow reflects the requirements from spec session 2026-05-25 and research findings:

1. **`actions/checkout@v6.0.2`** — pinned patch version as explicitly required by FR-018.

2. **`actions/setup-dotnet@v5.2.0`** — pinned patch version as explicitly required by FR-003.

3. **`--warnaserror` on `dotnet build`** — required by FR-008 ("MUST treat build warnings as errors by running the build step with `--warnaserror`"). `Directory.Build.props` also enforces warnings-as-errors; the CLI flag is redundant at the MSBuild level but mandatory per spec and makes intent self-documenting in the YAML.

4. **`permissions: { checks: write, actions: read }`** — required for `dorny/test-reporter` to create Check Runs on non-fork PRs. Fork-originated PRs receive a read-only token from GitHub's security model regardless of this declaration; v3.0.0 falls back gracefully to GitHub Actions Job Summary for those PRs (see Research Item 10).

5. **Extended `dotnet test` command** — adds `--collect:"XPlat Code Coverage"`, `--logger trx`, and `--results-directory ./TestResults` to capture both TRX results and Cobertura coverage XML.

6. **Four post-test steps** (all with `if: always()`):
   - **Upload Test Results** — `actions/upload-artifact@v7.0.1` uploads `TestResults/` as `test-results` artifact for diagnostic download.
   - **Publish Test Results** — `dorny/test-reporter@v3.0.0` publishes inline test results; creates a Check Run on non-fork PRs and writes a GitHub Actions Job Summary for all PRs (including fork PRs via v3's graceful degradation). `fail-on-error: true` (default) fails the step if any test failed; `fail-on-empty: true` (default) fails the step if no TRX files are found.
   - **Generate Coverage Report** — `danielpalme/ReportGenerator-GitHub-Action@5.5.10` self-installs `reportgenerator` and produces `coveragereport/` from `**/coverage.cobertura.xml`.
   - **Upload Coverage Report** — `actions/upload-artifact@v7.0.1` uploads `coveragereport/` as the `coverage-report` artifact (FR-014).

### Final Workflow YAML

```yaml
# .github/workflows/build.yml
name: Build

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    permissions:
      contents: read
      checks: write
      actions: read

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
          path: ~/.nuget/packages
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
          reporttypes: 'HtmlInline;Cobertura'

      - name: Upload Coverage Report
        uses: actions/upload-artifact@v7.0.1
        if: always()
        with:
          name: coverage-report
          path: coveragereport/
```

## Complexity Tracking

> No Constitution violations detected — this section is intentionally empty.

