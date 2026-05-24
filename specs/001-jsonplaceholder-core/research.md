# Research: JSONPlaceholder Core Data Layer

**Feature**: 001-jsonplaceholder-core  
**Phase**: 0 — Outline & Research  
**Date**: 2026-05-23

---

## Summary

All major technology decisions for this feature are prescribed by the project constitution and the feature spec. No unresolved `NEEDS CLARIFICATION` items exist after reviewing both documents. This research phase confirms and records those decisions with rationale.

---

## Decision 1 — HTTP Client Pattern

**Decision**: Use `IHttpClientFactory` with a named typed client (`JsonPlaceholderClient`) registered via `AddHttpClient<IJsonPlaceholderClient, JsonPlaceholderClient>()`.

**Rationale**: The constitution mandates `IHttpClientFactory` with typed clients for all outbound HTTP calls. Typed clients wrap the `HttpClient` in a domain-scoped class, ensuring proper lifetime management (avoiding socket exhaustion), and making the injection surface mockable via an interface for unit tests.

**Alternatives considered**:
- Raw `HttpClient` (singleton or `new`) — Rejected: socket exhaustion risk, harder to test, violates constitution mandate.
- `RestSharp` or `Refit` — Rejected: YAGNI (Principle V) and constitution preference for standard library; `System.Net.Http` + `System.Text.Json` covers all requirements.

---

## Decision 2 — Polly Resilience Policy

**Decision**: Apply a Polly `AddTransientHttpErrorPolicy` with an exponential-backoff retry (3 attempts, 1s/2s/4s) and a 10-second overall timeout per request, configured at registration time via `AddPolicyHandler`.

**Rationale**: The constitution mandates Polly retry and timeout policies for all external dependencies. JSONPlaceholder is a stable but publicly hosted service; transient network errors are possible. 3 retries with backoff is a conservative, well-established default for read-only public APIs. 10 seconds is generous given the small payloads (100 albums, 5000 photos).

**Alternatives considered**:
- No resilience policy — Rejected: constitution prohibits it.
- Circuit breaker — Deferred: no failure-rate data yet; YAGNI (Principle V). Can be added in a future feature if needed.

---

## Decision 3 — Serialization

**Decision**: `System.Text.Json` with default `JsonSerializerOptions` (camelCase property names handled via `[JsonPropertyName]` attributes on DTOs).

**Rationale**: Constitution mandates `System.Text.Json` only; Newtonsoft.Json is explicitly prohibited. The JSONPlaceholder API returns camelCase JSON (`userId`, `albumId`, `thumbnailUrl`) which maps directly to C# DTOs annotated with `[JsonPropertyName]`.

**Alternatives considered**:
- Newtonsoft.Json — Rejected: constitution prohibition.
- Source-generated serialization — Deferred: YAGNI; standard reflection-based serialization is adequate for 5000 photos.

---

## Decision 4 — Testing Stack and Strategy

**Decision**: xUnit for test runner; Moq for mocking `IJsonPlaceholderClient` in service tests. No other frameworks. Tests live in `tests/SpecKitApi.Tests/`.

**Rationale**: Constitution mandates xUnit + Moq. Service unit tests mock `IJsonPlaceholderClient` so no live HTTP call is made during `dotnet test`. Client unit tests verify request construction using `MockHttpMessageHandler` (hand-rolled or via `RichardSzalay.MockHttp`, a well-established helper library).

**Alternatives considered**:
- NSubstitute — Rejected: constitution mandates Moq.
- Integration tests with live JSONPlaceholder — Out of scope for feature 001 per spec assumption; would also violate SC-004 (tests run in under 5 seconds without live connection).

---

## Decision 5 — Domain Model vs DTO Separation

**Decision**: Maintain separate DTO types (`AlbumDto`, `PhotoDto`) for external API shapes and internal domain types (`Album`, `Photo`, `AlbumWithPhotos`). A mapping step inside `AlbumService` converts DTOs to domain models.

**Rationale**: Constitution mandates "DTOs required to separate external API shapes from internal domain models. No external types may leak across layer boundaries." The JSONPlaceholder field names (`url`, `thumbnailUrl`) and the domain model property names (`ImageUrl`, `ThumbnailUrl`) differ enough to warrant explicit mapping. This also insulates the domain from any future JSONPlaceholder API changes.

**Alternatives considered**:
- Single class for both DTO and domain — Rejected: violates constitution.
- AutoMapper — Rejected: YAGNI; two entities with four fields each do not justify a mapping library dependency.

---

## Decision 6 — Project and Solution Layout

**Decision**: Follow the constitution-mandated layout exactly:
- Solution file: `SpecKitApi.sln` at repository root.
- Source project: `src/SpecKitApi/SpecKitApi.csproj`.
- Test project: `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`.
- Source folders: `Clients/`, `Services/`, `Models/`, `DTOs/`.
- No `Endpoints/` folder in this feature (no API endpoints in scope for 001).

**Rationale**: Constitution specifies this layout. Using the solution root keeps CI and tooling simple. Single source project is appropriate for a single bounded service; splitting into multiple projects would be premature (Principle V).

**Alternatives considered**:
- Separate `SpecKitApi.Core` class library — Rejected: YAGNI. A single project with internal visibility modifiers is sufficient for this scope.

---

## Decision 7 — Base URL Configuration

**Decision**: Store `https://jsonplaceholder.typicode.com` in `appsettings.json` under key `JsonPlaceholderOptions:BaseUrl`. Bind to a `JsonPlaceholderOptions` POCO via `IOptions<JsonPlaceholderOptions>`.

**Rationale**: Constitution prohibits hardcoded URLs. Using an options POCO rather than raw `IConfiguration` is idiomatic for .NET and enables easy override in test configuration (`appsettings.Test.json`) or environment variables.

**Alternatives considered**:
- `IConfiguration["JsonPlaceholder:BaseUrl"]` direct read — Rejected: less type-safe and makes test overrides messier.
- Environment variable only — Rejected: `appsettings.json` is the primary configuration source per constitution.

---

## NEEDS CLARIFICATION — All Resolved

| Item | Resolution |
|------|-----------|
| HTTP client lifetime | `IHttpClientFactory` typed client — managed by DI container |
| Resilience policy parameters | 3-retry exponential backoff + 10s timeout |
| Serialization library | `System.Text.Json` — mandated by constitution |
| Test doubles for HTTP | Moq for service layer; `MockHttpMessageHandler` for client layer |
| Project layout | Constitution-mandated single `src/SpecKitApi/` project |
| Base URL storage | `appsettings.json` → `JsonPlaceholderOptions` POCO |
