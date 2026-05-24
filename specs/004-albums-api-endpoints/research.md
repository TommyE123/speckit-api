# Research: Albums API Endpoints

**Feature**: `004-albums-api-endpoints` | **Phase**: 0 — Research & Unknowns Resolution

## Summary

Four questions required investigation before the Phase 1 design could be locked. All are resolved below. No `NEEDS CLARIFICATION` tokens remain.

---

## Q1 — Integration testing pattern for .NET 10 Minimal APIs

**Question**: How do we integration-test a `WebApplication` (Minimal API) with a stubbed `IJsonPlaceholderClient` without making real network calls?

**Decision**: Use `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<TProgram>`) with `ConfigureTestServices` to replace `IJsonPlaceholderClient`.

**Rationale**:
- `WebApplicationFactory<TProgram>` spins up the real ASP.NET Core pipeline in-process (no TCP port; `HttpClient` talks directly to the test host).
- `ConfigureTestServices(services => { services.RemoveAll<IJsonPlaceholderClient>(); services.AddSingleton<IJsonPlaceholderClient>(stub); })` replaces the typed client after the normal `AddJsonPlaceholderServices` call, so all real wiring is exercised except the outbound HTTP.
- A hand-rolled `StubJsonPlaceholderClient` (implementing `IJsonPlaceholderClient`) is sufficient — no Moq needed in integration tests, keeping them readable.
- `Program` must end with `public partial class Program { }` so the test assembly can reference the entry-point type as `TProgram`.

**Packages needed**:
- `Microsoft.AspNetCore.Mvc.Testing` v10.x — add to `tests/SpecKitApi.Tests/SpecKitApi.Tests.csproj`.

**Alternatives considered**:

| Alternative | Rejected because |
|---|---|
| Separate `SpecKitApi.IntegrationTests` project | Adds a third project with no benefit; existing test project already has all required infrastructure (xUnit v3, Moq, project reference to `SpecKitApi`). Violates Principle V (YAGNI). |
| Live calls to JSONPlaceholder in integration tests | Spec assumption explicitly excludes live network calls from integration tests; flaky in CI |
| `TestServer` directly (no `WebApplicationFactory`) | `WebApplicationFactory` is the idiomatic .NET wrapper that handles host lifetime correctly; `TestServer` directly requires more boilerplate |

---

## Q2 — Correlation ID middleware approach

**Question**: How should a correlation ID be threaded through every log entry for a request, using only the standard library?

**Decision**: Custom `CorrelationIdMiddleware` that reads the `X-Correlation-ID` request header (falling back to `HttpContext.TraceIdentifier`), echoes it in the response header, and opens an `ILogger.BeginScope` dictionary with key `"CorrelationId"` for the duration of the request.

**Rationale**:
- `HttpContext.TraceIdentifier` is already unique per request — it serves as the fallback when the caller does not supply a header.
- `ILogger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id })` is the standard `Microsoft.Extensions.Logging` mechanism for enriching all log entries within a scope. No extra SDK is required.
- Echoing the correlation ID in the `X-Correlation-ID` response header lets clients correlate responses back to their own request tracking.
- Placing the middleware before `UseExceptionHandler` ensures the scope is open even when the error handler runs.

**Middleware registration order** (in `Program.cs`):
```
app.UseMiddleware<CorrelationIdMiddleware>()  // sets scope first
app.UseExceptionHandler(...)                  // scope already open, error log includes CorrelationId
app.MapAlbums()
app.MapGet("/health", ...)
```

> **Addendum — plan update**: `CorrelationIdMiddleware` must additionally store the resolved correlation ID in `HttpContext.Items["CorrelationId"]` so that endpoint handlers and the global exception handler can embed it in `ErrorResponse.CorrelationId` (required by FR-006, SC-008). The `BeginScope` enrichment for logging and the `HttpContext.Items` storage for error responses are complementary and both required.

**Alternatives considered**:

| Alternative | Rejected because |
|---|---|
| OpenTelemetry `ActivitySource` | Adds `System.Diagnostics.DiagnosticSource` + OTel SDK dependency; distributed tracing is explicitly out of scope (spec Assumptions section) |
| Serilog `LogContext.PushProperty` | Third-party dependency; constitution requires conservative dependency choices (Principle V) |
| `IHttpContextAccessor` + scoped `CorrelationIdService` | Adds an extra service registration and accessor overhead; `BeginScope` in the middleware achieves the same result with less code |

---

## Q3 — Structured JSON 500 error handler

**Question**: How do we return a consistent `{ "message": "..." }` JSON body on unhandled exceptions without exposing stack traces?

**Decision**: `app.UseExceptionHandler(exceptionHandlerApp => { exceptionHandlerApp.Run(async ctx => { ... await ctx.Response.WriteAsJsonAsync(new ErrorResponse("An unexpected error occurred.")); }); })` — a self-contained lambda with no extra route.

**Rationale**:
- The lambda overload (available since .NET 7) is fully self-contained: no `/error` route is registered in the endpoint table, so callers cannot invoke it directly.
- `WriteAsJsonAsync` uses the app's configured `System.Text.Json` options, keeping serialization consistent.
- The generic message `"An unexpected error occurred."` satisfies FR-006 and SC-007 (no stack trace, no exception type names, no internal paths).
- The correlation ID scope opened by `CorrelationIdMiddleware` is still active when the exception handler runs, so the error log entry automatically carries the correlation ID.

**Alternatives considered**:

| Alternative | Rejected because |
|---|---|
| `MapGet("/error", ...)` route | Registers a discoverable endpoint that callers could invoke directly; also requires `app.UseStatusCodePages` for 404/405 handling |
| `ProblemDetails` middleware (`AddProblemDetails` + `UseStatusCodePages`) | Returns RFC 7807 Problem Details shape which differs from the spec's `ErrorResponse { Message }` contract; swapping to the spec shape requires overriding the factory anyway |
| `try/catch` in every endpoint handler | Duplicates error-handling logic; a middleware is the idiomatic single place for unhandled exception policy |

> **Correction — plan update**: The initial research answer for Q3 showed `ErrorResponse("An unexpected error occurred.")` with only a `message` field. This was incorrect: the spec (FR-006, SC-004, SC-008, Key Entities) explicitly requires three fields — `message`, `code`, and `correlationId`. The corrected constructor call is `new ErrorResponse("An unexpected error occurred.", "INTERNAL_ERROR", correlationId)` where `correlationId` is read from `HttpContext.Items["CorrelationId"]`. Correspondingly, the 400 handler calls `new ErrorResponse("userId must be a positive integer.", "INVALID_PARAMETER", correlationId)`. See `data-model.md` for the full updated `ErrorResponse` definition and code value table.

---

## Q4 — `userId` query parameter validation strategy

**Question**: How do we return a custom `ErrorResponse` JSON body for `userId=abc`, `userId=0`, and `userId=1.5` — cases that must all yield HTTP 400 with a consistent body?

**Decision**: Declare the `userId` parameter as `string?` in the endpoint handler, then `int.TryParse` + range-check manually.

**Rationale**:
- If `userId` is declared `int?`, the ASP.NET Core parameter binder intercepts non-integer values and returns its own 400 body before the handler executes. Overriding that body requires `IActionResultExecutor` hooks or `TypedResults` result filters — more complexity than switching to `string?`.
- `string?` gives the handler complete control: `!int.TryParse(userId, out var id) || id <= 0` covers `abc`, `0`, `-1`, `1.5`, and `null` (the last being the "no filter" case).
- The only trade-off is that the handler must parse manually; given a single parameter, this is two lines of code.

**Validation logic**:
```csharp
if (userId is not null)
{
    if (!int.TryParse(userId, out var parsedId) || parsedId <= 0)
        return Results.Json(new ErrorResponse("userId must be a positive integer."), statusCode: 400);
    // use parsedId for the service call
}
```

**Edge cases resolved**:

| Input | Parsed as | HTTP response |
|---|---|---|
| _(absent)_ | `null` | 200 — returns all albums |
| `1` | valid `int` | 200 — returns filtered albums |
| `abc` | parse fails | 400 |
| `0` | parse succeeds; `<= 0` check | 400 |
| `-5` | parse succeeds; `<= 0` check | 400 |
| `1.5` | `int.TryParse` fails (not an integer string) | 400 |
| `99` (no albums) | valid `int` | 200 — empty array |

**Alternatives considered**:

| Alternative | Rejected because |
|---|---|
| `int?` with `IEndpointFilter` to intercept binder errors | More code; the filter receives a `DefaultEndpointFilterInvocationContext` — accessing the binder error requires examining `ValidationProblemDetails`, which is less readable than a two-line manual parse |
| `int?` with custom `IParameterBindingMetadata` | Significant complexity for a single query parameter |
