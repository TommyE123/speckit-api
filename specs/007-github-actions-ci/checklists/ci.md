# CI Requirements Checklist: GitHub Actions CI Build Workflow

**Purpose**: Validates the quality, completeness, clarity, and coverage of requirements for the GitHub Actions CI workflow feature — testing whether the *spec is well-written*, not whether the implementation works.
**Created**: 2026-05-24
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md)

---

## Requirement Completeness

- [ ] CHK001 - Are requirements defined for concurrent workflow run behavior (e.g., cancel-in-progress policy when multiple commits push rapidly to `main`)? [Completeness, Gap]
- [ ] CHK002 - Are `GITHUB_TOKEN` permission scopes required by the workflow explicitly specified in the requirements? [Completeness, Gap]
- [ ] CHK003 - Is the solution file or project path scope for each `dotnet` command specified (e.g., targeting `SpecKitApi.slnx`)? [Completeness, Spec §FR-004]
- [ ] CHK004 - Are test result reporting requirements defined beyond pass/fail (e.g., artifact upload, JUnit XML report format)? [Completeness, Gap]
- [ ] CHK005 - Are requirements defined for maximum acceptable workflow timeout to prevent indefinitely stalled runs? [Completeness, Gap]

---

## Requirement Clarity

- [ ] CHK006 - Is "latest stable .NET 10 SDK" in FR-003 clarified with a specific semver wildcard (e.g., `10.0.x`) or explicit version resolution strategy? [Clarity, Spec §FR-003]
- [ ] CHK007 - Is "distinct, named step" in FR-004/FR-005/FR-006 defined with required step name formats or naming conventions? [Clarity, Spec §FR-004]
- [ ] CHK008 - Is FR-008 ("treat build warnings as errors") clarified as relying exclusively on `Directory.Build.props`, or is an explicit CLI override also acceptable? [Clarity, Spec §FR-008]
- [ ] CHK009 - Is the NuGet cache key format defined with specific input files and OS prefix (e.g., `${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}`)? [Clarity, Spec §FR-009]
- [ ] CHK010 - Is the use of `--no-restore` for `dotnet build` and `--no-build` for `dotnet test` defined to prevent redundant operations, or left as an implementation detail? [Clarity, Gap]
- [ ] CHK011 - Is the `Release` configuration applied consistently to both `dotnet build` and `dotnet test`, and is this consistency explicitly required in the spec? [Consistency, Spec §FR-005/FR-006]

---

## Trigger & Scope Coverage

- [ ] CHK012 - Are requirements defined for `pull_request` event sub-types that should trigger the workflow (e.g., `opened`, `synchronize`, `reopened`)? [Coverage, Spec §FR-002]
- [ ] CHK013 - Are requirements defined for draft pull request behavior — should CI run when a PR is in draft state? [Coverage, Gap]
- [ ] CHK014 - Are requirements defined for workflow behavior when triggered by a pull request from a forked repository (read-only `GITHUB_TOKEN`, potential cache limitations)? [Coverage, Gap]
- [ ] CHK015 - Is the intentional exclusion of push triggers on non-`main` branches (e.g., feature branches) explicitly documented as a scope decision? [Scope, Spec §FR-001]

---

## Step Definition Quality

- [ ] CHK016 - Is the required execution order of restore → build → test explicitly stated in the requirements, or is it only implied? [Clarity, Spec §FR-004/FR-005/FR-006]
- [ ] CHK017 - Are requirements defined for `--no-build` flag usage on `dotnet test` to prevent the test step from re-building an already-built solution? [Completeness, Gap]
- [ ] CHK018 - Are requirements defined for what constitutes a "named" step — i.e., is a human-readable `name:` field in the YAML step required or just the `run:` key? [Clarity, Spec §SC-007]

---

## Failure & Error Handling Coverage

- [ ] CHK019 - Are requirements defined for workflow behavior when the .NET 10 SDK version is unavailable on the runner at execution time (FR-003 failure mode)? [Edge Case, Spec §Edge Cases]
- [ ] CHK020 - Are failure propagation requirements defined for a multi-project solution where one project fails to build — must the entire workflow fail, not just the failing project? [Coverage, Spec §Edge Cases]
- [ ] CHK021 - Are requirements defined for distinguishing a test runner runtime error (e.g., missing assembly) from a test assertion failure, and is the expected workflow behavior the same for both? [Coverage, Spec §Edge Cases]
- [ ] CHK022 - Are requirements defined for re-triggering or re-running a failed workflow run — is manual re-run behavior in scope? [Coverage, Gap]

---

## Caching Requirements

- [ ] CHK023 - Is the NuGet cache path (`~/.nuget/packages`) explicitly specified as the cache target location in the requirements? [Clarity, Spec §FR-009]
- [ ] CHK024 - Are cache restore fallback key requirements defined (e.g., partial key match strategy when the exact cache key is absent)? [Coverage, Spec §Edge Cases]
- [ ] CHK025 - Are requirements defined for cache invalidation when `packages.lock.json` is updated or does not exist in the repository? [Edge Case, Spec §Assumptions]
- [ ] CHK026 - Is the `actions/cache` action version (or minimum version) specified in requirements, or is version selection left entirely to implementation? [Clarity, Spec §plan.md Dependencies]

---

## Edge Case Coverage

- [ ] CHK027 - Are requirements defined for a solution with zero test projects — would `dotnet test` fail, succeed silently, or produce a warning? [Edge Case, Gap]
- [ ] CHK028 - Are requirements defined for the workflow when `packages.lock.json` does not exist (the spec assumes it may be used as a cache key source)? [Edge Case, Spec §Assumptions]
- [ ] CHK029 - Are requirements defined for a commit that changes only documentation or non-code files — should the full build-and-test pipeline still run? [Coverage, Gap]

---

## Non-Functional Requirements

- [ ] CHK030 - Are performance targets defined for maximum acceptable end-to-end workflow duration (warm-cache vs. cold-cache)? [NFR, Gap]
- [ ] CHK031 - Are security requirements defined for pinning third-party actions (`actions/checkout`, `actions/setup-dotnet`, `actions/cache`) to specific SHAs rather than mutable version tags? [NFR, Gap]
- [ ] CHK032 - Are maintainability requirements defined for action version update cadence (e.g., Dependabot or manual policy for upgrading `@v4` actions)? [NFR, Gap]

---

## Assumptions & Dependencies

- [ ] CHK033 - Is the assumption that `TreatWarningsAsErrors=true` in `Directory.Build.props` is sufficient to satisfy FR-008 validated and explicitly documented? [Assumption, Spec §Assumptions]
- [ ] CHK034 - Is the assumption that the current codebase is "green" (no pre-existing test failures or warnings) documented as a named prerequisite for safe workflow introduction? [Assumption, Spec §Assumptions]
- [ ] CHK035 - Is the dependency on `ubuntu-latest` runner support for .NET 10 SDK validated against GitHub's official runner support documentation, not just assumed? [Dependency, Spec §Assumptions]
- [ ] CHK036 - Is FR-011 ("no changes to source code or test files") documented with a rationale, and is there a requirement for how this constraint is enforced or verified? [Assumption, Spec §FR-011]

---

## Acceptance Criteria Quality

- [ ] CHK037 - Can SC-003 ("green workflow run 100% of the time" for a passing codebase) be objectively measured given non-deterministic runner availability and infrastructure flakiness? [Measurability, Spec §SC-003]
- [ ] CHK038 - Is SC-006 ("restore step faster due to caching") quantified with a measurable threshold or percentage improvement rather than a relative qualitative comparison? [Measurability, Spec §SC-006]
- [ ] CHK039 - Does SC-001 ("syntactically valid") specify the validation method or tool (e.g., `actionlint`, `yamllint`, GitHub's built-in YAML parser)? [Clarity, Spec §SC-001]
- [ ] CHK040 - Are all 11 functional requirements (FR-001 through FR-011) traceable to at least one acceptance scenario or success criterion? [Traceability]

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

- [ ] CHK041 - Are FR-019, FR-020, FR-021, FR-022, and FR-023 each mapped to at least one task in tasks.md? The requirements coverage table in tasks.md stops at FR-018 — is this an intentional scope boundary or an untracked gap that leaves five clarification-session requirements with no implementation task? [Traceability, Gap, Spec §FR-019–FR-023, tasks.md §Requirements Coverage]
- [ ] CHK042 - Is there a task that explicitly implements the workflow-level `env:` block from FR-019 (`DOTNET_SKIP_FIRST_TIME_EXPERIENCE`, `DOTNET_CLI_TELEMETRY_OPTOUT`, `NUGET_PACKAGES`)? Without it, FR-009's `path: ${{ env.NUGET_PACKAGES }}` references an undefined variable — is this dependency between FR-019 and FR-009 documented? [Completeness, Gap, Spec §FR-019/FR-009]
- [ ] CHK043 - Is there a task that implements the `Write Coverage to Job Summary` step from FR-020, including the exact shell command (`cat coveragereport/SummaryGithub.md >> $GITHUB_STEP_SUMMARY`) and the ordering constraint ("immediately after Generate Coverage Report")? No task in tasks.md describes this step. [Completeness, Gap, Spec §FR-020]
- [ ] CHK044 - Is there a task that implements the sticky PR comment step from FR-022 (`marocchino/sticky-pull-request-comment@2.9.4`, `recreate: true`, event-gated to `pull_request`)? No task in tasks.md describes this step. [Completeness, Gap, Spec §FR-022]
- [ ] CHK045 - Is FR-023 verified by an explicit task that inspects `SpecKitApi.Tests.csproj` for the `coverlet.collector` package reference, rather than solely relying on plan.md's assertion that it is "already present at v10.0.1"? If the package is absent, the entire coverage pipeline silently produces no output. [Completeness, Assumption, Spec §FR-023, plan.md §Summary]

---

## Cross-Artifact Conflicts (Spec vs. Tasks)

> Focus: Places where tasks.md diverges from spec.md or plan.md in ways that would produce a broken or non-compliant workflow if implemented as written.

- [ ] CHK046 - Does tasks.md T010 omit `pull-requests: write` from the permissions block? T010 lists `contents: read, checks: write, actions: read` only, but FR-021 explicitly requires `pull-requests: write` and plan.md's final YAML includes it. If T010 is implemented as written, the sticky PR comment step (FR-022) will fail with a permissions error — is this discrepancy resolved? [Conflict, Spec §FR-021, tasks.md T010, plan.md §Workflow Design]
- [ ] CHK047 - Does tasks.md T013 specify an incomplete `reporttypes` value (`'HtmlInline;Cobertura'`) that omits the `MarkdownSummaryGithub` type required by FR-013? Without `MarkdownSummaryGithub`, `coveragereport/SummaryGithub.md` is never generated, causing both the `Write Coverage to Job Summary` step (FR-020) and the sticky PR comment step (FR-022) to fail silently or with a file-not-found error. Is this truncation intentional or an authoring error in T013? [Conflict, Spec §FR-013, tasks.md T013]
- [ ] CHK048 - Does tasks.md T013 also omit `assemblyfilters: '-*.Tests*'` and `verbosity: 'Warning'` required by FR-013? If so, does T013 as written satisfy FR-013, or does it produce a non-compliant implementation that includes test assembly coverage data and verbose logging? [Conflict, Spec §FR-013, tasks.md T013]
- [ ] CHK049 - Does tasks.md T015 hardcode `path: ~/.nuget/packages` for the NuGet cache, conflicting with FR-009 (`path: ${{ env.NUGET_PACKAGES }}`) and FR-019 (which defines `NUGET_PACKAGES` as `${{ github.workspace }}/.nuget/packages`)? These two paths resolve differently on the runner — if T015 is implemented as written, NuGet packages are cached at a path that no other step reads from. Is this conflict explicitly resolved? [Conflict, Spec §FR-009/FR-019, tasks.md T015]
- [ ] CHK050 - Does plan.md claim "thirteen named steps" while the final YAML block in plan.md contains twelve steps (Checkout through Post Coverage Summary PR Comment)? Does SC-007 ("restore, build, and test steps are individually named") reflect only the original three core steps, leaving the nine post-test steps' naming requirements unaddressed in any success criterion? [Conflict, Spec §SC-007, plan.md §Workflow Design]

---

## Coverage Reporting Pipeline Coherence (FR-012–FR-022)

> Focus: The six-step post-test reporting chain is tightly coupled — failures in early steps cascade silently to later steps. Are these interdependencies fully specified?

- [ ] CHK051 - Is the dependency chain between FR-013 → FR-020 → FR-022 explicitly documented? All three steps consume `coveragereport/SummaryGithub.md`: if FR-013 is misconfigured (e.g., missing `MarkdownSummaryGithub` report type), both FR-020 and FR-022 fail — but only one of them (`if: always()` on FR-020) is guaranteed to surface the failure. Is this cascade failure mode defined in requirements? [Coverage, Spec §FR-013/FR-020/FR-022, Gap]
- [ ] CHK052 - Is the `if: always()` requirement for FR-020 ("Write Coverage to Job Summary") explicitly reconciled with what happens when `coveragereport/SummaryGithub.md` does not exist? The `cat` command exits non-zero on a missing file — is this the intentional gating behaviour per FR-015, or does the spec need to define a guard condition? [Edge Case, Spec §FR-015/FR-020]
- [ ] CHK053 - Is FR-017's requirement that the workflow "fail if publication is unsuccessful" reconciled with plan.md's description of `dorny/test-reporter@v3.0.0`'s fork PR behaviour as "graceful degradation" to Job Summary? For fork PRs, Check Run creation fails (read-only token) and the action falls back silently — does this constitute "unsuccessful publication" under FR-017, and if not, is the exception explicitly documented? [Conflict, Spec §FR-017, plan.md §Constraints]
- [ ] CHK054 - Is FR-022's event-gate condition (`github.event_name == 'pull_request'`, not `if: always()`) explicitly identified as a named exception to FR-015's "failures MUST fail the workflow unless explicitly event-gated" rule? Without an explicit exception callout in FR-015 or FR-022, a reviewer cannot confirm compliance — the phrase "explicitly event-gated" needs a cross-reference. [Clarity, Spec §FR-015/FR-022]
- [ ] CHK055 - Is the file path `coveragereport/SummaryGithub.md` consumed by FR-020 and FR-022 documented as a deterministic output of the `MarkdownSummaryGithub` report type in FR-013? If the filename is silently different across ReportGenerator versions, both downstream steps fail without a clear error. Is this path assumption validated in research.md? [Assumption, Spec §FR-013/FR-020/FR-022, Gap]

---

## Notes (Appended 2026-05-25 — PR Review Focus)

- Items CHK041–CHK055 are scoped to the 2026-05-25 clarification session requirements (FR-019–FR-023) and the post-test coverage reporting pipeline (FR-012–FR-022)
- Primary audience: peer reviewer validating whether the spec and plan are coherent enough to approve merging the feature branch
- Items marked `[Conflict]` represent direct contradictions between two artifacts — these are highest-priority blocks for PR approval
- Items marked `[Gap]` represent missing traceability — requirements that exist in spec.md with no corresponding task in tasks.md
- The T010/T013/T015 conflicts (CHK046–CHK049) are the highest-risk items: if implemented as written in tasks.md rather than as designed in plan.md, the workflow will be deployed broken
