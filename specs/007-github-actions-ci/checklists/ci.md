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
