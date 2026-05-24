# Research: Add Analyzer and Formatting Packages

**Feature**: `005-add-analyzer-packages` | **Phase**: 0 - Research & Unknowns Resolution

## Summary

This feature is a tooling and code-quality enforcement change. Clarifications in the spec removed policy ambiguity, so Phase 0 focuses on concrete implementation decisions for exact version pinning, strict failure behavior, generated-file exclusions, and parity between source and test projects.

---

## Decision 1 - Exact package version pinning strategy

**Decision**: Pin exact versions in each project file for all five packages, using the same values in both projects.

**Pinned set for planning baseline**:
- `CSharpier.MsBuild` = `1.2.6`
- `JetBrains.Annotations` = `2025.2.4`
- `Roslynator.Analyzers` = `4.15.0`
- `Roslynator.CodeAnalysis.Analyzers` = `4.15.0`
- `Roslynator.Formatting.Analyzers` = `4.15.0`

**Rationale**:
- Satisfies FR-001/FR-002/FR-003 and SC-006 requiring exact pins.
- Avoids version drift between `src` and `tests`.
- Keeps dependency state deterministic in CI and local environments.

**Alternatives considered**:
- Central Package Management (`Directory.Packages.props`): Rejected for this feature plan because requirements explicitly emphasize explicit exact pins in each project.
- Floating ranges (for example `4.*`): Rejected because non-deterministic and violates clarified spec.

---

## Decision 2 - Fail build on analyzer and formatter findings

**Decision**: Enforce warnings-as-errors for the build so CSharpier and Roslynator findings fail the build, and hold the full solution to zero warnings.

**Rationale**:
- Directly satisfies FR-004 and SC-001/SC-007.
- Makes CI and local enforcement behavior consistent.
- Prevents warning accumulation and quality drift over time.

**Alternatives considered**:
- Keep warnings but fail only in CI script parsing: Rejected because fragile and can diverge from local developer behavior.
- Promote only selected diagnostic IDs to errors: Rejected because clarification requires strict behavior and full-build zero warnings.

---

## Decision 3 - Generated/auto-produced file exclusions

**Decision**: Exclude generated files through standard mechanisms:
- `.csharpierignore` for formatter exclusion.
- `.editorconfig` generated-code patterns for analyzer exclusion behavior.

**Rationale**:
- Meets FR-008 and SC-008 without ad hoc edits.
- Keeps exclusions maintainable and visible in standard repository configuration.
- Avoids suppressing diagnostics in source with pragmas or `NoWarn`.

**Alternatives considered**:
- Rule suppression via `NoWarn` or pragma in generated files: Rejected by FR-005 and clarification.
- Removing generated files from build: Rejected as too invasive and not needed for this scope.

---

## Decision 4 - Same enforcement in source and tests

**Decision**: Apply the same package set and enforcement configuration to both `src/SpecKitApi/SpecKitApi.csproj` and `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`.

**Rationale**:
- Satisfies FR-009 and SC-009.
- Ensures quality gates do not differ by project type.
- Reduces maintenance risk when new diagnostics are introduced.

**Alternatives considered**:
- Strict analyzers only in source, relaxed settings in tests: Rejected by clarified requirement for same enforcement level.

---

## Decision 5 - Full-build quality verification

**Decision**: Use full solution build and full test suite as acceptance gates:
- `dotnet build SpecKitApi.slnx --no-incremental`
- `dotnet test SpecKitApi.slnx`

**Rationale**:
- Verifies SC-001 and SC-003 directly against solution-wide output.
- Confirms that zero warnings applies to total build output, not only selected analyzers.

**Alternatives considered**:
- Project-by-project build checks only: Rejected because they can miss solution-level warnings and do not fully cover clarified success criteria.

---

## Open Clarifications

No unresolved clarifications remain for planning. Phase 0 is complete.
