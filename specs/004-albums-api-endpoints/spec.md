# Feature Specification: Albums API Endpoints

**Feature Branch**: `004-albums-api-endpoints`

**Created**: 2026-05-24

**Status**: Draft

**Input**: User description: "Build the API endpoints for the JSONPlaceholder Web API. Wire up the existing AlbumService and JsonPlaceholderClient (built in 001) into Minimal API endpoints in a .NET 10 ASP.NET Core application."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Retrieve All Albums with Photos (Priority: P1)

A consumer of the API can call a single endpoint to receive a complete list of all albums, each enriched with its full set of associated photos. The caller does not need to make separate requests for albums and photos — the API delivers the combined view in one response.

**Why this priority**: This is the primary data-delivery capability of the API. It is the foundational endpoint that all other scenarios build on, and it directly fulfils the core integration requirement.

**Independent Test**: Can be fully tested by calling `GET /albums` and asserting that the response contains albums each with their associated photos, all HTTP metadata is correct, and no internal errors are exposed to the caller.

**Acceptance Scenarios**:

1. **Given** the API is running and the external data source is available, **When** a client sends `GET /albums`, **Then** the response status is 200 and the body contains a list of albums, each with its associated photos embedded.
2. **Given** the API is running, **When** a client sends `GET /albums`, **Then** the response body is valid JSON and every album contains a non-null photos collection.
3. **Given** the external data source is unavailable, **When** a client sends `GET /albums`, **Then** the response status is 500 and the body contains a structured JSON error message with no raw exception details exposed.

---

### User Story 2 - Retrieve Albums Filtered by User ID (Priority: P2)

A consumer of the API can call the albums endpoint with an optional user ID query parameter to receive only the albums — and their photos — belonging to that specific user. This allows clients to scope results to a single user without receiving the full dataset.

**Why this priority**: Filtered retrieval by user is the primary business query pattern described in the requirements. It depends on the unfiltered endpoint (P1) being in place but delivers focused, practical value on its own.

**Independent Test**: Can be fully tested by calling `GET /albums?userId=1` and verifying that only albums owned by user 1 are returned, each with their photos, and that albums from other users are absent.

**Acceptance Scenarios**:

1. **Given** the API is running and user 1 owns albums, **When** a client sends `GET /albums?userId=1`, **Then** the response status is 200 and the body contains only albums belonging to user 1, each with their associated photos.
2. **Given** the API is running and no albums exist for user 99, **When** a client sends `GET /albums?userId=99`, **Then** the response status is 200 and the body contains an empty list.
3. **Given** the API is running, **When** a client sends `GET /albums?userId=abc`, **Then** the response status is 400 and the body contains a structured JSON error indicating the parameter is invalid.
4. **Given** the API is running, **When** a client sends `GET /albums?userId=0`, **Then** the response status is 400 and the body contains a structured JSON error indicating the user ID must be a positive integer.

---

### User Story 3 - Health Check Endpoint (Priority: P3)

An operations team member or deployment system can probe the API's health endpoint to confirm the service has started and is accepting requests. The health check is lightweight, fast, and requires no authentication.

**Why this priority**: Health checks are a standard operational requirement that enables load balancers, container orchestrators, and monitoring systems to verify service availability. Independently useful and low-risk to implement.

**Independent Test**: Can be fully tested by calling `GET /health` immediately after startup and verifying a 200 response is returned promptly.

**Acceptance Scenarios**:

1. **Given** the API is running, **When** a client sends `GET /health`, **Then** the response status is 200.
2. **Given** the API has just started successfully, **When** `GET /health` is called, **Then** the response is returned in under 500 milliseconds.

---

### User Story 4 - Structured Error Responses (Priority: P2)

A client developer receiving an error from the API can parse a consistent, structured JSON error body rather than encountering raw exception messages or plain text. Every error response — regardless of whether it is a validation failure, a server error, or an unexpected condition — follows the same shape.

**Why this priority**: Consistent error handling is essential for API consumers to build reliable client code. Without it, error handling in consumers becomes fragile and inconsistent.

**Independent Test**: Can be fully tested by deliberately triggering each error condition (invalid input, server fault) and asserting the response body always follows the same JSON structure with no stack traces or raw exception text.

**Acceptance Scenarios**:

1. **Given** a request with an invalid `userId` parameter, **When** the API returns a 400 response, **Then** the body is a JSON object containing at least an error message field; no stack trace or raw exception text is present.
2. **Given** an internal failure occurs during request processing, **When** the API returns a 500 response, **Then** the body is a JSON object containing a safe, generic error message; no internal detail is exposed.

---

### User Story 5 - Structured Logging with Correlation ID (Priority: P3)

A developer or support engineer investigating an issue can trace a specific API request through the logs using a correlation ID. Every log entry for a given request — from entry to exit, including errors — carries the same correlation ID, making root-cause analysis practical.

**Why this priority**: Structured logging and correlation IDs are a foundational observability capability. They do not affect the user-facing API contract but are critical for production operations.

**Independent Test**: Can be fully tested by making requests to any endpoint and inspecting log output to confirm correlation IDs appear consistently across all log entries for each request.

**Acceptance Scenarios**:

1. **Given** the API receives any request, **When** the request is processed, **Then** all log entries for that request carry the same correlation ID.
2. **Given** an error occurs during request processing, **When** the error is logged, **Then** the log entry contains the correlation ID, the endpoint path, and a safe error description.

---

### Edge Cases

- What happens when `userId` is provided but is a floating-point number (e.g., `userId=1.5`)?
- How does the system behave when the external data source returns a very large payload (all 100 albums and 5000 photos)?
- What happens when the API receives a request with additional unknown query parameters beyond `userId`?
- How does the system handle concurrent requests to `GET /albums` when the external source is slow?
- What happens if `appsettings.json` is missing or the base URL configuration entry is absent at startup?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST expose a `GET /albums` endpoint that returns all albums with their associated photos as a JSON response.
- **FR-002**: The `GET /albums` endpoint MUST accept an optional `userId` query parameter that, when provided, filters the response to include only albums belonging to that user.
- **FR-003**: The `GET /albums` endpoint MUST return HTTP 400 when `userId` is provided but is not a valid positive integer.
- **FR-004**: The `GET /albums` endpoint MUST return HTTP 500 with a structured JSON error body when an internal or external failure prevents the request from completing.
- **FR-005**: The system MUST expose a `GET /health` endpoint that returns HTTP 200 when the API is running.
- **FR-006**: All error responses MUST use a consistent structured JSON format and MUST NOT expose raw exception messages, stack traces, or internal system details.
- **FR-007**: The system MUST emit structured log entries for every request, including a correlation ID that is consistent across all log entries for the same request.
- **FR-008**: The system MUST register all services using the existing `ServiceCollectionExtensions` and MUST NOT duplicate or bypass existing registration logic.
- **FR-009**: All API endpoints MUST use the Minimal API pattern — no MVC controllers may be introduced.
- **FR-010**: All configuration values (such as the external data source base URL) MUST be read from `appsettings.json`; no values may be hardcoded in application code.
- **FR-011**: All 31 existing unit tests MUST continue to pass after the API endpoint layer is added.
- **FR-012**: The system MUST include integration tests covering at least: successful retrieval of all albums, successful filtered retrieval by user ID, and the 400 response for an invalid `userId`.

### Key Entities

- **Album**: A named collection of photos owned by a user. Carries a unique album ID, an owning user ID, and a title. Defined by the existing data layer from feature 001.
- **Photo**: A single image belonging to an album. Carries a unique photo ID, a parent album ID, a title, a full-size image location, and a thumbnail location. Defined by the existing data layer from feature 001.
- **AlbumWithPhotos**: The combined view pairing an Album with its associated Photos collection. This is the primary response payload returned by `GET /albums`.
- **ErrorResponse**: A structured JSON object returned on error responses. Contains at minimum a human-readable message field. No implementation details or raw exceptions are included.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: `dotnet run` starts the API without errors, and `GET /health` returns 200 within 5 seconds of startup.
- **SC-002**: `GET /albums` returns a JSON array containing albums with their associated photos; verified by integration test on every build.
- **SC-003**: `GET /albums?userId=1` returns only albums belonging to user 1; all other users' albums are absent from the response.
- **SC-004**: `GET /albums?userId=abc` returns HTTP 400 with a structured JSON error body containing no raw exception text.
- **SC-005**: All 31 pre-existing unit tests and all new integration tests pass on a clean checkout without manual configuration.
- **SC-006**: Every log entry produced during a request includes a correlation ID field that matches across all entries for that request.
- **SC-007**: No error response body contains a stack trace, exception type name, or internal file path.

## Assumptions

- The `AlbumService` and `JsonPlaceholderClient` built in feature 001 are complete, stable, and require no modification to support this feature.
- The existing `ServiceCollectionExtensions` class correctly registers all dependencies required for the service and client layers; this feature only needs to call it.
- The external JSONPlaceholder base URL is the only configuration value that requires externalisation; other settings (timeouts, retry policies) use sensible defaults from feature 001.
- Integration tests will use the real `AlbumService` wired to a stubbed or faked HTTP client for the external source — they will not make live network calls to JSONPlaceholder.
- A floating-point `userId` (e.g., `1.5`) is treated as invalid and returns HTTP 400.
- Unknown query parameters beyond `userId` are silently ignored.
- The API is a single-process, single-instance application; no distributed tracing, clustering, or load-balancing concerns are in scope.
- A `README` curl example for `GET /albums` is in scope for this feature, as this is the first feature to expose public endpoints.
- Performance under concurrent load beyond standard ASP.NET Core defaults is out of scope for this feature.
