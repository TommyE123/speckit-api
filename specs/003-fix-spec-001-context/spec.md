# Feature Specification: Fix Spec 001 Context Metadata

**Feature Branch**: `003-fix-spec-001-context`

**Created**: 2026-05-24

**Status**: Draft

**Input**: User description: "Cleanup spec for 001-jsonplaceholder-core. Known issues to fix: 'currentStep' is set to 'analyze' instead of 'implement' — analyze is not a standard SpecKit step and is causing a spinner in SpecKit Companion. Change to 'implement' to match the correct final step, consistent with 002. Timestamp inconsistency where specify startedAt is later than completedAt, causing a 'Plan generated before spec' warning."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Correct Step Display in SpecKit Companion (Priority: P1)

A developer opening SpecKit Companion sees spec 001 (JSONPlaceholder Core) displaying a green test tube icon that indicates completed implementation — matching the visual state of spec 002. Currently the spinner never resolves because `currentStep` is set to `analyze`, which is not a recognised final step in the SpecKit workflow.

**Why this priority**: The incorrect step value actively breaks the UI, causing a persistent spinner that misleads developers about the state of a fully-completed feature. This is the most disruptive issue.

**Independent Test**: Open SpecKit Companion and observe that spec 001 shows the same green test tube icon as spec 002, with no spinner visible.

**Acceptance Scenarios**:

1. **Given** the `.spec-context.json` for spec 001 has `currentStep` set to `analyze`, **When** the value is corrected to `implement`, **Then** SpecKit Companion displays a green test tube icon for spec 001 with no spinner.
2. **Given** `currentStep` is `implement`, **When** SpecKit Companion loads spec 001, **Then** the displayed state matches spec 002's completed state.

---

### User Story 2 - Correct Chronological Timestamps (Priority: P2)

A developer reviewing spec 001 metadata in SpecKit Companion no longer sees the "Plan generated before spec" warning. The `specify.startedAt` timestamp currently records a time one hour *after* `specify.completedAt`, which is chronologically impossible and triggers the warning.

**Why this priority**: While the warning does not break functionality, it undermines confidence in the spec's history and creates noise in the UI. Fixing it produces a clean, trustworthy audit trail.

**Independent Test**: Inspect the `.spec-context.json` for spec 001 and verify that `specify.startedAt` is earlier than `specify.completedAt`, and that `selectedAt` is no later than `specify.startedAt`.

**Acceptance Scenarios**:

1. **Given** `specify.startedAt` is "2026-05-23T13:35:55Z" and `specify.completedAt` is "2026-05-23T12:37:24.874Z", **When** `specify.startedAt` is corrected to a time before `completedAt`, **Then** timestamps are in logical chronological order.
2. **Given** timestamps are in chronological order, **When** SpecKit Companion loads spec 001, **Then** no "Plan generated before spec" warning is shown.

---

### Edge Cases

- The `transitions` array in `.spec-context.json` is left unchanged — only the top-level `currentStep` and the `stepHistory.specify.startedAt` / `selectedAt` fields are corrected.
- No source code files or test files are modified.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The `.spec-context.json` for `specs/001-jsonplaceholder-core` MUST have `currentStep` set to `"implement"`.
- **FR-002**: The `selectedAt` field in `specs/001-jsonplaceholder-core/.spec-context.json` MUST be earlier than or equal to `stepHistory.specify.startedAt`.
- **FR-003**: The `stepHistory.specify.startedAt` field MUST be earlier than `stepHistory.specify.completedAt` ("2026-05-23T12:37:24.874Z").
- **FR-004**: All other fields in `specs/001-jsonplaceholder-core/.spec-context.json` MUST remain unchanged.
- **FR-005**: No changes MUST be made to any source code or test files.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: SpecKit Companion shows a green test tube icon for spec 001 with no spinner — matching spec 002's visual state.
- **SC-002**: No "Plan generated before spec" warning appears for spec 001 in SpecKit Companion.
- **SC-003**: All timestamps in `stepHistory` are in strict chronological order (each `startedAt` precedes its `completedAt`, and each step's start follows the previous step's completion).
- **SC-004**: Zero source code or test files are modified as part of this change.

## Assumptions

- The correct final step for a completed SpecKit workflow is `"implement"`, consistent with how spec 002 is recorded.
- The `transitions` array accurately reflects the real sequence of events and does not need correction.
- The earliest plausible `specify.startedAt` is approximately two minutes before `specify.completedAt` ("2026-05-23T12:37:24.874Z"), based on typical spec generation duration.
- SpecKit Companion reads `currentStep` directly from `.spec-context.json` to determine the icon and spinner state.
