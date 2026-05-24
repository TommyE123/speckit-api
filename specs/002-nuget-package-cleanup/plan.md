# Implementation Plan: NuGet Package Cleanup

**Branch**: `002-nuget-package-cleanup` | **Date**: 2026-05-24 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-nuget-package-cleanup/spec.md`

---

## Summary

Audit and clean up all direct NuGet package references across the two-project solution (`SpecKitApi` and `SpecKitApi.Tests`). The main deliverable is replacing the deprecated `xunit` 2.9.3 package with `xunit.v3` 3.2.2 (aligning the entire xUnit stack on v3), updating two out-of-date test infrastructure packages (`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`), and confirming that all remaining packages are at their latest stable versions with no unused, duplicate, or conflicting references.

---

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies** (direct references under review):

| Project | Package | Current | Target | Action |
|---------|---------|---------|--------|--------|
| Tests | `xunit` | 2.9.3 | — | **Remove** (deprecated; replaced by `xunit.v3`) |
| Tests | `xunit.v3` | — | 3.2.2 | **Add** (official replacement for deprecated `xunit`) |
| Tests | `xunit.runner.visualstudio` | 3.1.4 | 3.1.5 | **Update** |
| Tests | `Microsoft.NET.Test.Sdk` | 17.14.1 | 18.5.1 | **Update** |
| Tests | `Moq` | 4.20.72 | 4.20.72 | No change (latest stable) |
| Tests | `coverlet.collector` | 10.0.1 | 10.0.1 | No change (latest stable) |
| Main | `Microsoft.Extensions.Http` | 10.0.8 | 10.0.8 | No change (latest stable) |
| Main | `Microsoft.Extensions.Http.Resilience` | 10.6.0 | 10.6.0 | No change (latest stable) |

**Storage**: N/A

**Testing**: xUnit v3 + Moq. After cleanup the test stack is: `xunit.v3` 3.2.2 + `xunit.runner.visualstudio` 3.1.5 + `Microsoft.NET.Test.Sdk` 18.5.1.

**Target Platform**: .NET 10 / net10.0

**Project Type**: Dependency cleanup — no new projects, no new production code.

**Constraints**:
- No test code modifications permitted (FR-009, spec assumption 6).
- No new packages beyond `xunit.v3` (which is the NuGet-endorsed replacement for the deprecated `xunit` package — treated as a rename, not new functionality).
- System.Text.Json: no explicit package reference exists in either `.csproj`; no action required per FR-005.

---

## Research Findings

### Package Audit Results

**`dotnet list package --deprecated`** output (pre-cleanup):
```
Project `SpecKitApi.Tests`:
  > xunit  2.9.3  Reason: Legacy  Alternative: xunit.v3 >= 0.0.0
```

**`dotnet list package --outdated`** output (pre-cleanup):
```
Project `SpecKitApi.Tests`:
  > Microsoft.NET.Test.Sdk     17.14.1 → 18.5.1
  > xunit.runner.visualstudio  3.1.4   → 3.1.5
```

**`dotnet restore` and `dotnet build`** pre-cleanup: succeed with zero warnings. The xunit v2/v3 runner mismatch does not produce build-time warnings in the current environment, but the spec requires a consistent version family (FR-002) and the deprecated `xunit` package must be addressed.

### Version Family Decision: xUnit v3

The `xunit.runner.visualstudio` package is already at v3 (3.1.4). The official NuGet deprecation notice for `xunit` points to `xunit.v3` as the replacement. Migrating to `xunit.v3` aligns all xUnit packages on v3 and resolves the FR-002 consistency requirement and the deprecation in a single change.

Migrating to `xunit.v3` does NOT require test code modifications: all five test files use only `[Fact]`, standard `Assert.*` methods, and the `Xunit` namespace — all of which are fully compatible with xunit.v3.

### FR-004: Microsoft.Extensions.Http vs. Microsoft.Extensions.Http.Resilience

`Microsoft.Extensions.Http` (10.0.8) and `Microsoft.Extensions.Http.Resilience` (10.6.0) carry different version numbers because they ship on independent release cadences. Both are the latest stable releases for their respective packages. `dotnet restore` confirms zero dependency conflict or version mismatch warnings. FR-004 is satisfied: these are aligned, compatible stable versions for .NET 10.

### FR-005: System.Text.Json

No explicit `<PackageReference>` for `System.Text.Json` exists in either `.csproj`. The package is consumed from the .NET 10 framework component. No action required.

### FR-006/FR-007: Duplicates and Unused References

No duplicate package references exist within any single `.csproj`. All packages are demonstrably used: xunit/Moq by test code, coverlet by test runner tooling, Microsoft.NET.Test.Sdk by the test host, and the HTTP packages by `JsonPlaceholderClient` in the main project. No removals required beyond the deprecated `xunit`.

---

## Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. API-First Design | ✅ PASS (N/A) | No API contracts are added or modified. Dependency cleanup only. |
| II. Spec-Driven Development | ✅ PASS | `specs/002-nuget-package-cleanup/spec.md` is complete. Workflow: `speckit.specify` → `speckit.plan`. |
| III. Test-First (NON-NEGOTIABLE) | ✅ PASS | No new business logic is introduced. The existing test suite (written first in feature 001) is the validation mechanism for this cleanup. Red-Green-Refactor: existing tests will confirm the updated packages work correctly. |
| IV. Observability & Structured Logging | ✅ PASS (N/A) | No endpoints are added or modified. Silent failures remain prohibited — no changes to error handling. |
| V. Simplicity & YAGNI | ✅ PASS | Only the minimum changes required by the spec are made: one package replaced, two updated, rest unchanged. No new abstractions. |

**Post-Design Re-check**: All five gates remain PASS. The `xunit.v3` introduction (replacing the deprecated `xunit`) is the endorsed upgrade path by the xUnit team and does not add new functionality.

---

## Project Structure

### Documentation (this feature)

```text
specs/002-nuget-package-cleanup/
├── plan.md              ← This file
├── spec.md              ← Feature specification
└── tasks.md             ← Phase 2 output (generated by /speckit.tasks — NOT this command)
```

> No `research.md`, `data-model.md`, or `contracts/` — this is a dependency-only cleanup with no new data models, no API surface changes, and no new source files.

### Source Code (files modified)

```text
tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj   ← 3 changes (see tasks below)
src/SpecKitApi/SpecKitApi.csproj                 ← No changes required
```

---

## Implementation Tasks

> **Ordering**: Tasks T-001 through T-003 are independent `.csproj` edits and can be applied in any order. T-004 validates the combined result.

### T-001 — Replace deprecated `xunit` with `xunit.v3` in test project

**File**: `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`

**Why**: `xunit` 2.9.3 is marked Legacy on NuGet.org with `xunit.v3` as the recommended replacement (FR-002, FR-003-equivalent for xunit). The `xunit.runner.visualstudio` is already at v3, so this completes the v3 family alignment.

**Steps**:
1. Remove `<PackageReference Include="xunit" Version="2.9.3" />`
2. Add `<PackageReference Include="xunit.v3" Version="3.2.2" />`

**Verification**: `dotnet build` succeeds; `dotnet test` runs all 14 existing tests without modification to test code.

**Note**: The `<Using Include="Xunit" />` global import remains unchanged — the `Xunit` namespace is present in `xunit.v3`.

---

### T-002 — Update `xunit.runner.visualstudio` to 3.1.5

**File**: `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`

**Why**: 3.1.4 → 3.1.5 is available (minor/patch update). Brings the runner up-to-date alongside `xunit.v3`.

**Steps**:
1. Change `<PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />` → `Version="3.1.5"`

---

### T-003 — Update `Microsoft.NET.Test.Sdk` to 18.5.1

**File**: `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`

**Why**: 17.14.1 → 18.5.1 is available (latest stable). Keeps the test host current.

**Steps**:
1. Change `<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />` → `Version="18.5.1"`

---

### T-004 — Build and test validation

**Files**: Solution-level validation (no file changes)

**Steps**:
1. `dotnet restore` — confirm zero warnings
2. `dotnet build` — confirm zero warnings and zero errors
3. `dotnet test` — confirm all existing tests pass with no test code changes

**Pass criteria** (maps directly to spec Success Criteria):
- SC-001: Build produces zero package-related warnings or errors
- SC-002: All existing unit tests pass
- SC-003: Every package reference is at latest stable, non-deprecated version for .NET 10
- SC-004: No duplicate or unused references remain
- SC-005: `dotnet restore` completes with zero dependency conflict warnings

---

## Complexity Tracking

*No constitution violations introduced. No complexity justification required.*
