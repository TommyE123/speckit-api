# Feature Specification: Add Analyzer and Formatting Packages

**Feature Branch**: `006-add-analyzer-packages`

**Created**: 2026-05-24

**Status**: Draft

**Input**: User description: "Add analyzer and formatting packages to all projects in the solution and resolve any violations they surface."

## Clarifications

### Session 2026-05-24

- Q: How should package versions be specified? → A: Pin exact package versions in each project at implementation time.
- Q: How strictly should analyzer and formatter findings be enforced? → A: Fail the build for both CSharpier and Roslynator findings.
- Q: How should generated files be handled? → A: Exclude generated or auto-produced files from formatter and analyzer enforcement using standard tooling mechanisms.
- Q: Should source and test projects use the same enforcement level? → A: Yes; apply the same analyzer and formatter package set and enforcement level in both source and test projects.
- Q: What does zero warnings mean for this feature? → A: Zero warnings applies to the full build output, not only CSharpier and Roslynator diagnostics.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent Code Formatting Enforced (Priority: P1)

A developer working in the repository wants all code to be uniformly formatted. By adding CSharpier as a build-time formatter, any formatting inconsistency is surfaced as a build warning, encouraging the whole team to keep code style consistent without manual enforcement.

**Why this priority**: Formatting consistency is foundational — it reduces noise in code reviews and ensures a uniform baseline before static analysis runs.

**Independent Test**: Can be fully tested by building the solution after package addition and verifying that CSharpier reports zero formatting violations on the existing codebase.

**Acceptance Scenarios**:

1. **Given** the solution is built after CSharpier.MsBuild is added to both projects, **When** all existing code already conforms to CSharpier formatting rules (or has been reformatted), **Then** the build completes with zero CSharpier-related warnings or errors.
2. **Given** a developer introduces deliberately mis-formatted code, **When** they build the solution, **Then** the build surfaces a CSharpier formatting violation instead of silently accepting it.

---

### User Story 2 - Static Analysis Violations Resolved (Priority: P2)

A developer wants Roslynator's code-analysis and formatting analyzers active in both the main library and the test project so that common code-quality issues (unused members, unnecessary casts, style inconsistencies, etc.) are caught at build time.

**Why this priority**: Static analysis increases long-term maintainability and catches latent bugs; it builds on the clean formatting baseline established in P1.

**Independent Test**: Can be fully tested by building the solution after adding all three Roslynator packages and confirming zero new Roslynator warnings exist on the existing codebase.

**Acceptance Scenarios**:

1. **Given** Roslynator.Analyzers, Roslynator.CodeAnalysis.Analyzers, and Roslynator.Formatting.Analyzers are added to both projects, **When** the solution is built, **Then** no Roslynator diagnostics are emitted (all violations have been fixed).
2. **Given** a developer introduces code that violates a Roslynator rule, **When** they build the solution, **Then** the build surfaces the specific Roslynator diagnostic.

---

### User Story 3 - Code-Contract Annotations Available (Priority: P3)

A developer wants JetBrains.Annotations available in both projects so that nullability attributes (e.g., `[NotNull]`, `[CanBeNull]`) and flow-analysis hints can be used throughout the codebase, improving IDE inspections and reducing null-related defects.

**Why this priority**: Annotations improve developer-facing tooling but do not change runtime behaviour; they add value after the higher-priority quality gates are in place.

**Independent Test**: Can be fully tested by verifying the JetBrains.Annotations package is resolvable in both projects and that the solution builds cleanly with it present.

**Acceptance Scenarios**:

1. **Given** JetBrains.Annotations is added to both projects, **When** the solution is built, **Then** the build succeeds with zero new warnings or errors introduced by the package.
2. **Given** a developer uses a JetBrains annotation attribute in the source, **When** the solution is built, **Then** the attribute is recognised and no unknown-type error is emitted.

---

### Edge Cases

- What happens if a package version conflict arises between an analyzer and an existing transitive dependency?
- How does the build behave if CSharpier detects a file it cannot parse (e.g., generated code)?
- What if Roslynator flags code inside auto-generated files that cannot easily be changed?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Both `src/SpecKitApi/SpecKitApi.csproj` and `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` MUST reference the CSharpier.MsBuild package using an exact stable version with appropriate `PrivateAssets` and `IncludeAssets` metadata so it does not become a runtime dependency.
- **FR-002**: Both project files MUST reference JetBrains.Annotations using an exact stable version.
- **FR-003**: Both project files MUST reference Roslynator.Analyzers, Roslynator.CodeAnalysis.Analyzers, and Roslynator.Formatting.Analyzers using exact stable versions with appropriate analyzer-package metadata.
- **FR-004**: The solution MUST fail the build when CSharpier or Roslynator emits any diagnostic, and MUST build with zero errors and zero warnings across the full build output after all violations are resolved.
- **FR-005**: Developers MUST NOT suppress CSharpier or Roslynator violations using `NoWarn` MSBuild properties or `#pragma warning disable` directives — all violations MUST be corrected in source.
- **FR-006**: All existing automated tests MUST continue to pass without modification after the packages are added and violations are resolved.
- **FR-007**: No new production functionality MUST be introduced as part of this change — the scope is strictly tooling configuration and code-quality remediation.
- **FR-008**: Generated or auto-produced files MUST be excluded from CSharpier and Roslynator enforcement using standard tooling mechanisms rather than ad hoc source edits.
- **FR-009**: The source and test projects MUST use the same analyzer and formatter package set and the same enforcement level.

### Key Entities

- **Project File**: An MSBuild `.csproj` file that declares package references, build settings, and asset metadata for a single compilable unit within the solution.
- **Analyzer Package**: A NuGet package that provides Roslyn diagnostic analyzers; it participates only at build/analysis time and carries no runtime payload.
- **Formatting Package**: A NuGet package (CSharpier.MsBuild) that integrates a code formatter into the MSBuild pipeline as a build step.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The solution builds to completion with zero errors and zero warnings of any kind on a clean checkout after this change is merged.
- **SC-002**: All packages (CSharpier.MsBuild, JetBrains.Annotations, Roslynator.Analyzers, Roslynator.CodeAnalysis.Analyzers, Roslynator.Formatting.Analyzers) appear in the restored dependency graph for both projects.
- **SC-003**: The full automated test suite passes with a 100% pass rate after the change.
- **SC-004**: Zero `NoWarn` entries for CSharpier or Roslynator rules exist in any project file or source file after the change.
- **SC-005**: All pre-existing CSharpier formatting violations and Roslynator diagnostic violations are resolved — the violation count drops from any non-zero baseline to zero.
- **SC-006**: Every analyzer and formatting package reference in both project files uses an explicit exact version value rather than a floating range.
- **SC-007**: A newly introduced CSharpier or Roslynator violation causes the build to fail until the source is corrected.
- **SC-008**: Generated or auto-produced files do not cause formatter or analyzer build failures once the agreed exclusions are applied.
- **SC-009**: Building either the source project or the test project surfaces the same analyzer and formatter enforcement behavior for equivalent violations.

## Assumptions

- The solution uses SDK-style `.csproj` files compatible with NuGet `<PackageReference>` syntax.
- NuGet.org is accessible to restore packages; no private feed configuration is required.
- The selected exact stable versions of all five packages are mutually compatible with the project's current target framework.
- Auto-generated files (if any) that cannot be reformatted are either excluded via `.csharpierignore` or already conformant.
- Auto-generated or tool-produced files can be identified reliably enough to exclude them through standard analyzer and formatter configuration.
- The `PrivateAssets="all"` and `IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive"` metadata pattern (standard for analyzer-only packages) will be applied to Roslynator packages and CSharpier.MsBuild.
