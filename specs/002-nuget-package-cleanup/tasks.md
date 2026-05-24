---

description: "Task list for NuGet Package Cleanup"
---

# Tasks: NuGet Package Cleanup

**Input**: Design documents from `specs/002-nuget-package-cleanup/`

**Prerequisites**: [plan.md](plan.md) ✅ | [spec.md](spec.md) ✅

**Tests**: No new test tasks — the existing test suite (`dotnet test`) is the validation mechanism for this
cleanup (Principle III satisfied: tests were written first in feature 001 and act as the Red-Green gate here).
No test code modifications are permitted per FR-009 and plan assumption 6.

**Organization**: Tasks are grouped by user story. US1 contains the only file edits (T001–T003).
US2 and US3 are audit-confirmation tasks (plan research established no changes are required; these tasks
re-verify that finding at implementation time before validation runs).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on each other)
- **[Story]**: Which user story this task belongs to
- **No [P] marker**: Must complete before the next phase begins

## Scope Summary (from plan.md)

| File | Changes |
|------|---------|
| `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` | 3 edits (T001, T002, T003) |
| `src/SpecKitApi/SpecKitApi.csproj` | No changes required |

---

## Phase 1: Setup — Not Applicable

> This feature is a dependency-only cleanup with no new projects, source files, or build infrastructure.
> The existing solution structure (`SpecKitApi.slnx`, two `.csproj` files) is already the target.
> **Proceed directly to Phase 2.**

---

## Phase 2: Foundational — Not Applicable

> No shared infrastructure, migrations, or base models are introduced. All edits are isolated to a single
> `.csproj` file. **Proceed directly to Phase 3.**

---

## Phase 3: User Story 1 — Consistent and Up-to-Date Test Dependencies (Priority: P1) 🎯 MVP

**Goal**: Replace the deprecated `xunit` 2.x package with `xunit.v3` and bring the two out-of-date test
infrastructure packages (`xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`) to their latest stable
versions — establishing a fully consistent xUnit v3 stack with no deprecation warnings.

**Independent Test**: `dotnet build` succeeds; `dotnet test` runs all 14 existing tests without any
modification to test code (SC-001, SC-002).

**Spec coverage**: FR-001, FR-002, FR-009 | SC-001, SC-002, SC-003

**Why T001–T003 are all [P]**: Each edit touches a different `<PackageReference>` line in the same file
and can be applied in any order by a single implementer or in parallel by multiple implementers without
conflict.

### Implementation for User Story 1

- [ ] T001 [P] [US1] In `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`: remove `<PackageReference Include="xunit" Version="2.9.3" />` and add `<PackageReference Include="xunit.v3" Version="3.2.2" />` — satisfies FR-002 (consistent xUnit v3 family) and resolves the NuGet Legacy deprecation notice (FR-001). The `<Using Include="Xunit" />` global import is unchanged; the `Xunit` namespace is present in xunit.v3.
- [ ] T002 [P] [US1] In `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`: update `<PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />` → `Version="3.1.5"` — brings the Visual Studio runner to its latest stable patch release, aligned with xunit.v3 3.2.2 (FR-001, FR-002).
- [ ] T003 [P] [US1] In `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`: update `<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />` → `Version="18.5.1"` — updates the test host to the latest stable major release compatible with .NET 10 (FR-001).

**Checkpoint**: All three .csproj edits applied. User Story 1 implementation is complete — proceed to
Phase 4 audit before running validation (Phase 6).

---

## Phase 4: User Story 2 — Aligned Resilience and HTTP Package Versions (Priority: P2)

**Goal**: Confirm that `Microsoft.Extensions.Http` and `Microsoft.Extensions.Http.Resilience` are on
mutually compatible, aligned stable versions with no NuGet conflict warnings.

**Independent Test**: `dotnet restore` reports zero dependency-conflict or version-mismatch warnings
(SC-005).

**Spec coverage**: FR-004, FR-008 | SC-003, SC-005

> **Research finding (plan.md §FR-004)**: `Microsoft.Extensions.Http` (10.0.8) and
> `Microsoft.Extensions.Http.Resilience` (10.6.0) ship on independent release cadences; both are the
> latest stable releases for .NET 10. `dotnet restore` produces zero warnings. No file edits are required.
> T004 re-verifies this finding at implementation time.

### Implementation for User Story 2

- [ ] T004 [US2] Audit `src/SpecKitApi/SpecKitApi.csproj`: confirm `Microsoft.Extensions.Http` is at 10.0.8 and `Microsoft.Extensions.Http.Resilience` is at 10.6.0; verify no newer stable .NET 10-compatible release exists for either package on NuGet.org; confirm no version-conflict warnings appear in `dotnet restore` output — no file changes expected per plan.md research (FR-004, FR-008).

**Checkpoint**: US2 audit complete. Both HTTP packages confirmed aligned. Proceed to Phase 5.

---

## Phase 5: User Story 3 — No Unused or Duplicate Package References (Priority: P3)

**Goal**: Confirm that every `<PackageReference>` in every `.csproj` is actively used and appears exactly
once, with no redundant references across the solution.

**Independent Test**: Manual review of both `.csproj` files post-cleanup shows every reference
demonstrably used; `dotnet build` emits zero unused-reference warnings (SC-004).

**Spec coverage**: FR-006, FR-007, FR-008 | SC-004

> **Research finding (plan.md §FR-006/FR-007)**: No duplicates and no unused references were found in
> the pre-cleanup audit. All packages are demonstrably used (xunit.v3/Moq by test code, coverlet by
> runner tooling, Microsoft.NET.Test.Sdk by the test host, HTTP packages by `JsonPlaceholderClient`).
> No removals are required beyond the deprecated `xunit` already handled in T001. T005 re-verifies
> this finding after T001–T003 have been applied.

### Implementation for User Story 3

- [ ] T005 [US3] Audit both `.csproj` files after T001–T003 are applied: review `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` and `src/SpecKitApi/SpecKitApi.csproj` line-by-line; confirm no package reference appears more than once within the same file; confirm every remaining reference is actively used by source code in that project — no file changes expected per plan.md research (FR-006, FR-007).

**Checkpoint**: US3 audit complete. All `.csproj` files confirmed clean. Proceed to Phase 6 validation.

---

## Phase 6: Validation — Build and Test Confirmation

**Purpose**: Run the full validation pipeline to confirm all success criteria are met. This phase depends
on T001–T005 all being complete.

**⚠️ CRITICAL**: Do NOT begin Phase 6 until all of T001–T005 are checked off.

**Spec coverage**: SC-001 through SC-005 (all success criteria)

- [ ] T006 Run `dotnet restore` at solution root and confirm the command exits with code 0 and **zero** NuGet dependency-conflict or version-mismatch warnings in the output — satisfies SC-005 (FR-005, FR-008). Expected: clean restore with updated package graph reflecting xunit.v3 3.2.2, xunit.runner.visualstudio 3.1.5, Microsoft.NET.Test.Sdk 18.5.1.
- [ ] T007 Run `dotnet build` at solution root and confirm the command exits with code 0 with **zero** warnings and **zero** errors — satisfies SC-001 (FR-001 through FR-009). Expected: both `SpecKitApi` and `SpecKitApi.Tests` build successfully; no package deprecation, version-conflict, or unused-reference warnings.
- [ ] T008 Run `dotnet test` at solution root and confirm **all existing tests pass** with no modifications to any test source file — satisfies SC-002 (FR-009). Expected: all 14 tests pass; the `Xunit` namespace resolves correctly from `xunit.v3` 3.2.2; `[Fact]` and `Assert.*` behave identically to the removed `xunit` 2.9.3 package.

**Checkpoint**: Validation complete. All success criteria satisfied. Feature 002 is done.

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 3 (T001–T003)  ──── no prerequisites ────► can start immediately
Phase 4 (T004)       ──── no prerequisites ────► can start immediately (audit only)
Phase 5 (T005)       ──── requires T001–T003 ──► wait for Phase 3 to confirm [P] .csproj edits
Phase 6 (T006–T008)  ──── requires T001–T005 ──► all audits + edits must be done first
```

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies — start immediately
- **User Story 2 (P2)**: No dependencies — can audit in parallel with US1
- **User Story 3 (P3)**: Depends on T001–T003 being applied (auditing the post-edit .csproj state)
- **Validation (Phase 6)**: Depends on all user stories complete (T001–T005)

### Within User Story 1

- T001, T002, T003 are fully independent — apply in any order or in parallel

### Parallel Opportunities

All three US1 edits can be applied simultaneously:

```
# Apply all three .csproj changes in parallel (all target the same file, non-overlapping lines):
Task T001: Remove xunit 2.9.3, add xunit.v3 3.2.2
Task T002: Update xunit.runner.visualstudio 3.1.4 → 3.1.5
Task T003: Update Microsoft.NET.Test.Sdk 17.14.1 → 18.5.1

# US2 audit can also run in parallel with the above:
Task T004: Verify HTTP package alignment in src/SpecKitApi/SpecKitApi.csproj
```

Validation tasks T006 → T007 → T008 are **sequential** (each depends on the prior step).

---

## Implementation Strategy

### MVP (User Story 1 Only — Minimum to Resolve Deprecation)

1. Apply T001, T002, T003 (three parallel .csproj edits)
2. Skip T004/T005 audits (pre-verified by plan.md research — acceptable for MVP)
3. Run T006, T007, T008 (validate)
4. **STOP and VALIDATE**: Deprecated package gone, all tests pass → Feature 002 complete at MVP level

### Full Delivery (All User Stories)

1. Apply T001, T002, T003 in parallel
2. Run T004, T005 audits (confirm no further changes needed)
3. Run T006 → T007 → T008 sequentially
4. All SC-001 through SC-005 confirmed ✅

### Solo Developer Strategy

```
1. Open tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj
2. Apply T001 (swap xunit → xunit.v3)
3. Apply T002 (bump runner version)
4. Apply T003 (bump SDK version)
5. Review src/SpecKitApi/SpecKitApi.csproj (T004 + T005 combined audit)
6. dotnet restore  (T006)
7. dotnet build    (T007)
8. dotnet test     (T008)
```

---

## Traceability Matrix

| Task | Plan Task | Spec FR | Spec SC |
|------|-----------|---------|---------|
| T001 | T-001 | FR-001, FR-002, FR-009 | SC-001, SC-002, SC-003 |
| T002 | T-002 | FR-001, FR-002, FR-009 | SC-001, SC-002, SC-003 |
| T003 | T-003 | FR-001, FR-009 | SC-001, SC-002, SC-003 |
| T004 | — (audit) | FR-004, FR-008 | SC-003, SC-005 |
| T005 | — (audit) | FR-006, FR-007, FR-008 | SC-004 |
| T006 | T-004 step 1 | FR-005, FR-008 | SC-005 |
| T007 | T-004 step 2 | FR-001 through FR-009 | SC-001 |
| T008 | T-004 step 3 | FR-009 | SC-002 |

---

## Notes

- [P] tasks = independent edits, no ordering dependency between them
- [Story] label maps each task to its user story for traceability
- T004 and T005 are audit/verify tasks — expected outcome is "no changes needed"
- If T004 or T005 discover an unexpected issue (e.g., a newer stable package version exists), raise it
  before proceeding to Phase 6 and update the plan accordingly
- No test code modifications are permitted under any circumstances (FR-009)
- Commit after Phase 3 (T001–T003) and again after Phase 6 passes to preserve a clean history
