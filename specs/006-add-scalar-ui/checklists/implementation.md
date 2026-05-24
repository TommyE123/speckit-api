# Implementation Checklist: Add Scalar OpenAPI UI

**Purpose**: Validate that implementation requirements are complete, clear, consistent, and measurable enough to build against — unit-testing the requirements themselves, not the code.
**Created**: 2026-05-24
**Feature**: [spec.md](../spec.md) | [plan.md](../plan.md) | [tasks.md](../tasks.md)

---

## Requirement Completeness

- [ ] CHK001 - Are NuGet package version-pin requirements documented with a rationale for the wildcard `2.*` constraint, or is the minimum acceptable version specified? [Completeness, plan.md Phase 1]
- [ ] CHK002 - Are DI registration ordering requirements explicitly stated — does the spec or plan document that `AddOpenApi("v1")` must precede `builder.Build()` and why? [Completeness, Spec §Assumptions]
- [ ] CHK003 - Are middleware pipeline ordering requirements fully specified for `MapOpenApi()` and `MapScalarApiReference()` relative to all other registered middleware (not just `CorrelationIdMiddleware` and `MapAlbums`)? [Completeness, plan.md Phase 3]
- [ ] CHK004 - Is the named document identifier (`"v1"`) and its relationship to the served route (`/openapi/v1.json`) explicitly documented as a coupling constraint, so implementers know changing one requires changing both? [Completeness, Spec §FR-001]
- [ ] CHK005 - Are requirements defined for the format/schema of the generated OpenAPI JSON document (e.g., OpenAPI version 3.x, JSON encoding only, no YAML variant)? [Gap, Spec §FR-001]
- [ ] CHK006 - Is the specific `launchSettings.json` profile name that must be modified identified in the spec or plan, rather than assumed from context? [Completeness, plan.md Phase 3 — T004]
- [ ] CHK007 - Are requirements defined for how the integration test (T009) boots the application with `ASPNETCORE_ENVIRONMENT=Production` — e.g., via `WebApplicationFactory` environment override — and is the exact mechanism documented? [Completeness, tasks.md T009]
- [ ] CHK008 - Are the `.Produces<IReadOnlyList<AlbumWithPhotosResponse>>(200)` type parameters fully specified, including whether the response wrapper type must exactly match the runtime serialization type? [Completeness, tasks.md T005]

---

## Requirement Clarity

- [ ] CHK009 - Is "accessible only in Development" (FR-003) precisely scoped — does it apply to every non-Development value of `ASPNETCORE_ENVIRONMENT` (e.g., `Staging`, `Testing`, custom values), or only `Production`? [Clarity, Spec §FR-003]
- [ ] CHK010 - Is "all existing automated tests" in FR-008 clearly bounded — does it refer exclusively to integration tests in `SpecKitApi.Tests`, or does it include any other test projects, build checks, or static analysis? [Clarity, Spec §FR-008]
- [ ] CHK011 - Is SC-001's "within 5 seconds" threshold precisely defined — measured from which event (process start, first HTTP bind, OS browser handoff) and under what baseline hardware/OS conditions? [Clarity, Spec §SC-001]
- [ ] CHK012 - Is "human-readable summaries" in FR-005 given a quality standard — minimum length, no technical jargon, a specific tone — or is it left entirely to implementer discretion? [Clarity, Spec §FR-005]
- [ ] CHK013 - Is the "optional positive integer" constraint on `userId` in FR-006 precisely defined: does "positive" mean `> 0`, `>= 0`, or non-negative; and is this constraint validated at the API layer or documented only in the OpenAPI schema? [Ambiguity, Spec §FR-006]
- [ ] CHK014 - Is the term "non-Development environment" consistently used throughout spec.md, plan.md, and tasks.md, or do some documents use "Production" when they mean "any non-Development environment"? [Clarity, Spec §FR-003, SC-005]
- [ ] CHK015 - Is `ProducesProblem(500)` for the `/albums` endpoint justified in the spec with evidence that a 500 response is actually reachable from the current implementation path, not merely theoretical? [Clarity, Spec §FR-007]
- [ ] CHK016 - Is the default Scalar route resolution (no arguments → `/scalar`) cited with a package documentation reference, so the assumption can be validated if the package version changes? [Clarity, plan.md Phase 3 — T003]

---

## Requirement Consistency

- [ ] CHK017 - Does FR-001 ("OpenAPI JSON available in all environments") align unambiguously with FR-003 ("Scalar UI only in Development") — is there a stated reason the documents treat the JSON endpoint and the UI endpoint with different environment policies? [Consistency, Spec §FR-001, §FR-003]
- [ ] CHK018 - Are the response codes documented in FR-007 (`200`, `400`, `500`) consistent with the actual acceptance scenarios defined in User Story 2 (which only exercises `200` and `400` interactively)? [Consistency, Spec §FR-007, US2 §Acceptance Scenarios]
- [ ] CHK019 - Do the success criteria SC-006 and SC-007 consistently describe the same route `/openapi/v1.json` as reachable in both Development and Production, matching FR-001's "all environments" language? [Consistency, Spec §SC-006, §SC-007, §FR-001]
- [ ] CHK020 - Is the task T009 integration test scope consistent between plan.md (new file `ScalarUiIntegrationTests.cs`) and tasks.md (which offers `AlbumsEndpointsIntegrationTests.cs` or `ScalarUiIntegrationTests.cs` as alternatives)? [Consistency, plan.md Phase 6, tasks.md T009]
- [ ] CHK021 - Are the `.WithDescription()` strings in T005 and T006 consistent in tone, detail level, and formatting convention, given that they were authored in separate tasks? [Consistency, tasks.md T005, T006]

---

## Acceptance Criteria Quality

- [ ] CHK022 - Can SC-003's "non-empty response body" be objectively verified without knowing expected content — should the criterion specify minimum body structure (e.g., a JSON array for `/albums`, a JSON object for `/health`)? [Measurability, Spec §SC-003]
- [ ] CHK023 - Is SC-004 ("all existing automated tests pass") sufficient as a Definition of Done gate, or should it include a specific test count baseline so regressions from test deletion are also detectable? [Acceptance Criteria, Spec §SC-004]
- [ ] CHK024 - Is SC-002's "individually executable" criterion precise — does it require only that the endpoint is listed and has an execute button, or that a successful round-trip call is made? [Measurability, Spec §SC-002]
- [ ] CHK025 - Are the acceptance scenarios for User Story 2 sufficient to validate FR-007's 500-response requirement, given that none of the three scenarios exercise a server-side error path? [Acceptance Criteria, Spec §US2, §FR-007]

---

## Scenario Coverage

- [ ] CHK026 - Are requirements defined for the Scalar UI's behaviour when the application starts but `/openapi/v1.json` returns an error or malformed document — does the spec address this failure mode? [Gap, Edge Case, Spec §Edge Cases]
- [ ] CHK027 - Are requirements defined for any environment value other than `Development` and `Production` (e.g., `Staging`, `QA`, custom values) — is Scalar's absence in those environments specified or left as an undocumented consequence? [Gap, Spec §FR-003, §Edge Cases]
- [ ] CHK028 - Is the rollback scenario documented — if `Scalar.AspNetCore` causes a startup exception (e.g., due to a version conflict), what is the expected recovery path and who is responsible for it? [Gap, Recovery Flow]
- [ ] CHK029 - Are requirements defined for the case where `dotnet run` is invoked without a browser (e.g., in a CI pipeline or headless environment) — does FR-004's automatic browser-open requirement have an explicit "no-op if no browser" behaviour? [Coverage, Spec §FR-004]
- [ ] CHK030 - Is the behavior of `GET /health` in the OpenAPI document defined if it is called from the Scalar UI while the service is degraded — does the spec address what a non-`200` health response means in the context of the UI? [Coverage, Spec §US3]

---

## Non-Functional Requirements

- [ ] CHK031 - Are security requirements for the `/openapi/v1.json` endpoint in Production explicitly stated — the spec says it is reachable in all environments but does not define authentication, rate-limiting, or IP-restriction requirements. [Gap, Spec §Assumptions]
- [ ] CHK032 - Are CORS requirements for the `/openapi/v1.json` endpoint specified, particularly for any tooling that may request the document from a different origin? [Gap, Non-Functional]
- [ ] CHK033 - Are caching requirements for the OpenAPI JSON document specified — should the response include cache headers, and if so, what TTL is appropriate given that endpoint metadata changes only on deployment? [Gap, Non-Functional]
- [ ] CHK034 - Are performance requirements for the Scalar UI's initial load time defined beyond SC-001 (which measures browser launch time, not page render time)? [Gap, Spec §SC-001]

---

## Dependencies & Assumptions

- [ ] CHK035 - Is the assumption that "no authentication is required to access the Scalar UI in Development" validated against a project security policy or constitution principle, or is it undocumented convenience? [Assumption, Spec §Assumptions]
- [ ] CHK036 - Is the assumption that `Microsoft.AspNetCore.OpenApi` is the OpenAPI generation mechanism (not Swashbuckle) explicitly stated as a constraint that blocks use of alternative packages, and is there a reference to where this is enforced? [Assumption, Spec §Assumptions, plan.md §Technical Context]
- [ ] CHK037 - Is the dependency between `AddOpenApi("v1")` registration (T002) and endpoint metadata chains (T005, T006) documented as a hard blocking dependency in tasks.md, or could an implementer skip T002 and not notice until runtime? [Dependency, tasks.md §Phase Dependencies]
- [ ] CHK038 - Are external dependencies on the `launchSettings.json` profile name documented — if the profile is renamed or the project is cloned with a different profile, is the link between FR-004 and the configuration file entry explicit? [Dependency, Spec §Assumptions]

---

## Traceability

- [ ] CHK039 - Does every functional requirement (FR-001 through FR-009) trace to at least one task in tasks.md, and does every task in tasks.md cite at least one FR or SC? [Traceability]
- [ ] CHK040 - Does the integration test T009 explicitly cite FR-003 and SC-005 in its test method name, class summary, or inline comments so the requirement-to-test link is preserved during future refactoring? [Traceability, tasks.md T009]
- [ ] CHK041 - Are the constitution gate outcomes (plan.md §Constitution Check) traceable to specific items in the spec — e.g., which spec section satisfies the "Test-First" gate — so the gate results can be re-audited if the spec changes? [Traceability, plan.md §Constitution Check]

---

## Notes

- Check items off as completed: `[x]`
- Items marked `[Gap]` indicate a requirement or coverage area absent from the current spec/plan/tasks — resolve by adding the missing requirement or explicitly documenting the intentional omission.
- Items marked `[Ambiguity]` require clarification from the spec author before implementation begins.
- Items marked `[Assumption]` should be validated against the project constitution or a domain expert before accepting the assumption as safe.
- Reference mapping: CHK001–CHK008 → Completeness | CHK009–CHK016 → Clarity | CHK017–CHK021 → Consistency | CHK022–CHK025 → Acceptance Criteria | CHK026–CHK030 → Scenario Coverage | CHK031–CHK034 → Non-Functional | CHK035–CHK038 → Dependencies | CHK039–CHK041 → Traceability
