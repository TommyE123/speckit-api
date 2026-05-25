# Implementation Readiness Checklist: GitHub Actions CI Build Workflow

**Purpose**: Validates cross-document consistency (spec ↔ plan ↔ tasks), task definition quality, and implementation readiness — testing whether the design artifacts are complete and coherent enough for safe, correct implementation. Complements `ci.md` (requirements quality) and `requirements.md` (spec completeness).
**Created**: 2026-05-25
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [tasks.md](../tasks.md)

---

## Cross-Document Consistency: Spec ↔ Tasks

- [ ] CHK001 - Is FR-017 (`dorny/test-reporter` inline PR test reporting) represented in the tasks.md requirements coverage table? The table currently ends at FR-016, leaving FR-017 entirely untraceable to any task. [Gap, Spec §FR-017]
- [ ] CHK002 - Does the workflow YAML embedded in tasks.md T001 include the `Publish Test Results` step (`dorny/test-reporter@v3.0.0`) required by FR-017, or does it reflect an earlier pre-clarification version? [Conflict, Spec §FR-017 vs tasks.md T001]
- [ ] CHK003 - Does the workflow YAML in tasks.md T001 include the `permissions:` block (`contents: read`, `checks: write`, `actions: read`) specified in plan.md as required for `dorny/test-reporter` Check Run creation? [Conflict, plan.md §Workflow Design vs tasks.md T001]
- [ ] CHK004 - Are all 17 functional requirements (FR-001 through FR-017) individually traceable to at least one named task in tasks.md? [Traceability, Spec §FR-001–FR-017]
- [ ] CHK005 - Is the clarification session (2026-05-25) outcome — requiring `dorny/test-reporter@v3.0.0` to be mandatory, gating, and applicable to all PRs including fork-originated — reflected faithfully in tasks.md T001? [Consistency, Spec §Clarifications vs tasks.md]

---

## Cross-Document Consistency: Spec ↔ Plan

- [ ] CHK006 - Does plan.md address the `fail-on-error: true` and `fail-on-empty: true` defaults of `dorny/test-reporter@v3.0.0` as the mechanism by which FR-017's gating requirement is satisfied? Are these defaults explicitly documented as requirements-level assumptions in spec? [Clarity, Spec §FR-017 vs plan.md §Workflow Design]
- [ ] CHK007 - Is the fork PR fallback behaviour (`dorny/test-reporter@v3.0.0` graceful degradation to Job Summary) mentioned in plan.md explicitly required in spec, or is it only an implementation-level detail? [Completeness, Spec §FR-017 vs plan.md §Constraints]
- [ ] CHK008 - Does plan.md reference all 17 FRs from spec, or do any requirements introduced in the 2026-05-25 clarification session lack corresponding plan.md coverage? [Traceability, Spec §Clarifications]
- [ ] CHK009 - Is the plan.md statement that "`TreatWarningsAsErrors=true` already active via `Directory.Build.props` — no CLI override needed" directly traceable to spec FR-008, and is this dependency on an existing project file documented as a named assumption in spec? [Consistency, Spec §FR-008 vs plan.md §Constraints]

---

## Task Definition Quality

- [ ] CHK010 - Is T001 in tasks.md the authoritative YAML source for implementation, or should the YAML in plan.md §Workflow Design be considered canonical? Is this arbitration explicitly documented? [Clarity, tasks.md T001 vs plan.md]
- [ ] CHK011 - Are the acceptance criteria for T002 ("run `dotnet test` and confirm all existing tests pass") specific enough to define what constitutes a passing baseline — e.g., zero failures, zero skipped tests, zero errors? [Clarity, tasks.md T002]
- [ ] CHK012 - Is T003 YAML validation defined with a concrete success indicator — e.g., `actionlint` exit code 0, specific output expected — or only described as "confirm the file is readable"? [Clarity, tasks.md T003]
- [ ] CHK013 - Does T002's description specify which solution file to target (`SpecKitApi.slnx`) for the baseline `dotnet test` run, rather than leaving the target implicit? [Clarity, tasks.md T002]
- [ ] CHK014 - Are the tasks.md Phase 2 verification tasks (T002, T003) sufficient to validate FR-017 compliance, or is a live workflow trigger required as an acceptance step and documented as such? [Coverage, tasks.md §Phase 2]
- [ ] CHK015 - Is FR-011 ("no changes to source code or test files") traceable to a specific verification step in tasks.md that confirms this constraint was not violated during T001? [Traceability, Spec §FR-011]

---

## Post-Test Step Requirements Completeness

- [ ] CHK016 - Are requirements defined for the execution order of the four post-test steps (Upload Test Results → Publish Test Results → Generate Coverage Report → Upload Coverage Report), or is the order treated as an unspecified implementation detail? [Completeness, Spec §FR-015–FR-017]
- [ ] CHK017 - Is the requirement that `Publish Test Results` run "immediately after" the test results artifact upload step (as stated in FR-017) specific enough to be verifiable in a YAML file — i.e., does "immediately after" mean adjacent steps with no intervening steps? [Clarity, Spec §FR-017]
- [ ] CHK018 - Are requirements defined for what artifact name should be used for TRX test results (e.g., `test-results`) and coverage report (e.g., `coverage-report`) to ensure consistent, predictable artifact naming across runs? [Completeness, Spec §FR-014, FR-016]
- [ ] CHK019 - Are artifact retention period requirements specified for `test-results` and `coverage-report` artifacts — e.g., default GitHub retention (90 days) acceptable, or a custom retention policy required? [Completeness, Gap]
- [ ] CHK020 - Are requirements defined for artifact upload collision behavior when two concurrent workflow runs produce artifacts with the same name? [Edge Case, Gap]
- [ ] CHK021 - Is the `reports` glob pattern (`**/coverage.cobertura.xml`) in FR-013 validated against the actual output path of `XPlat Code Coverage` within `./TestResults/` — are they consistent? [Consistency, Spec §FR-012 vs FR-013]

---

## Permissions & Security Requirements

- [ ] CHK022 - Are `GITHUB_TOKEN` minimum required permission scopes (`checks: write`, `actions: read`, `contents: read`) specified as requirements in spec rather than deferred entirely to plan.md as an implementation detail? [Completeness, Spec §FR-017 vs plan.md §Constraints]
- [ ] CHK023 - Are requirements defined for workflow behaviour when an organisation's GitHub Actions permission policy restricts `GITHUB_TOKEN` scopes below `checks: write` — would `dorny/test-reporter` silently degrade or hard-fail? [Edge Case, Gap]
- [ ] CHK024 - Is the security model for fork PRs (read-only `GITHUB_TOKEN`, fallback to Job Summary) documented as a known limitation or explicit design decision in spec, rather than only in plan.md? [Coverage, Spec §FR-017]
- [ ] CHK025 - Are requirements defined for whether action version tags (`@v4`, `@v5`, `@v3.0.0`, `@5.5.10`) are sufficient for supply-chain security, or whether SHA pinning is required? [NFR, Gap]

---

## `dotnet test` & Coverage Collection Requirements

- [ ] CHK026 - Is the `--results-directory ./TestResults` path specified in FR-012 consistent with the glob pattern `TestResults/**/*.trx` in FR-017 and `TestResults/` in FR-016 — are all three using the same relative path? [Consistency, Spec §FR-012, FR-016, FR-017]
- [ ] CHK027 - Are requirements defined for what output `XPlat Code Coverage` must produce (specifically `coverage.cobertura.xml`) and whether the `**/coverage.cobertura.xml` glob in FR-013 will reliably match that output location within `./TestResults/`? [Clarity, Spec §FR-012 vs FR-013]
- [ ] CHK028 - Are requirements defined for `dotnet test` behaviour when the `coverlet.collector` package is not installed in the test project — would the `--collect:"XPlat Code Coverage"` flag silently succeed without producing coverage data? [Edge Case, Gap]
- [ ] CHK029 - Is the `--no-build` flag on `dotnet test` required by spec (to prevent the test step from rebuilding the Release binaries from the Build step), or is it only specified in plan.md as an implementation detail? [Completeness, Spec §FR-006 vs plan.md]

---

## Trigger & Workflow-Level Requirements Completeness

- [ ] CHK030 - Are requirements defined for `concurrency` policy — e.g., should a new push to `main` cancel an in-progress workflow run triggered by the previous push? [Completeness, Gap]
- [ ] CHK031 - Is the `pull_request` event configuration (default sub-types: `opened`, `synchronize`, `reopened`) explicitly documented in requirements, or left to GitHub Actions defaults? [Clarity, Spec §FR-002]
- [ ] CHK032 - Are requirements defined for whether the CI workflow should run on draft pull requests, or is this excluded and documented as an intentional scope decision? [Coverage, Gap]
- [ ] CHK033 - Is the exclusion of feature branch push triggers (only `main` triggers on push per FR-001) documented as an intentional scope boundary rather than an omission? [Scope, Spec §FR-001]

---

## Acceptance Criteria & Success Criterion Quality

- [ ] CHK034 - Are FR-012 through FR-017 (post-test reporting requirements) traceable to at least one user story acceptance scenario, or do they lack a corresponding behavioural scenario in the spec? [Traceability, Spec §User Stories]
- [ ] CHK035 - Is SC-007 ("restore, build, and test steps are individually named") extended to cover the four post-test steps required by FR-013–FR-017, or does it only address the three core build steps? [Completeness, Spec §SC-007]
- [ ] CHK036 - Can SC-006 ("restore step faster due to caching") be objectively verified given that GitHub Actions cache hit/miss is not deterministically observable from workflow step duration alone — is a more precise measurement method defined? [Measurability, Spec §SC-006]
- [ ] CHK037 - Is there a success criterion that specifically validates FR-017 — i.e., that inline test results appear on a pull request check run — rather than relying solely on workflow green/red status? [Coverage, Gap]
- [ ] CHK038 - Are all seven success criteria (SC-001–SC-007) individually traceable to at least one functional requirement, ensuring no criterion is orphaned from the requirement set? [Traceability, Spec §Success Criteria]

---

## Notes

- Check items off as completed: `[x]`
- Add findings or commentary inline below each item
- **Critical gap**: CHK001–CHK005 flag a significant inconsistency between tasks.md (which reflects a pre-FR-017 version) and spec.md/plan.md (which include FR-017 / `dorny/test-reporter`). Resolve before implementing T001.
- Items marked `[Gap]` represent requirements missing from the spec — decide whether to add them or explicitly exclude them
- Items marked `[Conflict]` flag direct contradictions between documents that must be resolved
- Items marked `[Traceability]` flag requirements or criteria that cannot be traced back to another document
