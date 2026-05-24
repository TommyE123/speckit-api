# Quickstart: Add Analyzer and Formatting Packages

**Feature**: `005-add-analyzer-packages`

This quickstart validates the planned quality gates for analyzer and formatter enforcement.

## Prerequisites

- .NET 10 SDK installed
- Restore access to NuGet feeds

## 1) Restore dependencies

From repository root:

```powershell
dotnet restore
```

Expected result:
- Restore succeeds for both projects.
- All five analyzer/formatter/annotation packages resolve in both projects.

## 2) Format check baseline

```powershell
dotnet csharpier .
```

Expected result:
- Command completes successfully.
- Repository is normalized to CSharpier style before strict build enforcement checks.

## 3) Full solution build quality gate

```powershell
dotnet build SpecKitApi.slnx --no-incremental
```

Expected result:
- Build exits successfully.
- Zero warnings and zero errors in full build output.
- Analyzer or formatter findings fail the build until fixed.

## 4) Full test suite regression gate

```powershell
dotnet test SpecKitApi.slnx
```

Expected result:
- All tests pass.
- No test changes are required solely to bypass analyzer/formatter checks.

## 5) Verify parity between source and tests

Manual verification points:
- `src/SpecKitApi/SpecKitApi.csproj` and `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj` reference the same five packages with exact versions.
- Both projects are subject to the same enforcement level.

## 6) Verify generated-file exclusions

Manual verification points:
- Generated or auto-produced files are excluded via standard mechanisms in `.csharpierignore` and `.editorconfig`.
- No `NoWarn` additions or `#pragma warning disable` directives are introduced for CSharpier or Roslynator.

## Definition of Done

The feature is complete when all of the following are true:
- Exact package versions are pinned in both projects.
- Build fails on analyzer/formatter findings.
- Generated-file exclusions are configured using standard tooling mechanisms.
- Source and test projects enforce the same quality level.
- Full solution build output reports zero warnings and zero errors.
- Full test suite passes.
