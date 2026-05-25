# Research: GitHub Actions CI Build Workflow

**Feature**: 007-github-actions-ci
**Phase**: 0 — Research
**Status**: Complete — all unknowns resolved

---

## Research Item 1: .NET 10 SDK availability on GitHub-hosted runners

**Decision**: Use `actions/setup-dotnet@v4` with `dotnet-version: '10.0.x'`

**Rationale**: .NET 10 was released as GA in November 2024 and is available on `ubuntu-latest`
runners. Using `actions/setup-dotnet` with a version specifier is the idiomatic, portable
approach — it guarantees the correct SDK family is active regardless of the runner image's
pre-installed state, and `10.0.x` auto-selects the latest patch release without requiring
workflow file updates on each .NET patch.

**Alternatives considered**:

- **Hardcode a specific patch** (e.g., `10.0.5`) — rejected: requires manual workflow updates on
  each .NET patch release; no benefit to pinning at the patch level for a build-only workflow.
- **Rely on pre-installed SDK without `setup-dotnet`** — rejected: fragile; the pre-installed
  SDK version on `ubuntu-latest` may lag behind `10.0.x` and cannot be pinned
  independently of the runner image.
- **Add `global.json`** — the most precise approach for version pinning, but FR-011 prohibits
  source/structural file changes outside `.github/workflows/build.yml`. Using `dotnet-version:
  '10.0.x'` in the workflow satisfies the requirement without modifying the repo tree.

---

## Research Item 2: NuGet package cache strategy

**Decision**: Use `actions/cache@v4` with:
```
key:          ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
restore-keys: ${{ runner.os }}-nuget-
path:         ~/.nuget/packages
```

**Rationale**: The repository contains no `packages.lock.json` (lock files are not enabled on
either project). Hashing all `*.csproj` files captures every `<PackageReference>` entry across
both `SpecKitApi.csproj` and `SpecKitApi.Tests.csproj`. When any package version is added,
removed, or bumped, the hash changes and the cache is invalidated — packages are re-downloaded
from NuGet.org (satisfying the edge case in spec §Edge Cases, item 4). The restore-key prefix
allows partial hits when only a subset of packages changes.

The NuGet global packages store on Linux runners is `~/.nuget/packages`.

**Alternatives considered**:

- **Hash `packages.lock.json`** — the preferred best practice for deterministic cache
  invalidation, but no lock file exists. Generating one would require enabling
  `RestoreLockedMode` in project files — a source file change prohibited by FR-011.
- **`actions/setup-dotnet` built-in cache** (`cache: 'nuget'`) — simpler one-liner, but its
  implementation also keys on `packages.lock.json` and silently degrades to no caching when the
  lock file is absent. Explicit `actions/cache@v4` is required here for reliable caching.
- **No caching** — directly contradicts FR-009 (MUST cache NuGet packages) and SC-006
  (subsequent runs complete faster).

---

## Research Item 3: TreatWarningsAsErrors enforcement (FR-008)

**Decision**: Add `--warnaserror` to the `dotnet build` command as required by FR-008.

**Rationale**: FR-008 explicitly states the workflow MUST treat build warnings as errors "by
running the build step with `--warnaserror`". The flag is mandatory per the spec regardless of
other project settings.

Additionally, the repository root already contains:

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

MSBuild imports this file automatically for every project, making the `--warnaserror` flag
technically redundant at the MSBuild level. However, FR-008's explicit requirement makes the
flag non-negotiable. Including it in the YAML also makes the intent self-documenting — anyone
reading the workflow file sees warnings-as-errors enforced without needing knowledge of
`Directory.Build.props`.

**Alternatives considered**:

- **Rely solely on `Directory.Build.props`** — satisfies the runtime behaviour but violates
  FR-008's explicit "by running the build step with `--warnaserror`" wording. Rejected.
- **`-p:TreatWarningsAsErrors=true`** — equivalent MSBuild property override via CLI; less
  idiomatic than the dedicated `--warnaserror` flag. Rejected in favour of the canonical flag.

---

## Research Item 4: dotnet CLI command flags for each step

**Decision**:

| Step | Command |
|------|---------|
| Restore | `dotnet restore` |
| Build | `dotnet build --configuration Release --no-restore --warnaserror` |
| Test | `dotnet test --configuration Release --no-build` |

**Rationale**:

- `--no-restore` on build: prevents an implicit second restore, keeping the Restore step's work
  from being duplicated. This is required for SC-007 (individually identifiable steps).
- `--no-build` on test: prevents an implicit rebuild, ensuring the test step runs against the
  exact Release-configuration binaries produced in the Build step.
- `--configuration Release` on both build and test: matches production build settings as
  required by FR-005 and FR-006.
- No explicit solution file argument: `dotnet` CLI auto-discovers `SpecKitApi.slnx` in the
  repository root. `.slnx` format is supported natively from .NET 9+; no workaround needed.

**Alternatives considered**:

- **`dotnet test` without `--no-build`** — causes an implicit rebuild (potentially in Debug
  configuration if `--configuration` is omitted), wasting time and risking testing different
  artifacts than those produced in the Build step. Rejected.
- **Explicit solution file argument** (e.g., `dotnet build SpecKitApi.slnx`) — acceptable in
  multi-solution repos, but unnecessary and adds fragility if the solution file is renamed.
  Auto-discovery is reliable for a single-solution repository.

---

## Research Item 5: Action versions

**Decision**: `actions/checkout@v6.0.2`, `actions/setup-dotnet@v5.2.0`, `actions/cache@v4`, `actions/upload-artifact@v7.0.1`

**Rationale**: `actions/checkout@v6.0.2` is explicitly required by FR-018 (pinned patch version). `actions/setup-dotnet@v5.2.0` is explicitly required by FR-003 (pinned patch version). `actions/upload-artifact@v7.0.1` is explicitly required by FR-014 for the coverage report artifact upload; the same version is used for the TRX test results artifact upload (FR-016) for consistency. `actions/cache@v4` remains on its current latest major version. Pinning to specific patch versions (`v6.0.2`, `v5.2.0`, `v7.0.1`) provides maximum reproducibility for the required actions.

**Alternatives considered**:

- **`actions/checkout@v4`** — the previous major version; superseded by v6.0.2 which is required by FR-018. Rejected.
- **`actions/setup-dotnet@v5`** (major-version tag) — receives automatic patch/minor updates but drifts from the version the spec explicitly pins. Rejected; FR-003 specifies `@v5.2.0`.
- **`actions/upload-artifact@v4`** — the previous major version, superseded by v7 which is required by FR-014. Rejected.
- **Pin to exact commit SHA** — maximum reproducibility for all actions; adds maintenance overhead. Appropriate for security-critical workflows; not required here beyond the explicit pinning already specified by the spec.

---

## Research Item 6: TRX test result artifact upload (FR-016)

**Decision**: Use `actions/upload-artifact@v7.0.1` with `name: test-results`, `path: TestResults/`, and `if: always()`.

**Rationale**: The spec clarification session (2026-05-25) confirmed: "How should test results be retained without inline PR reporting? → Upload TRX test result files as a build artifact using if: always()." No inline Check Run (`dorny/test-reporter`) is needed. A simple artifact upload preserves TRX diagnostics for download from the Actions run summary, even when tests fail. Version `@v7.0.1` is used (matching the upload-artifact version required by FR-014) for consistency across all artifact upload steps.

**Key finding — no `checks: write` required**: Dropping inline Check Run reporting means the job no longer needs `checks: write` permission. No `permissions:` block is required; the default read-only token is sufficient.

**Alternatives considered**:

- **`dorny/test-reporter@v1`** — creates an inline Check Run on PRs; requires `checks: write`; unavailable on fork PRs. The spec clarification explicitly chose artifact upload over inline reporting. Rejected.
- **`actions/upload-artifact@v4`** — older major version; using v7.0.1 for consistency with FR-014's requirement. Rejected.

---

## Research Item 7: Coverage collection — XPlat Code Coverage (FR-012)

**Decision**: Add `--collect:"XPlat Code Coverage" --logger trx --results-directory ./TestResults` to the `dotnet test` command.

**Rationale**: `--collect:"XPlat Code Coverage"` activates the `coverlet.collector` DataCollector, which is already referenced in `SpecKitApi.Tests.csproj` (added by `dotnet new xunit`). No new NuGet reference is needed. The `--results-directory` flag controls the output location for both the TRX file and the coverage XML.

**Key finding — coverage file location**: XPlat Code Coverage places `coverage.cobertura.xml` inside a **random GUID subdirectory** under `--results-directory`:
```
./TestResults/
  ├── SpecKitApi.Tests_2026-05-25_12-00-00.trx
  └── <random-guid>/
        └── coverage.cobertura.xml
```
The glob `**/coverage.cobertura.xml` is mandatory for ReportGenerator to find the file. A path like `TestResults/coverage.cobertura.xml` would never match.

**Alternatives considered**:

- **`coverlet.msbuild`** — an alternative coverage driver that writes to a predictable path, but requires adding a `<PackageReference>` to the test project (prohibited by FR-011). Rejected.
- **`--results-directory` without `--logger trx`** — valid for coverage-only, but FR-013 requires TRX output. Both flags are composable; both respect `--results-directory`.

---

## Research Item 8: Coverage report generation — danielpalme/ReportGenerator-GitHub-Action@5.5.10 (FR-013)

**Decision**: Use `danielpalme/ReportGenerator-GitHub-Action@5.5.10` with `reports: '**/coverage.cobertura.xml'`, `targetdir: 'coveragereport'`, `reporttypes: 'HtmlInline;Cobertura'`, and `if: always()`.

**Rationale**: FR-013 explicitly pins version `5.5.10`. The action **self-installs** `dotnet-reportgenerator-globaltool` at runtime — no manual `dotnet tool install` step is needed. The only hard prerequisite is a working `dotnet` binary on `PATH`, which is satisfied by the preceding `Setup .NET` step.

**Key finding — no manual install**: The action runs `dotnet tool install dotnet-reportgenerator-globaltool --tool-path ./reportgeneratortool` internally on each run (with an existence check to skip if already present). Adding a manual install step would be redundant.

**Key finding — ordering**: This step must run after `Test` so the `coverage.cobertura.xml` file exists. It uses `if: always()` so it runs even when the Test step fails (FR-015).

**Alternatives considered**:

- **`@5.5.9`** — one patch older. Rejected; the spec explicitly requires `5.5.10` (FR-013).
- **Manual `dotnet tool install` + `reportgenerator` CLI** — more portable but adds two extra steps. The dedicated action is simpler and is what the spec calls for.

---

## Research Item 9: Coverage artifact upload — actions/upload-artifact@v7.0.1 (FR-015)

**Decision**: Use `actions/upload-artifact@v7.0.1` with `name: coverage-report`, `path: coveragereport/`, and `if: always()`.

**Rationale**: FR-014 explicitly requires `actions/upload-artifact@v7.0.1` for uploading the generated coverage report. The `path: coveragereport/` trailing slash uploads the directory contents. The artifact is named `coverage-report` and downloadable from the Actions run summary page.

**Key finding — artifact name uniqueness**: In `upload-artifact@v7`, each artifact name must be unique per workflow run. Since this workflow has a single job with no matrix, `coverage-report` is unambiguous.

**Alternatives considered**:

- **`@v4`** — the previous major version; replaced by the explicitly required v7.0.1. Rejected.
- **`if-no-files-found: error`** — stricter, but the spec does not require failure if coverage report generation fails. Using the default `warn` behaviour is appropriate.

---

## Research Item 10: Inline PR test reporting — dorny/test-reporter@v3.0.0 and fork PR support (FR-017)

**Decision**: Use `dorny/test-reporter@v3.0.0` with `reporter: dotnet-trx`, `path: TestResults/**/*.trx`,
`name: Test Results`, and `if: always()` as a named step placed immediately after the Upload Test
Results step. Add `permissions: { checks: write, actions: read }` to the job.

**Rationale**: FR-017 requires inline PR test result publication for all PRs, including
fork-originated PRs. This creates a fundamental tension with GitHub Actions' security model:

- PRs triggered by **`pull_request`** events from forked repositories receive a **read-only
  `GITHUB_TOKEN`** regardless of the `permissions:` block declared in the workflow. GitHub does
  not grant `checks: write` to fork PR workflows for security reasons.
- Creating a Check Run requires `checks: write`. Under v1, `dorny/test-reporter` **only** created
  Check Runs — so for fork PRs the step failed with a 403 permission error.

**v3.0.0 introduces two key changes that resolve this (PR #745 — "Explicitly use lowest
permissions required"):**

1. **`use-actions-summary: 'true'` is now the default** — the action always writes test results
   to the GitHub Actions Job Summary. Job Summaries are part of the workflow run context and
   require no additional permissions beyond the default read token. This path always succeeds for
   both same-repo PRs and fork PRs.

2. **Graceful permission degradation** — when `checks: write` is unavailable (fork PR context),
   v3 falls back to the Job Summary path only and does not fail the step due to the permission
   restriction. The step's exit status is determined solely by `fail-on-error: true` (fails if
   any test failed) and `fail-on-empty: true` (fails if no TRX files are found) — not by whether
   a Check Run was successfully created.

**Result by PR type:**

| PR type | Check Run | Job Summary | Step result |
|---------|-----------|-------------|-------------|
| Same-repo PR (non-fork) | ✅ Created (checks: write available) | ✅ Always written | Pass if tests pass |
| Fork PR | ❌ Not created (token read-only) | ✅ Always written | Pass if tests pass |

This satisfies FR-017's requirement that publication succeeds for all PRs: "publication" is the
Job Summary (always succeeds); `fail-on-error: true` causes the workflow to fail if tests fail,
satisfying "fail the workflow if publication is unsuccessful" for the test-failure case. No
`continue-on-error` is needed or used.

**Key finding — permissions block required**: The job-level `permissions:` block must be added
with `checks: write` and `actions: read`. Without `checks: write` declared in the workflow, same-repo
PRs would also be denied Check Run creation (GitHub downgrades to the minimum required). Adding
the permissions block ensures same-repo PRs get the full Check Run while fork PRs fall back to
Job Summary.

**Key finding — action inputs**: The `name:` input is required (action.yml: `required: true`).
Using `name: Test Results` is idiomatic. The `path: TestResults/**/*.trx` glob matches all TRX
files regardless of subdirectory depth, consistent with the XPlat Code Coverage GUID subdirectory
layout already documented in Research Item 7.

**Key finding — no two-workflow pattern needed**: The recommended approach for v1 (and still
documented in v3's README as an alternative for public repos) was a two-workflow
`workflow_run` pattern: a CI workflow uploads the TRX artifact, and a separate `test-report`
workflow with `checks: write` downloads and publishes it. With v3's Job Summary fallback this
pattern is no longer required to satisfy FR-017. The single-workflow design is preserved.

**Alternatives considered**:

- **Two-workflow `workflow_run` pattern** — still the most robust approach for Check Run creation
  on fork PRs; rejected because FR-017 specifies placement immediately after Upload Test Results
  in the main build workflow, and v3's Job Summary fallback satisfies the requirement without
  splitting the workflow.
- **`pull_request_target` trigger** — runs in base-repo context with `checks: write` even for
  fork PRs; rejected because it executes potentially untrusted fork code with elevated permissions,
  creating a security risk for public repositories. Not acceptable for a standard CI workflow.
- **`dorny/test-reporter@v1`** — only creates Check Runs; fails for fork PRs; incompatible with
  FR-017. Rejected; the spec explicitly requires v3.0.0.
- **`continue-on-error: true`** — would suppress failure when tests fail, violating FR-017's
  requirement that failures fail the workflow. Explicitly rejected.

---

## Resolved Unknowns Summary

| Unknown | Resolution |
|---------|------------|
| .NET 10 SDK on `ubuntu-latest` | ✅ Available; use `actions/setup-dotnet@v5.2.0` with `dotnet-version: '10.0.x'` |
| NuGet cache key (no lock file present) | ✅ Hash `**/*.csproj`; restore-key prefix `${{ runner.os }}-nuget-` for partial hits |
| Warnings-as-errors enforcement | ✅ FR-008 requires `--warnaserror` on `dotnet build`; `Directory.Build.props` also enforces it (flag is redundant at runtime but mandatory per spec) |
| Build and test command flags | ✅ `--warnaserror` on build, `--no-restore` on build, `--no-build` on test, both `--configuration Release` |
| Action versions | ✅ `checkout@v6.0.2` (FR-018), `setup-dotnet@v5.2.0`, `cache@v4`, `upload-artifact@v7.0.1` |
| `.slnx` solution format support | ✅ Natively supported by .NET 9+ CLI; no workaround needed |
| TRX test result preservation (FR-016) | ✅ `actions/upload-artifact@v7.0.1` uploads `TestResults/` as `test-results` artifact; no inline Check Run needed |
| Coverage collection (FR-012) | ✅ `--collect:"XPlat Code Coverage" --logger trx --results-directory ./TestResults`; `coverlet.collector` already referenced in test project |
| Coverage XML path pattern (FR-013) | ✅ XPlat Code Coverage outputs to `TestResults/<guid>/coverage.cobertura.xml`; use glob `**/coverage.cobertura.xml` |
| ReportGenerator version (FR-013) | ✅ `danielpalme/ReportGenerator-GitHub-Action@5.5.10`; action self-installs the tool |
| Coverage artifact upload (FR-014) | ✅ `actions/upload-artifact@v7.0.1` with `name: coverage-report`, `path: coveragereport/` |
| `if: always()` on post-test steps (FR-015) | ✅ Apply to Upload TRX, Publish Test Results, Generate Coverage Report, and Upload Coverage Report steps |
| Inline PR test reporting (FR-017) | ✅ `dorny/test-reporter@v3.0.0` with `reporter: dotnet-trx`, `path: TestResults/**/*.trx`; placed after Upload Test Results |
| fork PR support for dorny/test-reporter | ✅ v3 `use-actions-summary: true` default writes Job Summary (no `checks: write` needed); Check Run additionally created for non-fork PRs |
| Job permissions | ✅ `permissions: { checks: write, actions: read }` required; ensures non-fork PRs get Check Run; fork PRs fall back to Job Summary via v3 graceful degradation |
