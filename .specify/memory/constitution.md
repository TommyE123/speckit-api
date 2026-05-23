<!--
## Sync Impact Report

**Version change**: 1.0.1 → 1.0.2

### Modified Principles
- None — all five Core Principles unchanged.

### Added Sections
- None.

### Removed Sections
- None.

### Templates Updated
- ✅ `.specify/templates/plan-template.md` — Constitution Check section is dynamic; references
  constitution gates at runtime. No changes required.
- ✅ `.specify/templates/spec-template.md` — Generic structure; aligns with Spec-Driven
  Development (Principle II). No changes required.
- ✅ `.specify/templates/tasks-template.md` — **Actually fixed** (v1.0.1 incorrectly reported
  this as done): Removed "OPTIONAL - only if tests requested" labeling from the Tests preamble
  and all three user story test section headers. These now read MANDATORY with explicit
  Red-Green-Refactor instruction, consistent with Principle III (Test-First, NON-NEGOTIABLE).
- ✅ `.specify/templates/checklist-template.md` — Generic placeholder structure; no principle
  references require updating.
- ✅ `.github/copilot-instructions.md` — References current plan for context; consistent with
  Spec-Driven Development principle. No changes required.

### Deferred Items
- None — all placeholders resolved.

### v1.0.1 → v1.0.2 Rationale
PATCH bump: applied the `.specify/templates/tasks-template.md` fix that was erroneously
reported as complete in v1.0.1 but was never actually written to disk. Four occurrences of
"OPTIONAL - only if tests requested" were replaced with "MANDATORY — write first, confirm
failing" language, enforcing Principle III (Test-First, NON-NEGOTIABLE). No principle content
was altered.

### Historical: v1.0.0 → v1.0.1 Rationale
PATCH bump: initial constitution fill — all five Core Principles defined, Governance,
Development Standards, Quality Gates, and Technical Stack sections populated from project
context. No template changes were applied at that time (contrary to the v1.0.1 report).
-->

# speckit-api Constitution

## Core Principles

### I. API-First Design

Every feature MUST be designed as an explicit API contract before any implementation begins.
Contracts (OpenAPI/REST) MUST be documented in `specs/[###-feature]/contracts/` prior to coding.
Breaking changes to an existing API contract MUST trigger a version bump and MUST include a
migration plan. Internal-only refactors that do not alter the public contract are exempt.

**Rationale**: Contract-first ensures that consumers and implementations stay aligned and that
changes are deliberate, reviewed, and versioned — not accidental side effects of implementation.

### II. Spec-Driven Development

No feature implementation may begin without a completed specification created via the Spec Kit
workflow (`/speckit.specify` → `/speckit.plan` → `/speckit.tasks`). Specs MUST define user
stories with acceptance scenarios, functional requirements, and measurable success criteria.
Ad-hoc implementation that bypasses the spec workflow is prohibited.

**Rationale**: A written specification anchors scope, prevents scope creep, and creates a shared
contract between developers, reviewers, and stakeholders before a single line of code is written.

### III. Test-First (NON-NEGOTIABLE)

TDD is mandatory for all non-trivial logic. The workflow MUST follow Red-Green-Refactor:
tests are written first, confirmed failing, then implemented against. Tests MUST be approved
before implementation begins. Unit tests cover business logic; contract tests cover API surfaces;
integration tests cover inter-service communication and shared schemas.

**Rationale**: Tests written after the fact tend to confirm existing code rather than verify
requirements. Writing tests first catches design problems early and produces a living
specification of expected behaviour.

### IV. Observability & Structured Logging

All API endpoints MUST emit structured logs (JSON format) for every request and error.
Errors MUST include a correlation ID, HTTP status, endpoint, and sanitised context.
Health-check and readiness endpoints MUST be present in every service. Silent failures
(swallowed exceptions, empty catch blocks) are prohibited.

**Rationale**: Invisible failures are the hardest to diagnose. Consistent structured logging
makes incidents traceable, metrics derivable, and alerts actionable without re-deploying.

### V. Simplicity & YAGNI

Implement the simplest solution that satisfies the current spec. Complexity MUST be justified
in `plan.md` under the Complexity Tracking table before it is introduced. Premature abstractions,
speculative generalisations, and over-engineering are prohibited. Dependencies MUST be chosen
conservatively — prefer the standard library or well-established packages over novel ones.

**Rationale**: Simple code is easier to review, test, and extend. Every layer of indirection
added for "future flexibility" that never materialises becomes permanent maintenance burden.

## Development Standards

- **Scripting**: PowerShell for all automation and setup scripts (cross-platform PS Core).
- **AI Integration**: GitHub Copilot is the primary AI assistant; copilot-instructions.md MUST
  stay up-to-date and reference the current plan for feature context.
- **Branching**: Feature branches follow the sequential numbering convention
  (`###-feature-name`); created via `/speckit.git.feature` before any spec work begins.
- **Commits**: Commits MUST be atomic and scoped to a single task or logical change;
  commit messages MUST follow Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`, etc.).
- **Code Review**: All PRs MUST pass the Constitution Check in `plan.md` before merging.
  Reviewers MUST verify compliance with all five Core Principles.

## Quality Gates

The following gates MUST be satisfied before a feature branch may be merged to `main`:

1. **Spec Gate** — `specs/[###-feature]/spec.md` exists and is complete (no `NEEDS CLARIFICATION`
   tokens remaining).
2. **Plan Gate** — `specs/[###-feature]/plan.md` Constitution Check section passes all five
   principles; any violations are documented in the Complexity Tracking table.
3. **Contract Gate** — All new or modified API endpoints have corresponding contract files in
   `specs/[###-feature]/contracts/`.
4. **Test Gate** — All tests are written and were confirmed failing before implementation;
   test suite passes (zero failures, zero skips on required tests).
5. **Observability Gate** — All new endpoints include structured logging and a health check
   is present or updated.

## Governance

This constitution supersedes all other practices documented in this repository. Any conflicting
guidance in README files, code comments, or ad-hoc notes is subordinate to this document.

**Amendment Procedure**:
1. Open a PR with the proposed constitution change and a completed Sync Impact Report.
2. Increment `CONSTITUTION_VERSION` according to semantic versioning rules:
   - **MAJOR** — principle removed, renamed, or redefined in a backward-incompatible way.
   - **MINOR** — new principle or section added, or materially expanded guidance.
   - **PATCH** — clarifications, wording fixes, or non-semantic refinements.
3. Update all dependent templates and artifacts listed in the Sync Impact Report.
4. PR requires at least one approving review before merge.
5. Update `LAST_AMENDED_DATE` to the merge date.

**Compliance Review**: Compliance is checked per-PR via the Quality Gates above. A full
constitution audit SHOULD be conducted at the start of each major feature cycle to verify
templates and tooling remain aligned.

For runtime development guidance, refer to `.github/copilot-instructions.md` and the current
feature's `plan.md`.

## Technical Stack

- **Framework**: .NET 10 (C# 13), Minimal APIs only — no MVC controllers.
- **HTTP Clients**: `IHttpClientFactory` with typed clients for all outbound calls.
  Polly policies required for retries and timeouts on external dependencies.
- **Serialization**: `System.Text.Json` only. Newtonsoft.Json is prohibited.
- **Testing**: xUnit + Moq. No other test frameworks permitted.
- **Data Contracts**: DTOs required to separate external API shapes from internal
  domain models. No external types may leak across layer boundaries.
- **Configuration**: All URLs and external addresses in `appsettings.json`. No
  hardcoded values anywhere in code.
- **Project Structure**: Solution file at root. Single project under `/src/ProjectName/`.
  Folder conventions: `/Endpoints`, `/Services`, `/Models`, `/DTOs`, `/Clients`.
- **Documentation**: XML comments on all public methods and interfaces. `README.md`
  covering how to run and how to test with `curl`.

**Version**: 1.0.2 | **Ratified**: 2026-05-23 | **Last Amended**: 2026-05-23
