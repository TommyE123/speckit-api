# Quickstart: JSONPlaceholder Core Data Layer

**Feature**: 001-jsonplaceholder-core  
**Date**: 2026-05-23

> **Scope note**: Feature 001 delivers only the core data layer (HTTP client, DTOs, service). No API endpoints or web host are exposed in this feature. The quickstart focuses on building the solution and running the unit test suite.

---

## Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| .NET SDK | 10.0+ | https://dotnet.microsoft.com/download |
| Git | any | https://git-scm.com |

Verify your setup:

```powershell
dotnet --version   # should print 10.x.x
git --version
```

---

## 1. Clone and checkout

```powershell
git clone https://github.com/TommyE123/speckit-api.git
cd speckit-api
git checkout 001-jsonplaceholder-core
```

---

## 2. Restore dependencies

```powershell
dotnet restore
```

---

## 3. Build

```powershell
dotnet build --configuration Release
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 4. Run unit tests

```powershell
dotnet test --configuration Release --no-build
```

Expected output:
```
Passed!  - Failed: 0, Passed: N, Skipped: 0, Total: N
```

All tests run in under 5 seconds on a clean machine with no network access (SC-004).

---

## 5. Run tests with coverage (optional)

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

Coverage reports are written to `TestResults/` in the test project directory.

---

## Project Layout

```text
speckit-api/
├── SpecKitApi.sln
├── src/
│   └── SpecKitApi/
│       ├── SpecKitApi.csproj
│       ├── appsettings.json          # Base URL for JSONPlaceholder
│       ├── Clients/
│       │   ├── IJsonPlaceholderClient.cs
│       │   └── JsonPlaceholderClient.cs
│       ├── DTOs/
│       │   ├── AlbumDto.cs
│       │   └── PhotoDto.cs
│       ├── Models/
│       │   ├── Album.cs
│       │   ├── Photo.cs
│       │   └── AlbumWithPhotos.cs
│       └── Services/
│           ├── IAlbumService.cs
│           └── AlbumService.cs
└── tests/
    └── SpecKitApi.Tests/
        ├── SpecKitApi.Tests.csproj
        ├── Clients/
        │   └── JsonPlaceholderClientTests.cs
        └── Services/
            └── AlbumServiceTests.cs
```

---

## Key Configuration

`src/SpecKitApi/appsettings.json`:

```json
{
  "JsonPlaceholderOptions": {
    "BaseUrl": "https://jsonplaceholder.typicode.com"
  }
}
```

Override for local or CI testing by setting the environment variable:
```
JsonPlaceholderOptions__BaseUrl=https://your-mock-server
```

---

## What is NOT in this feature

- HTTP API endpoints or controllers (added in feature 002+)
- `Program.cs` / minimal API host wiring (added in feature 002+)
- `curl` examples (added when endpoints exist)
- Database or persistent storage (not in scope)

---

## Next Steps

Once this feature is merged, feature 002 will add Minimal API endpoints that expose `IAlbumService` over HTTP, at which point a full `curl` runbook will be added to `README.md`.
