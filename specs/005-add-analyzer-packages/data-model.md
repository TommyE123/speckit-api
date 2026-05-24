# Data Model: Add Analyzer and Formatting Packages

**Feature**: `005-add-analyzer-packages` | **Phase**: 1 - Design & Contracts

This feature introduces no runtime domain entities. The model below describes configuration entities and verification outputs required to satisfy the quality constraints.

---

## Entity: ProjectToolingConfiguration

Represents analyzer/formatter package configuration for a project file.

| Field | Type | Constraints |
|---|---|---|
| `ProjectPath` | `string` | Must be one of: `src/SpecKitApi/SpecKitApi.csproj`, `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` |
| `Packages` | `PackagePin[]` | Must contain exactly the five required packages with exact versions |
| `AnalyzerMetadataApplied` | `bool` | Must be `true` for CSharpier and Roslynator package references (`PrivateAssets` and `IncludeAssets` pattern) |
| `AnnotationMetadataApplied` | `bool` | Must be `true` for `JetBrains.Annotations` (`PrivateAssets=all`) |

Validation rules:
- Package IDs and versions must match the planned pinned baseline.
- Source and test projects must have equivalent package set and versions.

---

## Entity: PackagePin

Represents a pinned package reference.

| Field | Type | Constraints |
|---|---|---|
| `PackageId` | `string` | One of required package IDs |
| `Version` | `string` | Exact SemVer-style pin; no ranges or floating values |
| `PrivateAssets` | `string` | `all` for analyzer/formatter/annotation tooling |
| `IncludeAssets` | `string?` | Required for CSharpier and Roslynator packages |

Required package IDs:
- `CSharpier.MsBuild`
- `JetBrains.Annotations`
- `Roslynator.Analyzers`
- `Roslynator.CodeAnalysis.Analyzers`
- `Roslynator.Formatting.Analyzers`

---

## Entity: EnforcementPolicy

Defines build enforcement behavior shared by source and test projects.

| Field | Type | Constraints |
|---|---|---|
| `TreatWarningsAsErrors` | `bool` | Must be `true` |
| `NoWarnSuppressionsForTargetTools` | `bool` | Must be `false` for CSharpier/Roslynator suppressions |
| `Scope` | `string[]` | Must include both source and test projects |

Validation rules:
- Build fails when CSharpier or Roslynator emits diagnostics.
- Full build output contains zero warnings after remediation.

---

## Entity: GeneratedFileExclusionPolicy

Describes standard exclusions for generated/auto-produced files.

| Field | Type | Constraints |
|---|---|---|
| `FormatterExclusions` | `string[]` | Stored in `.csharpierignore`; patterns target generated artifacts |
| `AnalyzerExclusions` | `string[]` | Stored in `.editorconfig` as generated code patterns |
| `MechanismType` | `string` | Must be standard tooling mechanism (not pragma or ad hoc source edits) |

Validation rules:
- Generated-file exclusions do not rely on `NoWarn` or `#pragma warning disable`.
- Exclusions do not reduce enforcement for normal hand-written source files.

---

## Entity: BuildQualityResult

Captures verification results for the feature acceptance gates.

| Field | Type | Constraints |
|---|---|---|
| `BuildCommand` | `string` | `dotnet build SpecKitApi.slnx --no-incremental` |
| `Warnings` | `int` | Must be `0` |
| `Errors` | `int` | Must be `0` |
| `TestsPassed` | `bool` | Must be `true` for `dotnet test SpecKitApi.slnx` |

State transition:
- `Pending` -> `RemediationInProgress` -> `CleanBuildAndTestsPassing`

Exit condition:
- State reaches `CleanBuildAndTestsPassing` with zero warnings on full build output.

---

## External Interface Contracts

No external API or CLI contract changes are introduced by this feature. Contract artifact generation under `contracts/` is intentionally skipped.
