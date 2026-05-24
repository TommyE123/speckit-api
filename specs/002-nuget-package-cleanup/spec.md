# Feature Specification: NuGet Package Cleanup

**Feature Branch**: `002-nuget-package-cleanup`

**Created**: 2026-05-24

**Status**: Draft

**Input**: User description: "Fix and modernise NuGet packages in the current solution. Review all .csproj files and identify packages that are: incorrect versions, unnecessary/unused, duplicated, conflicting, or deprecated. Replace all problematic packages with the latest stable non-deprecated versions compatible with .NET 10. Packages in scope: xUnit + Moq, Polly, and System.Text.Json. No new functionality — dependency cleanup only."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent and Up-to-Date Test Dependencies (Priority: P1)

A developer working on the solution notices that the xUnit and Moq packages are on mismatched or outdated versions. After this cleanup, all test-related packages use compatible, up-to-date versions so the test suite builds and runs cleanly without warnings or deprecation notices.

**Why this priority**: The test infrastructure is foundational — mismatched xUnit versions (e.g., v2 packages mixed with v3 runner) actively cause build inconsistencies and can mask test failures. Resolving this first ensures a reliable baseline.

**Independent Test**: Can be fully tested by building the test project and running all existing unit tests. Delivers a clean, warning-free test environment.

**Acceptance Scenarios**:

1. **Given** the test project references xUnit 2.x and an xunit.runner.visualstudio 3.x, **When** the cleanup is applied, **Then** all xUnit-related packages are from the same compatible version family (all v2 or all v3)
2. **Given** Moq is at version 4.20.x, **When** the cleanup is applied, **Then** Moq is updated to the latest stable, non-deprecated version compatible with .NET 10
3. **Given** the solution has been cleaned up, **When** `dotnet test` is executed, **Then** all existing tests pass with no changes to test code

---

### User Story 2 - Aligned Resilience and HTTP Package Versions (Priority: P2)

A developer reviewing the main project's dependencies sees that `Microsoft.Extensions.Http` and `Microsoft.Extensions.Http.Resilience` (which depends on Polly internally) are on inconsistent version numbers. After cleanup, both packages are on aligned, compatible versions with no version conflicts in the dependency graph.

**Why this priority**: Version mismatches between tightly coupled packages can cause runtime behaviour differences and NuGet restore warnings. Fixing this ensures a stable, predictable dependency graph.

**Independent Test**: Can be tested by restoring NuGet packages and inspecting the build output for zero version-related warnings. Delivers a coherent dependency graph.

**Acceptance Scenarios**:

1. **Given** `Microsoft.Extensions.Http` is at 10.0.8 and `Microsoft.Extensions.Http.Resilience` is at 10.6.0, **When** the cleanup is applied, **Then** both packages are updated to aligned, compatible stable versions for .NET 10
2. **Given** the cleanup is applied, **When** `dotnet restore` is run, **Then** no NuGet dependency conflict warnings are reported
3. **Given** the cleanup is applied, **When** the main project builds, **Then** zero package-related warnings or errors are produced

---

### User Story 3 - No Unused or Duplicate Package References (Priority: P3)

A developer auditing the solution finds that all .csproj files contain only the package references that are actively used, with no duplicates across projects. The dependency surface is minimal and easy to reason about.

**Why this priority**: Unused or duplicate references increase maintenance overhead and build times. Removing them reduces future confusion about what the project actually depends on.

**Independent Test**: Can be tested by auditing the .csproj files post-cleanup and confirming that no package appears more than once where it shouldn't, and that all remaining packages are demonstrably used.

**Acceptance Scenarios**:

1. **Given** a package reference exists in a .csproj file, **When** the cleanup is applied, **Then** every remaining reference is actively used by code in that project
2. **Given** the cleanup is applied, **When** all .csproj files are reviewed, **Then** no package reference appears in duplicate within the same project
3. **Given** the cleanup is applied, **When** `dotnet build` is run on the full solution, **Then** no warnings about unused or redundant package references are emitted

---

### Edge Cases

- What happens if a package's latest stable version introduces a breaking API change incompatible with .NET 10? The package must remain at the newest version that is both stable and .NET 10 compatible.
- How does the system handle packages that are deprecated with no direct replacement? They should be flagged for removal only if unused; if used, a note should be added documenting the deprecation.
- What if a transitive dependency conflict exists that cannot be resolved without modifying code? This is out of scope — only direct package references in .csproj files are in scope for this cleanup.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: All direct NuGet package references in every .csproj file MUST be updated to their latest stable, non-deprecated versions that are compatible with .NET 10
- **FR-002**: The xUnit test framework packages (xunit, xunit.runner.visualstudio, and any related xUnit packages) MUST use a consistent, mutually compatible version family (either all v2 or all v3 — no mixing)
- **FR-003**: Moq MUST be updated to the latest stable non-deprecated version compatible with .NET 10
- **FR-004**: Microsoft.Extensions.Http and Microsoft.Extensions.Http.Resilience MUST be updated to mutually compatible, aligned stable versions for .NET 10
- **FR-005**: Any explicit System.Text.Json package references MUST be aligned with the version shipped as part of .NET 10 or updated to the latest stable non-deprecated version if a newer release is available
- **FR-006**: All duplicate package references within any single .csproj file MUST be removed
- **FR-007**: Any package reference that is not actively used by code in its host project MUST be removed
- **FR-008**: No conflicting version requirements for the same package across different projects in the solution may remain after cleanup
- **FR-009**: No new packages, features, or functionality MAY be introduced as part of this cleanup — changes are limited to version updates and removal of unused/duplicate references

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The solution builds with zero package-related warnings or errors after cleanup
- **SC-002**: All existing unit tests pass without any modifications to test code after cleanup
- **SC-003**: Every package reference in every .csproj file is on the latest stable, non-deprecated version compatible with .NET 10 at the time of the cleanup
- **SC-004**: No duplicate or unused package references remain in any .csproj file after cleanup
- **SC-005**: NuGet package restore completes with zero dependency conflict or version mismatch warnings

## Assumptions

- The target framework for all projects is .NET 10, and package compatibility is evaluated against this version
- "Latest stable" means the most recent release that is not marked as a preview, release candidate, or deprecated on NuGet.org at the time of cleanup
- Polly is considered in scope via its presence as a dependency of `Microsoft.Extensions.Http.Resilience`; no direct Polly package reference is expected unless one is discovered during the audit
- System.Text.Json is in scope if an explicit package reference is found; it is not expected to be added if absent since .NET 10 includes it as a framework component
- No test code modifications are permitted — if a package update would require test changes, that version should be skipped in favour of the most recent compatible version
- Only direct package references in .csproj files are in scope; transitive dependency conflicts beyond what NuGet resolves automatically are out of scope
