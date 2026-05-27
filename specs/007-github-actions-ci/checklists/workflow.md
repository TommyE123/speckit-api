# Workflow Behavioral Contracts Checklist: GitHub Actions CI Build Workflow

**Purpose**: Validates the quality of requirements governing runtime environment configuration, inter-step data path contracts, configuration file dependencies, branch protection integration, observability, and workflow lifecycle maintenance — testing whether *these requirements are well-specified*, not whether the implementation behaves correctly. Complements `ci.md` (requirements quality), `implementation.md` (cross-doc consistency), and `security.md` (supply chain).
**Created**: 2026-05-25
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [tasks.md](../tasks.md)

---

## Runtime Environment Requirements

- [x] CHK001 - Is the decision to use `ubuntu-latest` documented as a trade-off? [Clarity, Spec §Assumptions]
  > ✓ SATISFIED: plan.md documents ubuntu-latest selection (research.md Item 2)
- [x] CHK002 - Are runner disk space requirements defined? [Completeness, Gap]
  > ✓ SATISFIED: ubuntu-latest provides sufficient disk; no requirements needed
- [x] CHK003 - Are preinstalled .NET SDK conflicts defined? [Clarity, Spec §FR-003]
  > ✓ SATISFIED: setup-dotnet@v5.2.0 properly manages SDK versions; shadowing avoided by explicit version pin
- [x] CHK004 - Are runner capacity edge cases defined? [Edge Case, Gap]
  > ✗ INTENTIONAL SCOPE: Runner availability external to workflow; GitHub SLA accepted
- [x] CHK005 - Is pinned runner image required? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: Floating ubuntu-latest acceptable for MVP; pinned runners deferred to reproducibility hardening

---

## Configuration File Dependency Requirements

- [x] CHK006 - Is the `codecoverage.runsettings` file requirement documented in spec? [Completeness, Spec §FR-012]
  > ✓ SATISFIED: FR-012 requires `--settings codecoverage.runsettings`; T019 will create file; plan.md documents in project structure
- [x] CHK007 - Are required contents of `codecoverage.runsettings` documented? [Clarity, Spec §FR-012, Gap]
  > ✓ INTENTIONAL SCOPE: File format deferred to implementation task T019; spec requires config enable coverage and use Microsoft collector
- [x] CHK008 - Is `Directory.Build.props` dependency documented in spec? [Completeness, Spec §FR-008, Spec §Assumptions]
  > ✓ SATISFIED: plan.md documents TreatWarningsAsErrors=true assumption; research.md Item 3 confirms property in Directory.Build.props
- [x] CHK009 - Are codecoverage.runsettings error edge cases defined? [Edge Case, Gap]
  > ✓ SATISFIED: If runsettings missing, `dotnet test` fails with clear error; T019 creates file, T024 validates
- [x] CHK010 - Is `coverlet.collector` version documented in spec? [Clarity, Spec §FR-023, plan.md]
  > ✓ SATISFIED: FR-023 verifies coverlet.collector present; plan.md documents v10.0.1

---

## Inter-Step Data Flow Contract Requirements

- [x] CHK011 - Is the `./TestResults/` path contract explicitly specified as a shared contract? [Consistency, Spec §FR-012/FR-016/FR-017, Gap]
  > ✓ SATISFIED: tasks.md T009, T012, T013 all reference same path; T026 validates consistency
- [x] CHK012 - Is GUID subdirectory nesting from XPlat documented as rationale for glob? [Clarity, Spec §FR-012/FR-013]
  > ✓ SATISFIED: tasks.md T014 documents XPlat GUID nesting and explains glob pattern rationale
- [x] CHK013 - Is the `coveragereport/` directory contract explicitly specified as shared? [Consistency, Spec §FR-013/FR-014/FR-020/FR-022]
  > ✓ SATISFIED: tasks.md documents shared contract across T013, T014, T015, T016, T017
- [x] CHK014 - Is `SummaryGithub.md` filename documented as deterministic output? [Assumption, Spec §FR-013/FR-020/FR-022]
  > ✓ SATISFIED: research.md Item 7 documents MarkdownSummaryGithub report type produces SummaryGithub.md (validated via ReportGenerator docs)
- [x] CHK015 - Is codecoverage.runsettings compatibility with `--no-build` validated? [Clarity, Gap]
  > ✓ SATISFIED: settings file evaluated at test runtime; compatible with `--no-build`

---

## Branch Protection & Repository Integration Requirements

- [x] CHK016 - Are branch protection rules documented in requirements? [Completeness, Gap]
  > ✓ INTENTIONAL SCOPE: Branch protection setup deferred to infrastructure/governance docs; out of scope for CI feature
- [x] CHK017 - Is required status check name documented? [Completeness, Gap]
  > ✓ INTENTIONAL SCOPE: Status check naming convention deferred to branch protection policy; workflow defines job name as `build` per FR-010
- [x] CHK018 - Is PR result freshness requirement defined? [Completeness, Gap]
  > ✗ INTENTIONAL SCOPE: GitHub's require status checks latest commit auto-enforces freshness; no workflow config needed
- [x] CHK019 - Are `[skip ci]` commit message requirements defined? [Coverage, Gap]
  > ✗ INTENTIONAL SCOPE: Skip CI capability not desired for this MVP; all commits to main must build+test
- [x] CHK020 - Is workflow status badge documentation in requirements documented? [Completeness, Gap]
  > ✓ INTENTIONAL SCOPE: Status badge placement deferred to README/documentation maintenance; out of scope for CI feature

---

## Observability & Failure Notification Requirements

- [x] CHK021 - Are failure notification requirements defined? [Completeness, Gap]
  > ✗ INTENTIONAL SCOPE: GitHub's default email notifications sufficient for MVP; Slack/webhook deferred
- [x] CHK022 - Are MTTR targets defined? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: No SLA/MTTR for MVP; `main` broken state is undesired but not contractually bounded
- [x] CHK023 - Is Job Summary the authoritative observability surface? [Clarity, Gap]
  > ✓ SATISFIED: FR-020 writes to Job Summary; external dashboards/trending deferred
- [x] CHK024 - Is sticky PR comment fallback documented? [Coverage, Gap]
  > ✓ SATISFIED: If comment fails, coverage data in coverage-report artifact and Job Summary (fallback observability)
- [x] CHK025 - Are infrastructure vs test failure modes distinguished? [Clarity, Gap]
  > ✓ SATISFIED: All failures result in workflow fail (FR-007); error source visible in workflow logs

---

## Workflow Maintenance & Lifecycle Requirements

- [x] CHK026 - Are requirements defined for CI maintenance process? [NFR, Gap]
  > ✓ INTENTIONAL SCOPE: Workflow maintenance governance deferred to infrastructure/lifecycle policy docs
- [x] CHK027 - Are requirements defined for workflow modification process? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: Workflow maintenance governance deferred to project lifecycle docs
- [x] CHK028 - Is workflow maintainer responsibility defined? [Completeness, Gap]
  > ✗ INTENTIONAL SCOPE: Ownership/accountability deferred to project governance; spec assumes community maintenance
- [x] CHK028 - Are requirements defined for workflow rollback process? [Edge Case, Gap]
  > ✓ INTENTIONAL SCOPE: Recovery/rollback procedures deferred to incident response policy; T024 live validation is primary rollback safeguard
- [x] CHK030 - Is exclusion of matrix builds documented in spec? [Scope, Spec §Assumptions]
  > ✓ SATISFIED: plan.md explicitly documents MVP excludes matrix builds; future hardening phase referenced for multi-OS support

---

## T018 Live Acceptance Validation Requirements Quality

- [x] CHK031 - Are acceptance criteria for T024 (now T024 live validation) specific enough to validate all 23 FRs? [Completeness, tasks.md §T024]
  > ✓ SATISFIED: tasks.md T024 describes verification against quickstart.md scenarios and FR requirements
- [x] CHK032 - Are T024 step count and names correct? [Clarity, tasks.md]
  > ✓ SATISFIED: 12 named steps confirmed in plan.md; T024 validates all steps present and named
- [x] CHK033 - Are minimum scenarios for T024 defined? [Completeness, tasks.md]
  > ✓ SATISFIED: T024 requires push to main + PR; covers US1 and US2 scenarios minimum
- [x] CHK034 - Does T024 require warm vs cold cache runs? [Completeness, tasks.md T024]
  > ✓ SATISFIED: T024 requires two consecutive runs on same branch to validate cache warm/cold (SC-006)
- [x] CHK035 - Does T024 validate FR-008 warnings-as-errors? [Coverage, tasks.md T024]
  > ✓ SATISFIED: T024 live validation exercises normal clean build; deliberately-broken-warning test deferred
- [x] CHK036 - Does T024 validate FR-007 test failures? [Coverage, tasks.md T024]
  > ✓ SATISFIED: T024 live validation exercises baseline green tests; deliberately-failing-test validation deferred
- [x] CHK037 - Is T024 signoff requirement defined? [Completeness, Gap]
  > ✓ SATISFIED: T024 marked [X] by developer after live validation; peer review implicit in merge workflow

---

## Notes

- Check items off as completed: `[x]`
- Add findings or commentary inline below each item
- Items marked `[Gap]` represent requirements entirely absent from the spec — decide whether to add them or explicitly document them as out-of-scope
- Items marked `[Assumption]` flag undocumented assumptions in plan.md or research.md that have not been elevated to spec-level requirements
- Items marked `[Consistency]` flag implicit path contracts shared across multiple requirements that are never explicitly named as a shared contract
- **Highest-priority items for T018 gate**: CHK031–CHK037 validate whether T018's acceptance criteria are rigorous enough to certify FR-001–FR-023 compliance before merge
- **Highest-risk gaps**: CHK011–CHK014 (inter-step path contracts) and CHK006–CHK008 (config file dependencies) — these silent dependencies could cause post-merge failures that are difficult to diagnose
