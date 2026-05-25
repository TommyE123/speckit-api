# Implementation Readiness Checklist: GitHub Actions CI Build Workflow

**Purpose**: Validates cross-document consistency (spec ↔ plan ↔ tasks), task definition quality, and implementation readiness — testing whether the design artifacts are complete and coherent enough for safe, correct implementation. Complements `ci.md` (requirements quality) and `requirements.md` (spec completeness).
**Created**: 2026-05-25
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [tasks.md](../tasks.md)

---

## Cross-Document Consistency: Spec ↔ Tasks

- [x] CHK001 - Is FR-017 traceable to the tasks.md requirements coverage table? [Traceability, Spec §FR-017]
  > ✓ SATISFIED: tasks.md regenerated 2026-05-25; requirements coverage table now includes FR-017 (maps to T011, T013)
- [x] CHK002 - Does tasks.md T001 include Publish Test Results step? [Conflict, Spec §FR-017, tasks.md T001]
  > ✓ SATISFIED: T013 (was missing from old version) now implements dorny/test-reporter per FR-017
- [x] CHK003 - Does tasks.md include permissions block? [Conflict, plan.md, tasks.md T011]
  > ✓ SATISFIED: T011 implements permissions block per plan.md/FR-021
- [x] CHK004 - Are all 23 functional requirements (FR-001 through FR-023) individually traceable to at least one named task in tasks.md? [Traceability, Spec §FR-001–FR-023]
  > ✓ SATISFIED: tasks.md regenerated 2026-05-25 includes all 23 FRs in §Requirements Coverage table
- [x] CHK005 - Is clarification session outcome reflected in tasks.md? [Consistency, Spec §Clarifications, tasks.md]
  > ✓ SATISFIED: tasks.md regenerated 2026-05-25 after clarifications; all 23 FRs included

---

## Cross-Document Consistency: Spec ↔ Plan

- [x] CHK006 - Does plan.md address `fail-on-error` and `fail-on-empty` defaults? [Clarity, Spec §FR-017, plan.md]
  > ✓ SATISFIED: plan.md §Constraints documents defaults; FR-017 requires gating on test publication
- [x] CHK007 - Is fork PR fallback behavior documented? [Completeness, Spec §FR-017, plan.md]
  > ✓ SATISFIED: plan.md documents graceful fallback to Job Summary for fork PRs
- [x] CHK008 - Does plan.md reference all 23 FRs from spec? [Traceability, Spec §Clarifications]
  > ✓ SATISFIED: plan.md covers all phases and FRs (as updated 2026-05-25)
- [x] CHK009 - Is the plan.md statement about `TreatWarningsAsErrors=true` documented as a named assumption? [Consistency, Spec §FR-008, plan.md §Constraints]
  > ✓ SATISFIED: plan.md explicitly documents this assumption in research.md Item 3

---

## Task Definition Quality

- [x] CHK010 - Is T001 the authoritative YAML source? [Clarity, tasks.md, plan.md]
  > ✓ SATISFIED: plan.md design section documents final workflow; tasks.md references plan.md
- [x] CHK011 - Are acceptance criteria for T023 (formerly T002) specific? [Clarity, tasks.md T023]
  > ✓ SATISFIED: T023 specifies "zero failures" baseline for green test run
- [x] CHK012 - Is T022 validation defined with concrete success indicator? [Clarity, tasks.md T022]
  > ✓ SATISFIED: T022 specifies actionlint or GitHub YAML validation
- [x] CHK013 - Does T002 (now T023) specify target solution file? [Clarity, tasks.md T023]
  > ✓ SATISFIED: tasks.md refers to `dotnet test` from repository root which auto-discovers SpecKitApi.slnx
- [x] CHK014 - Are Phase 2 verification tasks sufficient for FR-017 validation? [Coverage, tasks.md]
  > ✓ SATISFIED: T022-T024 (Polish/Validation phases) include live workflow trigger for T024 PR validation
- [x] CHK015 - Is FR-011 constraint verified in tasks? [Traceability, Spec §FR-011, tasks.md]
  > ✓ SATISFIED: tasks.md constraint "YAML + runsettings only" enforces FR-011; T024 review validates

---

## Post-Test Step Requirements Completeness

- [x] CHK016 - Are post-test step execution orders defined? [Completeness, Spec §FR-013–FR-017, tasks.md]
  > ✓ SATISFIED: tasks.md Phase 4 documents sequential step order (T012–T017)
- [x] CHK017 - Is "immediately after" requirement specific? [Clarity, Spec §FR-017, tasks.md T013]
  > ✓ SATISFIED: tasks.md T013 "immediately after" T012 means adjacent YAML steps
- [x] CHK018 - Are artifact names documented? [Completeness, Spec §FR-014, FR-016]
  > ✓ SATISFIED: T012 uses `test-results`, T016 uses `coverage-report` (tasks.md)
- [x] CHK019 - Are artifact retention requirements defined? [Completeness, Gap]
  > ✓ SATISFIED: T016 specifies `retention-days: 14` for coverage; test-results uses default 90 days
- [x] CHK020 - Are artifact collision requirements defined? [Edge Case, Gap]
  > ✗ INTENTIONAL SCOPE: Concurrent artifact collisions handled by GitHub (overwrites); deferred to future policy
- [x] CHK021 - Is the `reports` glob pattern consistent with XPlat Code Coverage output? [Consistency, Spec §FR-012, FR-013]
  > ✓ SATISFIED: T014 uses `'**/coverage.cobertura.xml'` glob, justified in plan.md and tasks.md T014; T021 will update for Microsoft Code Coverage

---

## Permissions & Security Requirements

- [x] CHK022 - Are GITHUB_TOKEN scopes specified in spec? [Completeness, Spec §FR-021]
  > ✓ SATISFIED: FR-021 requires job-level permissions; plan.md documents rationale
- [x] CHK023 - Are org permission override edge cases defined? [Edge Case, Gap]
  > ✗ INTENTIONAL SCOPE: Org policy enforcement external to workflow; no workflow-level override needed
- [x] CHK024 - Is fork PR security model documented in spec? [Coverage, Spec §FR-017, plan.md]
  > ✓ SATISFIED: plan.md documents read-only token for fork PRs and Job Summary fallback as design decision
- [x] CHK024 - Are action version pinning security requirements defined? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: SHA pinning deferred to security hardening phase; version tags acceptable for MVP

---

## `dotnet test` & Coverage Collection Requirements

- [x] CHK026 - Is the `--results-directory ./TestResults` path consistent across FR-012, FR-016, FR-017? [Consistency, Spec §FR-012, FR-016, FR-017]
  > ✓ SATISFIED: All three requirements use same path (plan.md, tasks.md)
- [x] CHK027 - Are requirements defined for XPlat Code Coverage output location? [Clarity, Spec §FR-012, FR-013]
  > ✓ SATISFIED: tasks.md T014 documents GUID subdirectory and glob rationale; T021 updates for Microsoft output
- [x] CHK028 - Are requirements defined for missing coverlet.collector? [Edge Case, Gap]
  > ✓ SATISFIED: FR-023 verifies presence; silent fail is acceptable edge case (task ensures package present)
- [x] CHK029 - Is `--no-build` flag required by spec? [Completeness, Spec §FR-006, plan.md]
  > ✓ SATISFIED: plan.md documents `--no-build` rationale; T020 implements

---

## Trigger & Workflow-Level Requirements Completeness

- [x] CHK030 - Are concurrency policy requirements defined? [Completeness, Gap]
  > ✗ INTENTIONAL SCOPE: Concurrency policy deferred to future hardening; MVP allows concurrent runs
- [x] CHK031 - Is `pull_request` event configuration explicitly documented? [Clarity, Spec §FR-002]
  > ✓ SATISFIED: GitHub defaults sufficient; plan.md documents event handling
- [x] CHK032 - Are draft PR requirements defined? [Coverage, Gap]
  > ✓ SATISFIED: Default behavior runs CI on draft PRs; no override needed
- [x] CHK033 - Is feature branch exclusion documented? [Scope, Spec §FR-001]
  > ✓ SATISFIED: FR-001 specifies `branches: [ main ]`; intentional boundary per plan.md

---

## Acceptance Criteria & Success Criterion Quality

- [x] CHK034 - Are FR-012–FR-017 traceable to acceptance scenarios? [Traceability, Spec §User Stories]
  > ✓ SATISFIED: US2 acceptance scenarios (SC1–SC2) exercise all post-test requirements; spec.md §User Stories
- [x] CHK035 - Is SC-007 extended for post-test steps? [Completeness, Spec §SC-007]
  > ✓ SATISFIED: SC-007 applies to all named steps; plan.md lists all 12 step names
- [x] CHK036 - Can SC-006 cache improvement be objectively verified? [Measurability, Spec §SC-006]
  > ✓ SATISFIED: SC-006 measurable by step duration comparison; T024 requires two consecutive runs for cache validation
- [x] CHK037 - Is there an SC for FR-017 validation (inline test results)? [Coverage, Gap]
  > ✗ INTENTIONAL SCOPE: SC-007 (named steps) verifies PR check run exists; Check Run content validation deferred to T024 live test
- [x] CHK038 - Are all success criteria (SC-001–SC-007) individually traceable to at least one functional requirement? [Traceability, Spec §Success Criteria]
  > ✓ SATISFIED: Spec.md documents SC–FR mapping

---

## Notes

- Check items off as completed: `[x]`
- Add findings or commentary inline below each item
- **Critical gap**: CHK001–CHK005 flag a significant inconsistency between tasks.md (which reflects a pre-FR-017 version) and spec.md/plan.md (which include FR-017 / `dorny/test-reporter`). Resolve before implementing T001.
- Items marked `[Gap]` represent requirements missing from the spec — decide whether to add them or explicitly exclude them
- Items marked `[Conflict]` flag direct contradictions between documents that must be resolved
- Items marked `[Traceability]` flag requirements or criteria that cannot be traced back to another document
