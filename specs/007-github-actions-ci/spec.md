# Feature Specification: GitHub Actions CI Build Workflow

**Feature Branch**: `007-github-actions-ci`

**Created**: 2026-05-24

**Status**: Draft

**Input**: User description: "Add a GitHub Actions CI build workflow for the SpecKitApi .NET 10 Web API."

## Clarifications

### Session 2026-05-25

- Q: How should test results be retained without inline PR reporting? → A: Upload TRX test result files as a build artifact using if: always().
- Q: Should post-test diagnostic steps be gating or non-gating? → A: Gating; if any post-test step fails, the workflow fails.
- Q: How should coverage report generation be implemented? → A: Use `danielpalme/ReportGenerator-GitHub-Action@5.5.10` (not a manual `dotnet tool install` step).
- Q: Should test results also be published inline on pull requests? → A: Yes; add `dorny/test-reporter@v3.0.0` with `reporter: dotnet-trx` and `path: TestResults/**/*.trx`, running with `if: always()` immediately after test-results artifact upload.
- Q: Should inline PR test reporting be mandatory and gating for all PRs (including forks)? → A: Yes; the `dorny/test-reporter` step must run and succeed for every pull request, and workflow execution must fail if reporting cannot be published.
- Q: What checkout action version should be required? → A: Use `actions/checkout@v6.0.2`.
- Q: What workflow hardening and reporting enhancements are required? → A: Add workflow-level env vars (`DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true`, `DOTNET_CLI_TELEMETRY_OPTOUT=true`, `NUGET_PACKAGES=${{ github.workspace }}/.nuget/packages`), use `${{ env.NUGET_PACKAGES }}` for cache path, extend ReportGenerator options (`assemblyfilters: '-*.Tests*'`, `verbosity: Warning`, `reporttypes: 'HtmlInline;Cobertura;MarkdownSummaryGithub'`), add `Write Coverage to Job Summary` step (`if: always()`) after coverage generation, add `pull-requests: write` permission, add sticky PR comment via `marocchino/sticky-pull-request-comment@v3.0.4` on `pull_request` events with `recreate: true` reading `coveragereport/SummaryGithub.md`, and set coverage artifact `retention-days: 14`.
- Q: How should workflow coverage be collected to maximize CI portability? → A: CI test execution must collect with `--collect:"XPlat Code Coverage"` so Ubuntu-based GitHub Actions runs reliably produce Cobertura input for ReportGenerator.
- Q: How should universal coverage migration be layered onto the existing CI spec? → A: Add shared coverage wiring as an incremental extension to this feature (single root `codecoverage.runsettings`, `Directory.Build.props` path injection via `$(MSBuildThisFileDirectory)`, and CI tests collecting with XPlat for Ubuntu compatibility).
- Q: What part of the migration example needs correction? → A: CI coverage collection uses `XPlat Code Coverage`; local Visual Studio Enterprise coverage may still use Microsoft `Code Coverage` semantics.

## Coverage Migration Addendum (Incremental)

This addendum extends the existing CI workflow feature rather than replacing it. The objective is a single repository coverage configuration that works in local Visual Studio Enterprise workflows and Ubuntu GitHub Actions CI without manual IDE setup.

### Addendum Scenarios

1. Visual Studio Enterprise users can run Analyze Code Coverage and get exclusions from the repository runsettings without manually selecting settings files.
2. CLI users can run `dotnet test --collect:"Code Coverage"` and obtain Cobertura-compatible coverage output governed by repository runsettings defaults.
3. Pull request CI runs produce Cobertura input for ReportGenerator and publish Markdown coverage summaries consistently.

### Addendum Acceptance Checks

- [ ] A single repository-root `codecoverage.runsettings` governs coverage behavior.
- [ ] `Directory.Build.props` injects `RunSettingsFilePath` using `$(MSBuildThisFileDirectory)`.
- [ ] Workflow test execution collects with `--collect:"XPlat Code Coverage"` and honors repository coverage rules.
- [ ] Coverage output excludes `xunit` and test assemblies consistently.
- [ ] Coverage summary publishing remains functional in CI (`SummaryGithub.md` generated and consumed).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Developer Pushes Code to Main (Priority: P1)

A developer pushes changes directly to the `main` branch. The CI workflow automatically triggers, restores dependencies, builds the solution in Release configuration, and runs all tests. If any step fails, the workflow reports failure and the developer is notified.

**Why this priority**: This is the core CI use case — protecting the `main` branch from broken code is the primary goal of the workflow.

**Independent Test**: Can be fully tested by pushing a commit to `main` and verifying the workflow runs all three steps (restore, build, test) and reports a result.

**Acceptance Scenarios**:

1. **Given** a commit is pushed to `main`, **When** the workflow triggers, **Then** the restore, build, and test steps all execute and pass for a healthy codebase.
2. **Given** a commit is pushed to `main` with a failing test, **When** the workflow runs, **Then** the workflow fails and reports which test(s) failed.
3. **Given** a commit is pushed to `main` with a build warning, **When** the workflow runs, **Then** the build step fails and the workflow is marked as failed.

---

### User Story 2 - Developer Opens a Pull Request to Main (Priority: P2)

A developer opens or updates a pull request targeting `main`. The CI workflow runs automatically, providing build and test feedback before the code is merged.

**Why this priority**: PR validation is critical for code review quality and prevents broken code from reaching `main` via merges.

**Independent Test**: Can be fully tested by opening a pull request to `main` and verifying the workflow triggers and reports status on the PR.

**Acceptance Scenarios**:

1. **Given** a pull request is opened against `main`, **When** the workflow triggers, **Then** all three steps (restore, build, test) run and the PR check status reflects the result.
2. **Given** a pull request is updated with a new commit, **When** the workflow triggers again, **Then** the latest result replaces the previous check status on the PR.

---

### User Story 3 - Developer Benefits from Cached NuGet Packages (Priority: P3)

After the first workflow run, NuGet packages are cached. Subsequent workflow runs restore packages from cache rather than re-downloading them, reducing build time.

**Why this priority**: Performance improvement — cached packages speed up CI feedback loops, especially important for frequent commits or PR updates.

**Independent Test**: Can be verified by comparing workflow run durations: the second run (cache warm) should show a cache hit in the restore step and complete faster than the first.

**Acceptance Scenarios**:

1. **Given** a prior workflow run has populated the NuGet cache, **When** a new workflow run starts, **Then** the restore step retrieves packages from cache and skips downloading them from NuGet.org.
2. **Given** the cache is stale or missing, **When** a new workflow run starts, **Then** the restore step downloads all packages and repopulates the cache.

---

### Edge Cases

- What happens when the .NET 10 SDK is not yet available as a stable release on the runner? The workflow should fail with a clear error rather than silently using a lower version.
- How does the workflow handle a solution with multiple projects where one builds successfully but another fails? The build step must fail the entire workflow, not just the failing project.
- What happens if the test runner encounters a runtime error (not a test failure)? The test step must fail and the workflow must surface the error output.
- What if the NuGet cache key changes (e.g., packages.lock.json updated)? The cache must be invalidated and packages re-downloaded.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The workflow MUST trigger automatically on every push to the `main` branch.
- **FR-002**: The workflow MUST trigger automatically on every pull request targeting the `main` branch.
- **FR-003**: The workflow MUST use `actions/setup-dotnet@v5.2.0` to install and configure the latest stable .NET 10 SDK available on the CI runner.
- **FR-004**: The workflow MUST execute `dotnet restore` as a distinct, named step.
- **FR-005**: The workflow MUST execute `dotnet build` in Release configuration as a distinct, named step.
- **FR-006**: The workflow MUST execute `dotnet test` in Release configuration as a distinct, named step.
- **FR-007**: The workflow MUST fail if any test fails during the test step.
- **FR-008**: The workflow MUST treat build warnings as errors by running the build step with `--warnaserror`, causing the build step to fail if any warnings are produced.
- **FR-009**: The workflow MUST cache NuGet packages between runs using `actions/cache@v4` with `path: ${{ env.NUGET_PACKAGES }}`.
- **FR-010**: The workflow file MUST be located at `.github/workflows/build.yml` in the repository.
- **FR-011**: No source code or business logic changes are permitted as part of this feature.
- **FR-012**: The workflow MUST collect code coverage during the test run using `--collect:"XPlat Code Coverage"` in CI so Ubuntu workflow runs generate Cobertura-compatible output for ReportGenerator.
- **FR-013**: The workflow MUST generate an HTML coverage report using `danielpalme/ReportGenerator-GitHub-Action@5.5.10` with `reports: '**/*.cobertura.xml'`, `targetdir: 'coveragereport'`, `assemblyfilters: '-*.Tests*'`, `verbosity: 'Warning'`, and `reporttypes: 'HtmlInline;Cobertura;MarkdownSummaryGithub'`; a separate manual `dotnet tool install` step MUST NOT be used for coverage report generation.
- **FR-014**: The workflow MUST upload the generated coverage report as a build artifact using `actions/upload-artifact@v7.0.1` with `retention-days: 14`.
- **FR-015**: All post-test steps (including TRX artifact upload, test-result publishing, coverage report generation, coverage summary publication, PR comment publication, and artifact upload) MUST run with `if: always()` where applicable so they execute even when tests fail, and failures in these steps MUST fail the workflow unless explicitly event-gated.
- **FR-016**: The workflow MUST upload TRX test result files as a build artifact to preserve test diagnostics when tests fail.
- **FR-017**: The workflow MUST publish test results inline on pull requests using `dorny/test-reporter@v3.0.0` with `reporter: dotnet-trx` and `path: TestResults/**/*.trx`; this step MUST run with `if: always()`, be placed immediately after the test results artifact upload step, run for all pull requests (including fork-originated pull requests), and fail the workflow if publication is unsuccessful.
- **FR-018**: The workflow MUST use `actions/checkout@v6.0.2` for the repository checkout step.
- **FR-019**: The workflow MUST define workflow-level environment variables: `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true`, `DOTNET_CLI_TELEMETRY_OPTOUT=true`, and `NUGET_PACKAGES=${{ github.workspace }}/.nuget/packages`.
- **FR-020**: The workflow MUST include a `Write Coverage to Job Summary` step immediately after `Generate Coverage Report` with `if: always()` and command `cat coveragereport/SummaryGithub.md >> $GITHUB_STEP_SUMMARY`.
- **FR-021**: The build job permissions MUST include `pull-requests: write` in addition to existing permissions required for checks and actions.
- **FR-022**: The workflow MUST add a sticky PR comment step using `marocchino/sticky-pull-request-comment@v3.0.4`, limited to `pull_request` events, with `recreate: true`, and comment content read from `coveragereport/SummaryGithub.md`.
- **FR-023**: The test project MUST reference `coverlet.collector`; if the package reference is missing, it MUST be added without introducing source code or business logic changes.
- **FR-024**: The repository MUST maintain exactly one canonical coverage settings file at `codecoverage.runsettings` in the repository root.
- **FR-025**: `Directory.Build.props` MUST define `RunSettingsFilePath` using `$(MSBuildThisFileDirectory)codecoverage.runsettings` to support nested test project paths.
- **FR-026**: Coverage configuration MUST exclude `xunit` and `*.Tests` assemblies from coverage results.
- **FR-027**: Coverage configuration MUST exclude source paths under `.specify/`, `specs/`, and `tests/` where source-path filtering is supported by the active collector.
- **FR-028**: CI coverage reporting MUST continue to consume Cobertura output via ReportGenerator and publish both job-summary and PR-comment outputs.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The workflow file exists at `.github/workflows/build.yml` and is syntactically valid.
- **SC-002**: Every push to `main` and every pull request to `main` automatically triggers the workflow without manual intervention.
- **SC-003**: A codebase with no warnings, no errors, and all tests passing results in a green (passing) workflow run 100% of the time.
- **SC-004**: A codebase with at least one build warning results in a failed workflow run 100% of the time.
- **SC-005**: A codebase with at least one failing test results in a failed workflow run 100% of the time.
- **SC-006**: Subsequent workflow runs (after an initial cache-warm run) complete the restore step faster due to NuGet package caching.
- **SC-007**: The restore, build, and test steps are individually named and identifiable in the workflow run log.
- **SC-008**: Running `dotnet test --collect:"Code Coverage"` at repository root produces coverage output governed by repository runsettings without requiring manual IDE configuration.
- **SC-009**: CI test commands collect with `--collect:"XPlat Code Coverage"` and still produce Cobertura coverage files consumable by ReportGenerator.
- **SC-010**: Coverage outputs and summaries exclude test assemblies consistently across local and CI execution paths.

## Assumptions

- The SpecKitApi solution builds successfully on the current codebase before this workflow is introduced.
- The latest stable .NET 10 SDK is available as a supported version on GitHub Actions hosted runners at time of implementation.
- All existing tests pass on the current codebase; the workflow is not expected to retroactively fix any pre-existing failures.
- NuGet package cache keys will be derived from the `packages.lock.json` or `*.csproj` files to ensure cache correctness.
- The workflow will run on the default GitHub-hosted Linux runner (`ubuntu-latest`) as a cost-effective and widely supported option.
- The Release configuration is used for both build and test to match production build settings, as this was explicitly specified.
- No matrix builds (multiple OS or SDK versions) are required for this initial CI implementation.
