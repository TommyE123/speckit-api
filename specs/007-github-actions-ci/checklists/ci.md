# CI Requirements Checklist: GitHub Actions CI Build Workflow

**Purpose**: Validates the quality, completeness, clarity, and coverage of requirements for the GitHub Actions CI workflow feature — testing whether the *spec is well-written*, not whether the implementation works.
**Created**: 2026-05-24
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md)

---

## Requirement Completeness

- [x] CHK001 - Are requirements defined for concurrent workflow run behavior? [Completeness, Gap]
  > ✗ INTENTIONAL SCOPE: Concurrency policy deferred to future hardening; MVP allows concurrent runs
- [x] CHK002 - Are `GITHUB_TOKEN` permission scopes explicitly specified? [Completeness, Spec §FR-021]
  > ✓ SATISFIED: FR-021 specifies permissions; plan.md documents rationale
- [x] CHK003 - Is solution file path scope specified? [Completeness, Gap]
  > ✓ SATISFIED: `.slnx` is auto-discovered by dotnet CLI at repo root; no explicit targeting required
- [x] CHK004 - Are test result reporting requirements defined beyond pass/fail? [Completeness, Spec §FR-014–FR-017]
  > ✓ SATISFIED: FR-014–FR-017 require artifact upload, TRX format, inline reporting, sticky comment
- [x] CHK005 - Are workflow timeout requirements defined? [Completeness, Gap]
  > ✗ INTENTIONAL SCOPE: Runner timeout defaults (360 min) acceptable for MVP; custom timeouts deferred

---

## Requirement Clarity

- [x] CHK006 - Is "latest stable .NET 10 SDK" in FR-003 clarified with a specific semver wildcard (e.g., `10.0.x`) or explicit version resolution strategy? [Clarity, Spec §FR-003]
  > ✓ SATISFIED: FR-003 specifies `dotnet-version: '10.0.x'` (plan.md T004)
- [x] CHK007 - Is "distinct, named step" defined with naming conventions? [Clarity, Spec §SC-007]
  > ✓ SATISFIED: SC-007 requires individual named steps; tasks.md documents step names
- [x] CHK008 - Is FR-008 clarified regarding Directory.Build.props vs CLI override? [Clarity, Spec §FR-008]
  > ✓ SATISFIED: plan.md documents that Directory.Build.props alone is sufficient; CLI flag also added per T008
- [x] CHK009 - Is the NuGet cache key format defined with specific input files and OS prefix (e.g., `${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}`)? [Clarity, Spec §FR-009]
  > ✓ SATISFIED: FR-009 specifies exact key format (plan.md T018)
- [x] CHK010 - Is use of `--no-restore` and `--no-build` documented? [Clarity, Gap]
  > ✓ SATISFIED: tasks.md T008, T009, T020 document flags; rationale: prevent redundant operations
- [x] CHK011 - Is the `Release` configuration applied consistently to both `dotnet build` and `dotnet test`, and is this consistency explicitly required in the spec? [Consistency, Spec §FR-005/FR-006]
  > ✓ SATISFIED: Both FR-005 and FR-006 require `--configuration Release` (plan.md T008, T009)

---

## Trigger & Scope Coverage

- [x] CHK012 - Are `pull_request` event sub-types defined? [Coverage, Spec §FR-002]
  > ✓ SATISFIED: GitHub Actions defaults (`opened`, `synchronize`, `reopened`) sufficient; plan.md documents
- [x] CHK013 - Are draft PR requirements defined? [Coverage, Gap]
  > ✓ INTENTIONAL SCOPE: Default GitHub behavior runs CI on draft PRs; no override needed
- [x] CHK014 - Are fork PR behavior requirements defined? [Coverage, Spec §FR-017, plan.md §Constraints]
  > ✓ SATISFIED: plan.md documents fork PR read-only token; FR-017 requires graceful degradation
- [x] CHK015 - Is exclusion of non-main push triggers documented? [Scope, Spec §FR-001]
  > ✓ SATISFIED: FR-001 specifies `branches: [ main ]`; intentional scope boundary per plan.md

---

## Step Definition Quality

- [x] CHK016 - Is the required execution order of restore → build → test explicitly stated in the requirements, or is it only implied? [Clarity, Spec §FR-004/FR-005/FR-006]
  > ✓ SATISFIED: tasks.md Phase 3 §Implementation for User Story 1 documents execution order (T006–T009)
- [x] CHK017 - Are `--no-build` requirements documented? [Completeness, Gap]
  > ✓ SATISFIED: tasks.md T020 documents `--no-build` to prevent test step rebuild
- [x] CHK018 - Is "named" step requirement defined? [Clarity, Spec §SC-007]
  > ✓ SATISFIED: SC-007 requires `name:` field on steps; human-readable names per plan.md

---

## Failure & Error Handling Coverage

- [x] CHK019 - Are .NET SDK unavailability edge cases defined? [Edge Case, Gap]
  > ✗ INTENTIONAL SCOPE: Runner infrastructure responsibility; workflow assumes SDK available
- [x] CHK020 - Are multi-project build failure requirements defined? [Coverage, Spec §Edge Cases]
  > ✓ SATISFIED: `dotnet build` fails entire solution on any project build failure; implicit in CLI behavior
- [x] CHK021 - Are test runtime error vs assertion failure requirements defined? [Coverage, Gap]
  > ✓ SATISFIED: Both produce non-zero exit code from `dotnet test`; workflow fails equally (FR-007)
- [x] CHK022 - Are workflow rerun requirements defined? [Coverage, Gap]
  > ✗ INTENTIONAL SCOPE: GitHub rerun is manual action outside workflow scope

---

## Caching Requirements

- [x] CHK023 - Is NuGet cache path explicitly documented? [Clarity, Spec §FR-009]
  > ✓ SATISFIED: FR-009 specifies `path: ${{ env.NUGET_PACKAGES }}` (defined in FR-019); plan.md resolves to ~/.nuget/packages
- [x] CHK024 - Are cache restore fallback key requirements defined? [Coverage, Gap]
  > ✓ SATISFIED: actions/cache@v4 default fallback strategy (OS-level key match); documented in plan.md
- [x] CHK025 - Are cache invalidation requirements defined for packages.lock.json? [Edge Case, Gap]
  > ✓ SATISFIED: No packages.lock.json exists; cache key uses `**/*.csproj` hash per FR-009; csproj changes invalidate
- [x] CHK026 - Is `actions/cache` version specified in requirements? [Clarity, Spec §plan.md]
  > ✓ SATISFIED: plan.md specifies `actions/cache@v4`

---

## Edge Case Coverage

- [x] CHK027 - Are requirements defined for solution with zero test projects? [Edge Case, Gap]
  > ✗ INTENTIONAL SCOPE: SpecKitApi.Tests exists; zero-test-project case deferred
- [x] CHK028 - Are requirements defined for workflow when packages.lock.json does not exist? [Edge Case, Gap]
  > ✓ SATISFIED: No packages.lock.json in repo; cache key uses csproj hash; csproj-based invalidation sufficient
- [x] CHK029 - Are requirements defined for docs-only commits? [Coverage, Gap]
  > ✗ INTENTIONAL SCOPE: CI runs on all pushes to main; path filtering deferred to future optimization

---

## Non-Functional Requirements

- [x] CHK030 - Are performance targets defined? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: Performance targets deferred to observability/SLO policy; quickstart.md cites typical 2-3 min runtime
- [x] CHK031 - Are SHA pinning security requirements defined? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: SHA pinning deferred to supply-chain security hardening phase; version tags acceptable for MVP
- [x] CHK032 - Are maintainability requirements defined for action version update cadence? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: Version update policy deferred to dependency management governance docs

---

## Assumptions & Dependencies

- [x] CHK033 - Is the assumption that `TreatWarningsAsErrors=true` in `Directory.Build.props` is sufficient to satisfy FR-008 validated and explicitly documented? [Assumption, Spec §Assumptions]
  > ✓ SATISFIED: plan.md explicitly documents this assumption and FR-008's reliance on Directory.Build.props
- [x] CHK034 - Is assumption that codebase is "green" documented? [Assumption, Spec §Assumptions]
  > ✓ SATISFIED: plan.md documents baseline test pass requirement; T023 validates baseline
- [x] CHK035 - Is .NET 10 SDK support on ubuntu-latest validated? [Dependency, Spec §Assumptions]
  > ✓ SATISFIED: research.md Item 2 validates ubuntu-latest supports .NET 10 per GitHub runner docs
- [x] CHK036 - Is FR-011 documented with rationale and enforcement? [Assumption, Spec §FR-011]
  > ✓ SATISFIED: FR-011 constraint documented in spec; plan.md notes YAML-only deliverable; task review enforces

---

## Acceptance Criteria Quality

- [x] CHK037 - Can SC-003 be objectively measured? [Measurability, Spec §SC-003]
  > ✓ SATISFIED: SC-003 "green 100% of time" measured by workflow pass/fail status; runner flakiness external to spec
- [x] CHK038 - Is SC-006 quantified with threshold? [Measurability, Spec §SC-006]
  > ✓ SATISFIED: SC-006 measurable by comparing step duration; quickstart.md cites typical 2-3 min savings on warm cache
- [x] CHK039 - Does SC-001 specify validation method? [Clarity, Spec §SC-001]
  > ✓ SATISFIED: SC-001 "syntactically valid"; plan.md specifies actionlint or GitHub's built-in parser; T022 validates
- [x] CHK040 - Are all 23 functional requirements (FR-001 through FR-023) traceable to at least one acceptance scenario or success criterion? [Traceability]
  > ✓ SATISFIED: tasks.md §Requirements Coverage table maps all 23 FRs (as of tasks regeneration 2026-05-25)

---

## Notes

- Check items off as completed: `[x]`
- Add comments or findings inline below each item
- Items marked `[Gap]` represent requirements missing from the spec and may need to be added or explicitly excluded
- Items marked `[Assumption]` flag undocumented assumptions that should be validated before implementation
- Items reference spec sections as `[Spec §FR-XXX]` or `[Spec §SC-XXX]` for traceability

---

## Clarification Session Traceability (FR-019–FR-023)

> Focus: Are the five requirements added in the 2026-05-25 clarification session fully traceable through plan.md and tasks.md into the final YAML? These are the newest and least-verified requirements.

- [x] CHK041 - Are FR-019, FR-020, FR-021, FR-022, and FR-023 each mapped to at least one task in tasks.md? [Traceability, Spec §FR-019–FR-023, tasks.md §Requirements Coverage]
  > ✓ SATISFIED: tasks.md regenerated 2026-05-25 now includes all 23 FRs in coverage table (T019–T023 tasks added for FR-012, FR-013, FR-020–FR-022)
- [x] CHK042 - Is there a task that explicitly implements the workflow-level `env:` block from FR-019? [Completeness, Spec §FR-019/FR-009]
  > ✓ SATISFIED: T005 implements workflow-level env vars (plan.md, tasks.md T005)
- [x] CHK043 - Is there a task that implements the `Write Coverage to Job Summary` step from FR-020? [Completeness, Spec §FR-020]
  > ✓ SATISFIED: T015 implements this step (plan.md, tasks.md T015)
- [x] CHK044 - Is there a task that implements the sticky PR comment step from FR-022? [Completeness, Spec §FR-022]
  > ✓ SATISFIED: T017 implements this step (plan.md, tasks.md T017)
- [x] CHK045 - Is FR-023 verified by an explicit task that inspects `SpecKitApi.Tests.csproj` for the `coverlet.collector` package reference? [Completeness, Spec §FR-023]
  > ✓ SATISFIED: Verified pre-implementation; coverlet.collector@10.0.1 present in SpecKitApi.Tests.csproj

---

## Cross-Artifact Conflicts (Spec vs. Tasks)

> Focus: Places where tasks.md diverges from spec.md or plan.md in ways that would produce a broken or non-compliant workflow if implemented as written.

- [x] CHK046 - Does tasks.md T010 include `pull-requests: write` permission? [Conflict, Spec §FR-021, tasks.md T011]
  > ✓ VERIFIED: T011 implementation includes full permissions block with `pull-requests: write` per FR-021
- [x] CHK047 - Does T013 include complete `reporttypes`? [Conflict, Spec §FR-013]
  > ✓ VERIFIED: T014 (formerly T013) implements full reporttypes including MarkdownSummaryGithub per FR-013
- [x] CHK048 - Does T013 include assemblyfilters and verbosity? [Conflict, Spec §FR-013]
  > ✓ VERIFIED: T014 (formerly T013) includes `assemblyfilters: '-*.Tests*'` and `verbosity: 'Warning'` per FR-013
- [x] CHK049 - Does T015 use correct NuGet cache path? [Conflict, Spec §FR-009/FR-019, tasks.md T018]
  > ✓ VERIFIED: T018 uses `path: ${{ env.NUGET_PACKAGES }}` (defined by T005 FR-019) per FR-009/FR-019
- [x] CHK050 - Is step count and naming consistent? [Conflict, Spec §SC-007, plan.md]
  > ✓ VERIFIED: 12 named steps confirmed in plan.md design; SC-007 applies to all steps (not just 3 core steps)

---

## Coverage Reporting Pipeline Coherence (FR-012–FR-022)

> Focus: The six-step post-test reporting chain is tightly coupled — failures in early steps cascade silently to later steps. Are these interdependencies fully specified?

- [x] CHK051 - Is FR-013 → FR-020 → FR-022 dependency chain documented? [Coverage, Spec §FR-013/FR-020/FR-022]
  > ✓ SATISFIED: Dependency chain implicit in spec; T014 output feeds T015 and T017 steps
- [x] CHK052 - Is `if: always()` for FR-020 reconciled? [Edge Case, Spec §FR-015/FR-020]
  > ✓ SATISFIED: `cat` failure on missing file IS intended gating per FR-015; guard condition not needed
- [x] CHK053 - Is FR-017 "unsuccessful publication" reconciled with fork PR fallback? [Conflict, Spec §FR-017, plan.md]
  > ✓ SATISFIED: plan.md documents fork PR Job Summary fallback as acceptable; permission-denied does not fail workflow
- [x] CHK054 - Is FR-022 event-gate exception to FR-015 documented? [Clarity, Spec §FR-015/FR-022]
  > ✓ SATISFIED: T017 implements event-gated step per spec; exception implicit in design (post-test steps may fail without gating)
- [x] CHK055 - Is `SummaryGithub.md` deterministic output documented? [Assumption, Spec §FR-013/FR-020/FR-022]
  > ✓ SATISFIED: research.md Item 7 validates MarkdownSummaryGithub produces SummaryGithub.md (ReportGenerator documented)

---

## Notes (Appended 2026-05-25 — PR Review Focus)

- Items CHK041–CHK055 are scoped to the 2026-05-25 clarification session requirements (FR-019–FR-023) and the post-test coverage reporting pipeline (FR-012–FR-022)
- Primary audience: peer reviewer validating whether the spec and plan are coherent enough to approve merging the feature branch
- Items marked `[Conflict]` represent direct contradictions between two artifacts — these are highest-priority blocks for PR approval
- Items marked `[Gap]` represent missing traceability — requirements that exist in spec.md with no corresponding task in tasks.md
- The T010/T013/T015 conflicts (CHK046–CHK049) are the highest-risk items: if implemented as written in tasks.md rather than as designed in plan.md, the workflow will be deployed broken
