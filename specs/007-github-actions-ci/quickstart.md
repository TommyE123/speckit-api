# Quickstart: GitHub Actions CI Build Workflow

**Feature**: 007-github-actions-ci
**Audience**: Developers working on SpecKitApi

---

## What This Workflow Does

The `build.yml` workflow is the sole deliverable of this feature. Once merged to `main`, it
runs automatically on every push and pull request targeting `main` and executes the following
individually named steps:

**Build steps**:
1. **Restore** — downloads NuGet packages (cache hit on warm runs)
2. **Build** — compiles the solution in Release configuration; any build warning fails the step
   (enforced by `Directory.Build.props`)
3. **Test** — runs the full xUnit v3 test suite in Release configuration, collecting XPlat Code
   Coverage and writing results to TRX format

**Post-test reporting steps** (all run with `if: always()` unless noted, even if tests fail):
4. **Upload Test Results** — uploads the `TestResults/` directory as a `test-results` artifact for download (TRX files and coverage XML preserved)
5. **Publish Test Results** — publishes inline test results using `dorny/test-reporter@v3.0.0`; creates a Check Run visible directly on the PR for same-repo pull requests, and writes a GitHub Actions Job Summary (visible in the workflow run summary) for all pull requests including fork-originated ones. Fails the workflow if any test failed or if no TRX files are found.
6. **Generate Coverage Report** — produces an HTML + Cobertura + Markdown (MarkdownSummaryGithub) coverage report; excludes test assemblies (`assemblyfilters: '-*.Tests*'`); outputs `coveragereport/SummaryGithub.md`
7. **Write Coverage to Job Summary** — appends `coveragereport/SummaryGithub.md` to `$GITHUB_STEP_SUMMARY` so coverage appears on the workflow run summary page
8. **Upload Coverage Report** — uploads the generated report directory as a downloadable build artifact named `coverage-report` (retained 14 days)
9. **Post Coverage Summary PR Comment** — posts a sticky PR comment with the coverage summary (only on `pull_request` events); updates in-place on each run (`recreate: true`)

---

## Workflow File Location

```
.github/workflows/build.yml
```

This is the only file added to the repository by this feature.

---

## Triggering the Workflow

The workflow triggers **automatically** — no manual action is required.

| Event | When it fires |
|-------|---------------|
| `push` | Any commit pushed directly to `main` |
| `pull_request` | Any PR opened, synchronized (new commits pushed), or reopened targeting `main` |

---

## Reading Results

### GitHub Actions tab

1. Navigate to the **Actions** tab in the GitHub repository.
2. Select the **Build** workflow from the left sidebar.
3. Click any run to see the full step-by-step log, including each named step's output.

### Pull request checks

When a PR targets `main`, the workflow result appears in the PR's **Checks** section:

- ✅ Green tick → all steps (including post-test reporting) passed
- ❌ Red X → at least one step failed; click **Details** to see which step and why
- For same-repo PRs, a **Test Results** Check Run appears inline on the PR (created by `dorny/test-reporter`) with a pass/fail count and annotation links
- For fork PRs, test results appear in the **GitHub Actions Job Summary** on the workflow run page (Check Run creation is unavailable for fork PRs due to GitHub's token security model; v3.0.0 writes the Job Summary as a fallback)
- The **test-results** artifact is downloadable from the workflow run summary page (TRX files and coverage XML)
- The **coverage-report** artifact is downloadable from the workflow run summary page even if tests fail (retained 14 days)
- A **coverage summary table** appears directly in the workflow run summary (written to `$GITHUB_STEP_SUMMARY` by the Write Coverage to Job Summary step)
- A **sticky PR comment** with the coverage summary is posted/updated on each pull request run (via `marocchino/sticky-pull-request-comment`)

---

## NuGet Cache Behaviour

| Run | Cache state | Behaviour |
|-----|-------------|-----------|
| First run (cold cache) | Miss | Packages downloaded from NuGet.org; cache saved at job end |
| Subsequent runs (warm cache) | Hit | `dotnet restore` reads from `${{ github.workspace }}/.nuget/packages`; NuGet.org not contacted |
| After any `.csproj` change | Miss (invalidated) | Hash of `**/*.csproj` changes; full download + cache refresh |

**Cache key format**: `Linux-nuget-<SHA256 of all *.csproj files>`

---

## Interpreting Failures

| Failing step | Most likely cause | How to investigate |
|--------------|-------------------|--------------------|
| **Restore** | NuGet.org unreachable, or a malformed `<PackageReference>` | Check the restore log for package resolution errors |
| **Build** | Compilation error, or a build warning (warnings are errors) | Read the MSBuild output; warnings from Roslynator analyzers are common sources |
| **Test** | At least one xUnit test failed, or the test host crashed | The test step output lists failed test names and failure messages |
| **Upload Test Results** | No TRX files found under `TestResults/` | Confirm `--logger trx --results-directory ./TestResults` is present on the test command |
| **Publish Test Results** | At least one test failed (`fail-on-error: true`), or no TRX files found (`fail-on-empty: true`) | Check the test output or the artifact upload step; if TRX files exist and tests passed, this step should not fail |
| **Generate Coverage Report** | No `coverage.cobertura.xml` found (test step failed before coverage was written) | Coverage is written by the Test step; if Test failed, there may be no XML to process |
| **Write Coverage to Job Summary** | `coveragereport/SummaryGithub.md` does not exist | Requires `MarkdownSummaryGithub` in `reporttypes` on the Generate Coverage Report step |
| **Upload Coverage Report** | `coveragereport/` directory does not exist (Report Generator step failed) | Check the Generate Coverage Report step log for errors |
| **Post Coverage Summary PR Comment** | `coveragereport/SummaryGithub.md` missing, or `pull-requests: write` permission absent | Check that `pull-requests: write` is in the job `permissions:` block and that coverage generation succeeded |

---

## Workflow Reference

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
        uses: marocchino/sticky-pull-request-comment@v3.0.4
        if: github.event_name == 'pull_request'
        with:
          recreate: true
          path: coveragereport/SummaryGithub.md
```

> **Note on warnings-as-errors**: The `--warnaserror` flag is required by FR-008. `Directory.Build.props`
> at the repository root also sets `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` for all
> projects — the CLI flag is redundant at the MSBuild level but mandatory per spec.
>
> **Note on coverage XML path**: XPlat Code Coverage writes `coverage.cobertura.xml` into a
> random GUID subdirectory under `--results-directory`. The glob `**/coverage.cobertura.xml`
> in the Generate Coverage Report step is mandatory to locate the file correctly.
>
> **Note on inline PR test results and fork PRs**: The `Publish Test Results` step uses
> `dorny/test-reporter@v3.0.0` with `permissions: { checks: write, actions: read, pull-requests: write }` declared on
> the job. For same-repo PRs, this creates an inline Check Run. For fork-originated PRs,
> GitHub's security model provides a read-only token regardless of the `permissions:` declaration,
> so the Check Run cannot be created; `dorny/test-reporter` v3 falls back to writing a GitHub
> Actions Job Summary.
>
> **Note on sticky PR comment**: `marocchino/sticky-pull-request-comment@v3.0.4` requires
> `pull-requests: write` in the job `permissions:` block. The step is gated with
> `if: github.event_name == 'pull_request'` so it skips on push events. `recreate: true` ensures
> the comment is replaced (not duplicated) on each run. The file `coveragereport/SummaryGithub.md`
> is produced by ReportGenerator when `reporttypes` includes `MarkdownSummaryGithub`.
>
> **Note on NUGET_PACKAGES env var**: The workflow-level `env:` block declares
> `NUGET_PACKAGES=${{ github.workspace }}/.nuget/packages`. The cache step references this via
> `path: ${{ env.NUGET_PACKAGES }}`. Both `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true` and
> `DOTNET_CLI_TELEMETRY_OPTOUT=true` reduce startup noise in CI logs.

---

## Success Criteria Cross-Reference

| Success Criterion | Verified by |
|-------------------|-------------|
| SC-001: Workflow file exists and is syntactically valid | CI runner parses and executes the YAML |
| SC-002: Automatic trigger on push/PR to `main` | `on: push/pull_request` trigger configuration |
| SC-003: Green run on clean codebase | All steps exit 0 |
| SC-004: Failed run on build warning | `TreatWarningsAsErrors=true` causes MSBuild to return non-zero |
| SC-005: Failed run on test failure | `dotnet test` returns non-zero on test failure |
| SC-006: Faster restore on warm cache | `actions/cache@v4` cache hit skips NuGet.org downloads |
| SC-007: Individually named steps | `name:` field on all 12 steps (Checkout through Post Coverage Summary PR Comment) |
