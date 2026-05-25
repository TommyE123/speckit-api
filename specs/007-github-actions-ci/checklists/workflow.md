# Workflow Behavioral Contracts Checklist: GitHub Actions CI Build Workflow

**Purpose**: Validates the quality of requirements governing runtime environment configuration, inter-step data path contracts, configuration file dependencies, branch protection integration, observability, and workflow lifecycle maintenance — testing whether *these requirements are well-specified*, not whether the implementation behaves correctly. Complements `ci.md` (requirements quality), `implementation.md` (cross-doc consistency), and `security.md` (supply chain).
**Created**: 2026-05-25
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [tasks.md](../tasks.md)

---

## Runtime Environment Requirements

- [ ] CHK001 - Is the decision to use `ubuntu-latest` (a floating runner image tag) documented as an intentional trade-off between runner freshness and reproducibility — given that `ubuntu-latest` can change silently between runs and break the build without a code change? [Clarity, Spec §Assumptions]
- [ ] CHK002 - Are requirements defined for minimum disk space available on the `ubuntu-latest` runner to accommodate NuGet package cache, `./TestResults/`, and `./coveragereport/` storage simultaneously — or is unlimited runner disk space explicitly assumed? [Completeness, Gap]
- [ ] CHK003 - Is it specified whether `ubuntu-latest` pre-installs any .NET SDK version before `actions/setup-dotnet@v5.2.0` runs, and whether a pre-installed SDK could conflict with or shadow the `10.0.x` version installation required by FR-003? [Clarity, Spec §FR-003, Spec §Assumptions]
- [ ] CHK004 - Are requirements defined for workflow behaviour when `ubuntu-latest` is temporarily unavailable or GitHub runner capacity is exhausted — should the workflow queue, fail immediately, or self-cancel after a timeout? [Edge Case, Gap]
- [ ] CHK005 - Is the floating `ubuntu-latest` runner label considered sufficient for reproducibility, or should requirements mandate a pinned runner image (e.g., `ubuntu-24.04`) to prevent unexpected OS upgrades from silently changing build behaviour between runs? [NFR, Gap]

---

## Configuration File Dependency Requirements

- [ ] CHK006 - Is the `codecoverage.runsettings` file referenced in FR-012 specified with its required location relative to the repository root — and is there a requirement that this file MUST exist in the repository before the CI workflow is merged? [Completeness, Spec §FR-012]
- [ ] CHK007 - Are the required contents of `codecoverage.runsettings` documented in spec or plan as named requirements — specifically, which data collector is activated and what Cobertura XML output format it produces — rather than treating it as an opaque pre-existing file the workflow must blindly reference? [Clarity, Spec §FR-012, Gap]
- [ ] CHK008 - Is `Directory.Build.props`'s `TreatWarningsAsErrors=true` property documented as a named dependency in spec, with a requirement that this property MUST remain in place for FR-008 to be reliably satisfied — and is the consequence of its removal (build warnings no longer failing the pipeline) documented? [Completeness, Spec §FR-008, Spec §Assumptions]
- [ ] CHK009 - Are requirements defined for workflow behaviour if `codecoverage.runsettings` is deleted or renamed — does `dotnet test` fail with a clear error or silently succeed without producing coverage data, and is this silent-failure mode addressed in requirements? [Edge Case, Gap]
- [ ] CHK010 - Is the `coverlet.collector@10.0.1` version documented as a minimum acceptable version in spec, or is any `coverlet.collector` version sufficient to satisfy FR-023 — and if a version constraint exists, is it stated as a requirement rather than only an assumption in plan.md? [Clarity, Spec §FR-023, plan.md §Summary]

---

## Inter-Step Data Flow Contract Requirements

- [ ] CHK011 - Is the `./TestResults/` path contract — shared by `--results-directory ./TestResults` (FR-012), `path: TestResults/` in the artifact upload step (FR-016), and `path: TestResults/**/*.trx` in `dorny/test-reporter` (FR-017) — explicitly specified in requirements as a shared, named contract rather than independently stated in three separate requirements with no cross-reference? [Consistency, Spec §FR-012/FR-016/FR-017, Gap]
- [ ] CHK012 - Is the GUID subdirectory nesting that XPlat Code Coverage writes into (`./TestResults/<GUID>/coverage.cobertura.xml`) documented in requirements as the explicit rationale for the `**/coverage.cobertura.xml` glob in FR-013 — so an implementer who uses `./TestResults/coverage.cobertura.xml` instead understands why the glob will never match? [Clarity, Spec §FR-012/FR-013, Gap]
- [ ] CHK013 - Is the `coveragereport/` directory contract — produced by FR-013 (`targetdir: 'coveragereport'`) and consumed by FR-020 (`cat coveragereport/SummaryGithub.md`), FR-014 (`path: coveragereport/`), and FR-022 (`path: coveragereport/SummaryGithub.md`) — explicitly specified as a shared, named output contract in requirements, not as four independently stated paths that happen to agree? [Consistency, Spec §FR-013/FR-014/FR-020/FR-022, Gap]
- [ ] CHK014 - Is the filename `SummaryGithub.md` within `coveragereport/` documented as a deterministic, versioned output filename of the `MarkdownSummaryGithub` report type — validated in research.md against the actual ReportGenerator output — rather than assumed to be stable across ReportGenerator versions? Both FR-020 and FR-022 depend on this exact filename. [Assumption, Spec §FR-013/FR-020/FR-022]
- [ ] CHK015 - Are requirements defined for whether `--settings codecoverage.runsettings` (FR-012) is compatible with `--no-build` on `dotnet test` — is the settings file evaluated at test execution time (compatible) or at project load time (potentially affected), and is this compatibility confirmed as a named assumption? [Clarity, Spec §FR-012, Gap]

---

## Branch Protection & Repository Integration Requirements

- [ ] CHK016 - Are requirements defined for branch protection rules on `main` that mandate the CI workflow's `build` job as a required status check — without which the workflow provides feedback but cannot block merges of broken code? [Completeness, Gap]
- [ ] CHK017 - Is the required status check name — which must exactly match the `build` job name in the workflow YAML to be effective in branch protection — specified in requirements so that repository settings can be configured consistently with the workflow? [Completeness, Gap]
- [ ] CHK018 - Are requirements defined for whether the CI workflow result must be non-stale (i.e., based on the latest commit in the PR) before a merge is permitted, or is a previously passing result on an older commit in the same branch acceptable? [Completeness, Gap]
- [ ] CHK019 - Are requirements defined for `[skip ci]` commit message behaviour — should including `[skip ci]` in a commit message skip the workflow, and if so, is this an intentional capability or an unacceptable bypass of the gate? [Coverage, Gap]
- [ ] CHK020 - Are requirements defined for whether a workflow status badge for the `build` workflow should be displayed in `README.md` — and if so, is the badge URL format and placement documented as a requirement? [Completeness, Gap]

---

## Observability & Failure Notification Requirements

- [ ] CHK021 - Are requirements defined for how developers are notified when a push to `main` causes a workflow failure — e.g., GitHub email notifications, Slack webhook, or other alerting mechanism — beyond the implicit GitHub UI notification? [Completeness, Gap]
- [ ] CHK022 - Are requirements defined for the acceptable time-to-fix for a failing workflow run on `main` — i.e., is there a mean-time-to-resolve (MTTR) target, or is `main` permitted to remain broken indefinitely until a developer notices? [NFR, Gap]
- [ ] CHK023 - Is the GitHub Actions Job Summary (written by FR-020) documented as the authoritative observability surface for coverage data within a workflow run, or are requirements silent on whether alternative dashboards, external coverage services, or long-term trend tracking are expected? [Clarity, Spec §FR-020, Gap]
- [ ] CHK024 - Are requirements defined for the fallback behaviour a developer should take when the sticky PR comment (FR-022) fails to post — e.g., find coverage data in the `coverage-report` artifact or the Job Summary — so the coverage reporting pipeline has a documented degradation path? [Coverage, Spec §FR-022, Gap]
- [ ] CHK025 - Are requirements defined for distinguishing a workflow failure caused by test failures (FR-007) from a workflow infrastructure failure (e.g., `actions/upload-artifact` unavailable) — should both produce the same PR check outcome, or is a differentiated failure reporting requirement needed? [Clarity, Gap]

---

## Workflow Maintenance & Lifecycle Requirements

- [ ] CHK026 - Are requirements defined for the process by which `.github/workflows/build.yml` can be safely modified after initial merge — e.g., must changes go through a new spec or plan phase, or can they be made informally without documentation? [NFR, Gap]
- [ ] CHK027 - Are requirements defined for action version update cadence — when a new major version of `actions/checkout`, `actions/setup-dotnet`, `actions/upload-artifact`, or `dorny/test-reporter` is released, is there a requirement to evaluate and update within a defined timeframe? [NFR, Gap]
- [ ] CHK028 - Are requirements defined for who is responsible for maintaining `.github/workflows/build.yml` after initial implementation — is there a named owner, a CODEOWNERS entry, or a team assignment documenting ongoing accountability? [Completeness, Gap]
- [ ] CHK029 - Are requirements defined for rollback of the workflow file — if T018 live validation reveals a broken workflow, what is the defined recovery procedure, and is the acceptable time-to-recovery specified? [Edge Case, Gap]
- [ ] CHK030 - Is the exclusion of matrix builds (multiple OS/SDK versions) documented in spec with an explicit rationale, and are requirements defined for the conditions under which matrix builds should be introduced in a future iteration — or is the exclusion treated as permanent? [Scope, Spec §Assumptions]

---

## T018 Live Acceptance Validation Requirements Quality

- [ ] CHK031 - Are the acceptance criteria for T018 in tasks.md specific enough to independently validate each of the 23 functional requirements (FR-001–FR-023), or do they only verify the observable green/red workflow outcome without tracing individual FR compliance? [Completeness, tasks.md §T018]
- [ ] CHK032 - Is "all 10 named steps appear in the Actions run log" in tasks.md T018 specific about which 10 steps are expected — and is this count intentional given that plan.md describes a 12-step workflow? If "10" is a documentation error, are the correct step names listed anywhere as a T018 success criterion? [Clarity, tasks.md §T018, plan.md §Workflow Design]
- [ ] CHK033 - Are requirements defined for the minimum set of scenarios T018 must exercise — is a single passing push to `main` and a single passing PR sufficient, or must all four acceptance scenario groups (US1 SC1–3, US2 SC1–2, US3 SC1–2, Edge Cases) be exercised before T018 is marked complete? [Completeness, Spec §User Stories, tasks.md §T018]
- [ ] CHK034 - Is there a requirement specifying that T018 must validate the US3 "warm cache" vs "cold cache" distinction with at least two consecutive workflow runs on the same branch — without which SC-006 ("restore step faster due to caching") cannot be verified? [Completeness, Spec §SC-006, tasks.md §T018]
- [ ] CHK035 - Are requirements defined for how T018 validates FR-008 (build warnings treated as errors) — does live validation require pushing a deliberate build-warning commit to confirm the workflow fails, or is static review of the `--warnaserror` flag sufficient? [Coverage, Spec §FR-008, tasks.md §T018]
- [ ] CHK036 - Are requirements defined for how T018 validates FR-007 (workflow fails when tests fail) — does live validation require introducing a deliberate failing test to confirm failure propagation, or is static analysis of `dotnet test` exit code behaviour sufficient? [Coverage, Spec §FR-007, tasks.md §T018]
- [ ] CHK037 - Is there a documented requirement for who must sign off on T018 completion before the feature branch is merged — is developer self-certification sufficient, or is peer review of the live workflow results required as a named acceptance gate? [Completeness, Gap]

---

## Notes

- Check items off as completed: `[x]`
- Add findings or commentary inline below each item
- Items marked `[Gap]` represent requirements entirely absent from the spec — decide whether to add them or explicitly document them as out-of-scope
- Items marked `[Assumption]` flag undocumented assumptions in plan.md or research.md that have not been elevated to spec-level requirements
- Items marked `[Consistency]` flag implicit path contracts shared across multiple requirements that are never explicitly named as a shared contract
- **Highest-priority items for T018 gate**: CHK031–CHK037 validate whether T018's acceptance criteria are rigorous enough to certify FR-001–FR-023 compliance before merge
- **Highest-risk gaps**: CHK011–CHK014 (inter-step path contracts) and CHK006–CHK008 (config file dependencies) — these silent dependencies could cause post-merge failures that are difficult to diagnose
