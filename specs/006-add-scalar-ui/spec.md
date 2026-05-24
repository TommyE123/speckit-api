# Feature Specification: Add Scalar OpenAPI UI

**Feature Branch**: `006-add-scalar-ui`

**Created**: 2026-05-24

**Status**: Draft

**Input**: User description: "Add Scalar as the OpenAPI UI for the SpecKitApi .NET 10 Web API. Scalar is the modern replacement for Swagger/Swashbuckle and is the recommended OpenAPI UI for .NET 9 and above."

## Clarifications

### Session 2026-05-24

- Q: Which OpenAPI document naming/route should Scalar use? -> A: Single document named v1 at /openapi/v1.json.
- Q: What should /scalar return outside Development? -> A: Scalar route is not mapped; /scalar returns 404.
- Q: Should OpenAPI JSON be exposed outside Development? -> A: OpenAPI JSON available in all environments; Scalar UI remains Development-only.
- Q: Should `GET /health` declare a 500 response in OpenAPI? -> A: No; FR-007 applies to `/albums` response documentation scope.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Launch Interactive API Documentation (Priority: P1)

A developer runs the application locally and the browser automatically opens the Scalar UI at `/scalar`, presenting a fully interactive documentation page for all API endpoints. The developer can read endpoint descriptions, review expected request/response shapes, and execute test calls directly from the browser without leaving the page.

**Why this priority**: This is the core value of the feature — replacing the old launch URL with a functional, modern API explorer. Without this, the feature has no user-facing value.

**Independent Test**: Can be fully tested by running `dotnet run`, observing the browser opening at `/scalar`, and verifying that all endpoints are listed with documentation.

**Acceptance Scenarios**:

1. **Given** the application is started with `dotnet run`, **When** the browser opens, **Then** it navigates automatically to the Scalar UI at `/scalar`
2. **Given** the Scalar UI is loaded, **When** a developer views the page, **Then** all three endpoints (`GET /albums`, `GET /albums?userId={id}`, `GET /health`) are listed with human-readable summaries
3. **Given** the Scalar UI is loaded, **When** a developer selects `GET /albums` and clicks the test/execute button, **Then** the live response is returned and displayed inline

---

### User Story 2 - Test Endpoints via Scalar UI (Priority: P2)

A developer uses the Scalar UI to test the albums endpoints interactively. They can invoke `GET /albums` to retrieve all albums, and `GET /albums?userId={id}` by supplying a user ID parameter in the UI form, and see the JSON response in the page.

**Why this priority**: Interactive testing is the primary reason to adopt an API UI tool. This makes the feature meaningfully useful for day-to-day development.

**Independent Test**: Can be tested independently by sending requests from the Scalar UI and comparing the responses to those from a direct HTTP client (e.g., curl).

**Acceptance Scenarios**:

1. **Given** the Scalar UI is open, **When** a developer executes `GET /albums` with no parameters, **Then** a list of albums with photos is returned and displayed
2. **Given** the Scalar UI is open, **When** a developer supplies a valid `userId` integer and executes `GET /albums`, **Then** only albums belonging to that user are returned
3. **Given** the Scalar UI is open, **When** a developer supplies an invalid `userId` (e.g., a negative number or a string), **Then** a `400 Bad Request` response is shown with a descriptive error message

---

### User Story 3 - Health Endpoint Visible in Documentation (Priority: P3)

The `GET /health` endpoint is visible and testable in the Scalar UI alongside the albums endpoints, giving operators a single place to verify both the API surface and service liveness.

**Why this priority**: While the health endpoint exists today, having it documented in the UI adds operational value with minimal effort. It is independent of the albums functionality.

**Independent Test**: Can be tested by locating `/health` in the Scalar UI and executing it, receiving a `200 OK` with `{ "status": "healthy" }`.

**Acceptance Scenarios**:

1. **Given** the Scalar UI is open, **When** a developer locates `GET /health`, **Then** it is listed as a documented endpoint with a summary
2. **Given** a developer executes `GET /health` from the UI, **Then** a `200 OK` response containing `{ "status": "healthy" }` is displayed

---

### Edge Cases

- What happens when the Scalar UI is accessed in a non-Development environment? The UI should only be mounted in the Development environment, following the same convention used for Swagger in .NET projects.
- How does the system handle a request to `/scalar` when OpenAPI document generation is misconfigured? The browser should show a clear error page rather than a blank/broken UI.
- What if an existing test or integration relies on the browser launch URL being `/albums`? The `launchSettings.json` change must not break the existing test suite.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST serve a single OpenAPI JSON document at `/openapi/v1.json` in all environments that describes all current API endpoints.
- **FR-002**: The application MUST mount the Scalar interactive UI at the `/scalar` path.
- **FR-003**: The Scalar UI MUST be accessible only when the application is running in the Development environment, and the `/scalar` route MUST be unmapped in non-Development environments.
- **FR-004**: The application startup configuration MUST open `/scalar` in the browser automatically when run locally with `dotnet run`.
- **FR-005**: The OpenAPI document MUST include human-readable summaries for `GET /albums`, `GET /albums?userId={id}`, and `GET /health`.
- **FR-006**: The OpenAPI document MUST describe the query parameter `userId` for the `/albums` endpoint, including its type and optional/required status.
- **FR-007**: The OpenAPI document MUST describe the possible response codes (`200`, `400`, `500`) for the `/albums` endpoint.
- **FR-008**: All existing automated tests MUST continue to pass after the changes are applied.
- **FR-009**: No changes MUST be made to existing business logic, service implementations, or data-access code.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Running `dotnet run` opens the browser at `/scalar` within 5 seconds of process start under normal local Development conditions.
- **SC-002**: All three documented endpoints (`GET /albums`, `GET /albums?userId={id}`, `GET /health`) are visible and individually executable from the Scalar UI.
- **SC-003**: A test call to each endpoint from the Scalar UI returns the correct HTTP status code and a non-empty response body.
- **SC-004**: All existing automated tests pass with no modifications to test code.
- **SC-005**: The Scalar UI is not accessible when the application is run in a Production environment profile, and requests to `/scalar` return `404 Not Found`.
- **SC-006**: The OpenAPI document is reachable at `/openapi/v1.json` while running in Development mode and is the document consumed by Scalar.
- **SC-007**: The OpenAPI document remains reachable at `/openapi/v1.json` in Production mode while Scalar UI remains unavailable.

## Assumptions

- The project targets .NET 10 and uses the built-in `Microsoft.AspNetCore.OpenApi` package for OpenAPI document generation (available natively from .NET 9+), rather than Swashbuckle.
- The `Scalar.AspNetCore` NuGet package is the appropriate client library for mounting the Scalar UI in ASP.NET Core.
- The Scalar UI is mounted in the Development environment only, consistent with standard .NET conventions for developer tooling endpoints.
- No authentication or API key is required to access the Scalar UI in the local development environment.
- The existing three endpoints (`GET /albums`, `GET /albums?userId={id}`, `GET /health`) represent the full API surface to be documented; no new endpoints are introduced by this feature.
- The `launchSettings.json` change (from `albums` to `scalar`) is the only configuration file change required outside of `Program.cs`.
