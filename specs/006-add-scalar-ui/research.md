# Research: Add Scalar OpenAPI UI

**Feature**: `006-add-scalar-ui` | **Phase**: 0 - Research & Unknowns Resolution

## Summary

This feature is an additive documentation-surface enhancement for the existing minimal API. Clarification decisions from the spec are now translated into concrete implementation choices for OpenAPI document naming, Scalar exposure policy, endpoint metadata, and launch behavior.

---

## Decision 1 - OpenAPI document naming and route

**Decision**: Use a single OpenAPI document named `v1` and expose it at `/openapi/v1.json`.

**Rationale**:
- Directly satisfies FR-001 and clarified route behavior.
- Aligns with the built-in ASP.NET Core OpenAPI document naming pattern.
- Keeps Scalar document source deterministic and easy to test.

**Alternatives considered**:
- Single unnamed/default document at `/openapi.json`: Rejected due to explicit clarification.
- Environment-specific route naming: Rejected because it adds complexity and weakens consistency.

---

## Decision 2 - Environment exposure policy

**Decision**:
- Map Scalar UI only in Development at `/scalar`.
- Do not map `/scalar` in non-Development environments (requests return 404).
- Keep `/openapi/v1.json` available in all environments.

**Rationale**:
- Satisfies FR-003, SC-005, and SC-007 simultaneously.
- Preserves secure default posture by not exposing UI tooling in Production.
- Supports operational introspection and contract consumers in non-Development environments via the OpenAPI JSON document.

**Alternatives considered**:
- Block `/scalar` with 403 in Production: Rejected; mapping then denying is more moving parts than not mapping.
- Restrict OpenAPI JSON to Development only: Rejected by clarified requirement to keep JSON available in all environments.

---

## Decision 3 - Endpoint documentation metadata strategy

**Decision**: Add endpoint metadata directly in minimal API mappings (`WithSummary`, `WithDescription`, and explicit response metadata) for `/albums` and `/health`.

**Rationale**:
- Satisfies FR-005, FR-006, and FR-007 without introducing controller layers or external doc files.
- Keeps API contract details co-located with route definitions, reducing drift.
- Works naturally with .NET minimal API OpenAPI generation.

**Alternatives considered**:
- XML-doc-only OpenAPI enrichment: Rejected because endpoint-level requirement clarity is stronger with explicit route metadata.
- Custom OpenAPI document transformers for all metadata: Rejected as unnecessary complexity for current scope.

---

## Decision 4 - Launch profile behavior

**Decision**: Update launch profile URL from `/albums` to `/scalar` for the Development profile.

**Rationale**:
- Directly satisfies FR-004 and SC-001.
- Makes the new documentation UI the default local developer entry point.

**Alternatives considered**:
- Keep launch URL as `/albums`: Rejected because it weakens discoverability and does not meet clarified UX intent.
- Add separate launch profile only for Scalar: Rejected as unnecessary unless multiple profile workflows are requested.

---

## Decision 5 - Dependency strategy for UI integration

**Decision**: Add `Scalar.AspNetCore` as the only new package and keep OpenAPI generation on built-in ASP.NET Core functionality.

**Rationale**:
- Meets feature goal to replace Swagger UI with Scalar.
- Honors Constitution Principle V (Simplicity & YAGNI) by avoiding additional tooling layers.
- Minimizes implementation and maintenance burden.

**Alternatives considered**:
- Swashbuckle + Swagger UI continuation: Rejected by feature objective.
- Custom static UI over raw OpenAPI JSON: Rejected as out-of-scope and higher effort.

---

## Open Clarifications

No unresolved clarifications remain for planning.
