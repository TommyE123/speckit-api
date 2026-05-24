# Data Model: Add Scalar OpenAPI UI

**Feature**: `006-add-scalar-ui` | **Phase**: 1 - Design & Contracts

This feature introduces no new business-domain entities. The model below captures documentation-surface configuration entities and verification states required to satisfy the spec.

---

## Entity: OpenApiDocumentConfiguration

Represents OpenAPI document exposure and naming.

| Field | Type | Constraints |
|---|---|---|
| `DocumentName` | `string` | Must equal `v1` |
| `DocumentRoute` | `string` | Must equal `/openapi/v1.json` |
| `AvailableEnvironments` | `string[]` | Must include Development and non-Development (all environments) |
| `IncludesEndpoints` | `string[]` | Must include `/albums` and `/health` |

Validation rules:
- Document route resolves successfully with HTTP 200 in Development and Production profiles.
- Generated document includes endpoint summaries and response metadata defined in route mappings.

---

## Entity: ScalarUiConfiguration

Represents interactive OpenAPI UI exposure policy.

| Field | Type | Constraints |
|---|---|---|
| `Route` | `string` | Must equal `/scalar` |
| `EnabledEnvironment` | `string` | Must be `Development` only |
| `OpenApiSourceRoute` | `string` | Must reference `/openapi/v1.json` |
| `NonDevelopmentBehavior` | `string` | Must be `RouteUnmapped404` |

Validation rules:
- `/scalar` returns 200 in Development and renders interactive docs.
- `/scalar` returns 404 when `ASPNETCORE_ENVIRONMENT` is not Development.

---

## Entity: EndpointDocumentationMetadata

Represents OpenAPI metadata coverage for existing endpoints.

| Field | Type | Constraints |
|---|---|---|
| `Route` | `string` | One of `/albums`, `/health` |
| `Summary` | `string` | Non-empty human-readable text |
| `Parameters` | `ParameterDoc[]` | `/albums` includes optional integer `userId` |
| `Responses` | `ResponseDoc[]` | Must include `200`, `400`, and `500` where applicable |

Validation rules:
- `/albums` parameter definition for `userId` is typed and optional.
- Response code metadata matches runtime behavior documented in spec.

---

## Entity: LaunchProfileDocumentationEntry

Represents launch-time developer entry point behavior.

| Field | Type | Constraints |
|---|---|---|
| `ProfileName` | `string` | `SpecKitApi (Development)` |
| `LaunchBrowser` | `bool` | Must remain `true` |
| `LaunchUrl` | `string` | Must equal `scalar` |

Validation rules:
- `dotnet run` with Development profile opens the browser to `/scalar`.

---

## Supporting Types

### ParameterDoc

| Field | Type | Constraints |
|---|---|---|
| `Name` | `string` | `userId` |
| `Location` | `string` | `query` |
| `DataType` | `string` | `integer` |
| `Required` | `bool` | `false` |

### ResponseDoc

| Field | Type | Constraints |
|---|---|---|
| `StatusCode` | `int` | One of `200`, `400`, `500` |
| `Description` | `string` | Non-empty |
| `PayloadShape` | `string` | Matches documented API response schema |

---

## State Transitions

Feature verification state:
- `Planned` -> `Configured` -> `Validated`

Transition conditions:
- `Planned` -> `Configured`: OpenAPI + Scalar wiring and endpoint metadata added.
- `Configured` -> `Validated`: Dev/prod route behavior verified and all automated tests pass.

Exit condition:
- State reaches `Validated` with all success criteria met.
