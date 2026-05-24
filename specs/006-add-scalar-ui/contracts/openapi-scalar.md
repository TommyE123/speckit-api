# Interface Contract: OpenAPI Document and Scalar UI

**Feature**: `006-add-scalar-ui` | **Version**: `1.0.0` | **Date**: 2026-05-24

This contract defines the developer-facing documentation interfaces added by this feature.

---

## Interface 1: OpenAPI Document

### Route

- `GET /openapi/v1.json`

### Availability

- Available in Development and non-Development environments.

### Response

- `200 OK`
- `Content-Type: application/json`
- Body: OpenAPI 3.x JSON document

### Required Content

The document MUST include:
- Paths for `GET /albums` and `GET /health`.
- Human-readable summaries for both operations.
- Query parameter documentation for `userId` on `/albums`:
  - location: query
  - type: integer
  - required: false
- Response documentation for `/albums` including status codes `200`, `400`, and `500`.

### Error Behavior

- If OpenAPI generation is misconfigured, the route may return a server error (`500`), which is surfaced by the standard API exception handler.

---

## Interface 2: Scalar UI

### Route

- `GET /scalar`

### Availability

- Development environment only.

### Response (Development)

- `200 OK`
- HTML UI is rendered and references the OpenAPI document from `/openapi/v1.json`.

### Response (Non-Development)

- Route is not mapped.
- Requests return `404 Not Found`.

---

## Behavioral Consistency Requirements

1. Scalar must present executable operations for:
- `GET /albums`
- `GET /health`

2. Executing operations via Scalar must reflect real runtime API behavior.

3. Launch profile behavior:
- Development launch profile browser target is `/scalar`.

---

## Out of Scope

- Authentication/authorization for docs UI.
- API versioning strategy beyond document name `v1`.
- Changes to endpoint business logic, service logic, or data-access logic.
