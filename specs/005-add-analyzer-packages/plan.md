# Implementation Plan: Add Analyzer and Formatting Packages

**Branch**: `006-add-analyzer-packages` | **Date**: 2026-05-24 | **Spec**: `specs/005-add-analyzer-packages/spec.md`

**Input**: Feature specification from `specs/005-add-analyzer-packages/spec.md`

## Summary

Introduce the same analyzer and formatter package set in both projects with exact version pinning, enforce build failure on any analyzer/formatter finding, and bring the repository to a full-build zero-warning state. Enforcement is shared across source and test projects, while generated or auto-produced files are excluded using standard CSharpier and Roslyn mechanisms.

## Technical Context

| Item | Value |
|---|---|
| Language / Framework | C# 13 on .NET 10 SDK-style projects |
| Primary Dependencies | CSharpier.MsBuild, JetBrains.Annotations, Roslynator.Analyzers, Roslynator.CodeAnalysis.Analyzers, Roslynator.Formatting.Analyzers |
| Package Version Strategy | Exact version pins in both `src/SpecKitApi/SpecKitApi.csproj` and `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` |
| Enforcement Strategy | Build fails on analyzer/formatter findings via warnings-as-errors, plus zero-warning full-build gate |
| Generated File Handling | Standard exclusions via `.csharpierignore` and `.editorconfig` generated-code patterns |
| Storage | N/A |
| Testing | xUnit test suite via `dotnet test SpecKitApi.slnx`; build quality verified with full solution build |
| Target Platform | Windows/Linux/macOS developer machines and CI build agents running .NET 10 SDK |
| Project Type | Backend web API solution (no new runtime features in this change) |
| Performance Goals | No runtime behavior changes; quality goal is deterministic clean build output |
| Constraints | No `NoWarn` or pragma-based suppression for CSharpier/Roslynator; same package set and same enforcement level across src/tests |
| Scale / Scope | 2 project files, shared build configuration, repository-wide formatting/analyzer cleanup |

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate (Phase 0 entry)

| Principle | Applicability | Status |
|---|---|---|
| I. API-First Design | No API contract shape changes or endpoint additions are in scope | ✅ PASS — contract surface remains unchanged |
| II. Spec-Driven Development | Spec is clarified and complete for package pinning, enforcement strictness, generated-file handling, parity across projects, and full-build zero warnings | ✅ PASS |
| III. Test-First (NON-NEGOTIABLE) | Change is tooling/configuration-focused; validation still requires build/test verification tasks before completion | ✅ PASS — test/build verification steps explicitly included |
| IV. Observability & Structured Logging | No endpoint behavior is altered; logging contracts remain unchanged | ✅ PASS |
| V. Simplicity & YAGNI | Prefer shared config and standard tooling exclusion mechanisms; no new architecture or speculative abstractions | ✅ PASS |

### Quality Gates

| Gate | Status | Notes |
|---|---|---|
| Spec Gate | ✅ PASS | `specs/005-add-analyzer-packages/spec.md` clarified, no unresolved items |
| Plan Gate | ✅ PASS | This plan documents all constraints and enforcement decisions |
| Contract Gate | ✅ PASS (N/A) | Internal tooling change only; no external interface contract needed |
| Test Gate | ⏳ PENDING | `dotnet test SpecKitApi.slnx` must pass after remediation |
| Observability Gate | ✅ PASS (N/A) | No observability behavior changes in scope |

### Post-Design Re-check (Phase 1 exit)

All five constitution principles remain satisfied after Phase 1 outputs. No additional complexity exceptions are required.

## Project Structure

### Documentation (this feature)

```text
specs/005-add-analyzer-packages/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── tasks.md
```

### Source Code (repository root)

```text
src/SpecKitApi/
└── SpecKitApi.csproj                    # MODIFY: add pinned analyzer/formatter packages

tests/SpecKitApi.Tests/
└── SpecKitApi.Tests.csproj              # MODIFY: add same pinned analyzer/formatter packages

Directory.Build.props                    # NEW: shared warnings-as-errors/analyzer enforcement
.editorconfig                            # NEW or MODIFY: generated-file analyzer exclusions
.csharpierignore                         # NEW or MODIFY: generated-file formatter exclusions
```

**Structure Decision**: Keep the existing two-project solution layout and apply uniform analyzer/formatter behavior through shared root configuration plus explicit package references in each project. This provides parity between source and tests without introducing additional projects or custom tooling.

## Complexity Tracking

No constitution violations requiring justification.
