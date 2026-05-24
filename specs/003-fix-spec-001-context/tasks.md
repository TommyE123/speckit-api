---

description: "Task list for Fix Spec 001 Context Metadata"
---

# Tasks: Fix Spec 001 Context Metadata

**Input**: Design documents from `specs/003-fix-spec-001-context/`

**Prerequisites**: [plan.md](plan.md) ✅ | [spec.md](spec.md) ✅

**Tests**: No test tasks — this feature modifies only a JSON metadata file. Acceptance is verified
by field inspection (plan.md Verification Steps) against the functional requirements.

**Organization**: Both issues reported in the spec have been verified as already resolved prior to
planning (see plan.md §Phase 0). The single implementation task is a structured verification pass
that confirms each functional requirement is met before committing the fix.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on each other)
- **[Story]**: Which user story this task belongs to
- **No [P] marker**: Must complete before the next phase begins

## Scope Summary (from plan.md)

| File | Changes |
|------|---------|
| `specs/001-jsonplaceholder-core/.spec-context.json` | No changes required — both issues pre-resolved |

---

## Phase 1: Setup — Not Applicable

> This feature requires no new projects, source files, or build infrastructure.
> **Proceed directly to Phase 2.**

---

## Phase 2: Foundational — Not Applicable

> No shared infrastructure or base models are introduced.
> **Proceed directly to Phase 3.**

---

## Phase 3: User Story 1 — Correct Step Display in SpecKit Companion (Priority: P1) 🎯 MVP

**Goal**: Confirm that `currentStep` in `specs/001-jsonplaceholder-core/.spec-context.json` is set
to `"implement"`, resolving the persistent spinner caused by the non-standard `"analyze"` value.

**Independent Test**: Open SpecKit Companion — spec 001 shows the same green test tube icon as
spec 002, with no spinner visible.

**Spec coverage**: FR-001 | SC-001

> **Plan finding**: `currentStep` is already `"implement"`. T001 re-verifies this at implementation
> time before any commit is made.

### Implementation for User Story 1

- [ ] T001 [US1] Verify `specs/001-jsonplaceholder-core/.spec-context.json`: read the `currentStep` field and confirm it equals `"implement"`. Run the PowerShell one-liner from plan.md Step 1: `$ctx = Get-Content "specs/001-jsonplaceholder-core/.spec-context.json" | ConvertFrom-Json; $ctx.currentStep`. Expected output: `implement`. No file changes required (FR-001, SC-001).

**Checkpoint**: `currentStep` confirmed as `"implement"`. User Story 1 verification complete — proceed to Phase 4.

---

## Phase 4: User Story 2 — Correct Chronological Timestamps (Priority: P2)

**Goal**: Confirm that `selectedAt` ≤ `specify.startedAt` < `specify.completedAt` in
`specs/001-jsonplaceholder-core/.spec-context.json`, eliminating the "Plan generated before spec"
warning in SpecKit Companion.

**Independent Test**: Inspect the `.spec-context.json` for spec 001 and verify that
`specify.startedAt` is earlier than `specify.completedAt`, and that `selectedAt` is no later than
`specify.startedAt`.

**Spec coverage**: FR-002, FR-003 | SC-002, SC-003

> **Plan finding**: All three timestamp fields are already in chronological order
> (`selectedAt` = `startedAt` = `"2026-05-23T12:35:00.000Z"` < `completedAt` = `"2026-05-23T12:37:24.874Z"`).
> T002 re-verifies this at implementation time.

### Implementation for User Story 2

- [ ] T002 [US2] Verify timestamp chronology in `specs/001-jsonplaceholder-core/.spec-context.json`: run the PowerShell block from plan.md Step 2 that parses `selectedAt`, `stepHistory.specify.startedAt`, and `stepHistory.specify.completedAt` as `DateTimeOffset` values and asserts `selectedAt ≤ startedAt < completedAt`. Expected output: `PASS: timestamps are in chronological order`. No file changes required (FR-002, FR-003, SC-002, SC-003).

**Checkpoint**: Timestamp chronology confirmed. User Story 2 verification complete — proceed to Phase 5.

---

## Phase 5: Integrity Confirmation (Priority: P3)

**Goal**: Confirm that no unintended changes have been made to other fields in
`specs/001-jsonplaceholder-core/.spec-context.json` and that no source or test files have been
modified.

**Independent Test**: `git diff --name-only HEAD -- src/ tests/` produces empty output.

**Spec coverage**: FR-004, FR-005 | SC-004

> **Plan finding**: Full-file review in plan.md §Phase 0 confirmed all other fields intact and no
> source/test file changes. T003 and T004 re-verify at implementation time.

### Implementation for Phase 5

- [ ] T003 [P] [US1,US2] Run `git diff --name-only HEAD -- src/ tests/` and confirm empty output — no source or test files have been modified as a side-effect of this feature (FR-005, SC-004).
- [ ] T004 [P] [US1,US2] Run `git status --short` and confirm no modified files exist under `src/` or `tests/`. Additionally review `specs/001-jsonplaceholder-core/.spec-context.json` for any unexpected field changes beyond `currentStep` and the three timestamp fields (FR-004).

**Checkpoint**: Integrity confirmed. All functional requirements satisfied. Proceed to Phase 6 validation.

---

## Phase 6: Validation — Acceptance Gate

**Purpose**: Final confirmation that all four functional requirements and all four success criteria
are met. This phase depends on T001–T004 all being complete.

**⚠️ CRITICAL**: Do NOT begin Phase 6 until all of T001–T004 are checked off.

**Spec coverage**: FR-001 through FR-005 | SC-001 through SC-004

- [ ] T005 Confirm all verification tasks T001–T004 have passed. Commit the feature branch with a message summarising the two fixes: `currentStep` corrected to `"implement"` and `specify.startedAt` corrected to precede `specify.completedAt`. No new source or test files should appear in the commit.

**Checkpoint**: Feature 003 complete. All success criteria confirmed ✅

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 3 (T001)    ──── no prerequisites ──────► can start immediately
Phase 4 (T002)    ──── no prerequisites ──────► can run in parallel with T001
Phase 5 (T003,T004)─── no prerequisites ──────► can run in parallel with T001 and T002
Phase 6 (T005)    ──── requires T001–T004 ────► all verifications must pass first
```

### Parallel Opportunities

T001, T002, T003, and T004 are all verification-only tasks that read different fields of the same
file or different git commands. They can be executed simultaneously:

```
Task T001: Verify currentStep = "implement"
Task T002: Verify timestamp chronology (selectedAt ≤ startedAt < completedAt)
Task T003: Confirm no src/tests diff
Task T004: Confirm git status clean for src/tests; full-field review
```

Validation task T005 is **sequential** — it depends on T001–T004 all passing.

---

## Implementation Strategy

### Minimum Viable Verification

1. Run T001 (verify `currentStep`)
2. Run T002 (verify timestamps)
3. **STOP and VALIDATE**: Both issues confirmed resolved → Feature 003 complete at MVP level

### Full Delivery

1. Run T001, T002, T003, T004 in parallel
2. Run T005 (commit)
3. All SC-001 through SC-004 confirmed ✅

### Solo Developer Strategy

```
1. $ctx = Get-Content "specs/001-jsonplaceholder-core/.spec-context.json" | ConvertFrom-Json
2. $ctx.currentStep          # T001 — should output: implement
3. Run Step 2 block (T002)   # should output: PASS
4. git diff --name-only HEAD -- src/ tests/  # T003 — should be empty
5. git status --short         # T004 — no src/tests entries
6. Commit (T005)
```

---

## Traceability Matrix

| Task | Plan Section | Spec FR | Spec SC |
|------|--------------|---------|---------|
| T001 | Phase 1 §FR-001 | FR-001 | SC-001 |
| T002 | Phase 1 §FR-002, FR-003 | FR-002, FR-003 | SC-002, SC-003 |
| T003 | Phase 1 §FR-005 | FR-005 | SC-004 |
| T004 | Phase 1 §FR-004 | FR-004 | SC-004 |
| T005 | Verification Steps | FR-001 through FR-005 | SC-001 through SC-004 |

---

## Notes

- All [P] tasks are verification-only; no ordering dependency between them
- No file edits are expected or permitted under any task
- If any verification task fails unexpectedly, stop and investigate before proceeding to T005
- No test code modifications are permitted under any circumstances (FR-005)
- Commit after Phase 6 (T005) to preserve a clean history for this feature
