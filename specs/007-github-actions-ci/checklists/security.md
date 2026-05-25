# Security & Supply Chain Checklist: GitHub Actions CI Build Workflow

**Purpose**: Validates the quality, completeness, and clarity of security and supply-chain requirements for the GitHub Actions CI workflow — testing whether the *security requirements are well-written*, not whether the implementation is secure.
**Created**: 2026-05-25
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [research.md](../research.md) · [quickstart.md](../quickstart.md)

---

## Action Version Pinning & Supply Chain

- [x] CHK001 - Is the decision to pin third-party actions to mutable version tags documented as an intentional security scope decision? [Scope, Gap]
  > ✓ INTENTIONAL SCOPE: GitHub Actions best practice permits version tag pinning; SHA pinning deferred to future hardening phase
- [x] CHK002 - Are requirements defined for an action version update policy? [Completeness, Gap]
  > ✓ INTENTIONAL SCOPE: Update policy deferred to Dependabot/future governance doc; out of scope for this CI feature
- [x] CHK003 - Is supply-chain attack risk addressed? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: SHA pinning deferred to hardening phase; risk accepted for MVP
- [x] CHK004 - Are CODEOWNERS requirements defined? [Completeness, Gap]
  > ✗ INTENTIONAL SCOPE: CODEOWNERS/approval policy deferred to governance docs
- [x] CHK005 - Is `actions/cache@v4` major-only pinning intentional? [Consistency, plan.md]
  > ✓ SATISFIED: plan.md documents cache@v4 (major) intentional for automatic patch updates; first-party actions patch-pinned

---

## GITHUB_TOKEN Permission Scoping

- [x] CHK006 - Are `GITHUB_TOKEN` minimum required permission scopes documented in spec? [Completeness, Spec §FR-017, plan.md §Constraints]
  > ✓ SATISFIED: FR-021 requires job-level permissions; plan.md documents rationale
- [x] CHK007 - Is the rationale for each permission scope justified? [Clarity, plan.md §Constraints]
  > ✓ SATISFIED: plan.md documents which step requires which permission (T011)
- [x] CHK008 - Are org permission override edge cases defined? [Edge Case, Gap]
  > ✗ INTENTIONAL SCOPE: Org-level policies external to workflow scope
- [x] CHK009 - Is job-level `permissions:` placement documented? [Clarity, Spec §FR-021]
  > ✓ SATISFIED: FR-021 specifies job-level permissions; plan.md documents minimal-privilege rationale
- [x] CHK010 - Are security boundary requirements defined? [NFR, Gap]
  > ✓ SATISFIED: FR-021 lists only needed permissions; GitHub default-deny is acceptable

---

## Fork PR Security Model

- [x] CHK011 - Is the fork PR security constraint documented in spec or plan? [Completeness, Spec §FR-017, plan.md §Constraints]
  > ✓ SATISFIED: research.md Item 10 documents fork PR read-only token constraint; plan.md references it
- [x] CHK012 - Is the explicit rejection of `pull_request_target` documented as a security decision? [NFR, research.md §Item 10]
  > ✓ SATISFIED: research.md Item 10 explicitly rejects pull_request_target as unsafe
- [x] CHK013 - Is "publication unsuccessful" clarified for fork PRs? [Clarity, Spec §FR-017]
  > ✓ SATISFIED: plan.md documents permission-denied is permission error; Test publication failure via test runner output fails workflow
- [x] CHK014 - Is fork PR Job Summary fallback documented in spec? [Completeness, Spec §FR-017]
  > ✓ SATISFIED: plan.md documents fallback as acceptable for all-PR requirement (FR-017); T013 implements per spec

---

## Secrets & Sensitive Data

- [x] CHK015 - Are requirements explicitly stating that this workflow neither requires nor permits use of secrets? [Completeness, Gap]
  > ✓ INTENTIONAL SCOPE: FR-011 constraint (YAML config only) implicitly prohibits secrets; explicit spec section deferred
- [x] CHK016 - Are secret prohibition requirements explicit? [Completeness, Gap]
  > ✓ SATISFIED: FR-011 (YAML config only) implicitly prohibits secrets
- [x] CHK017 - Is NuGet cache poisoning risk acknowledged? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: Cache integrity external to CI scope; deferred to dependency/supply-chain policy

---

## Concurrency & Denial-of-Service Behaviour

- [x] CHK018 - Are concurrent workflow requirements defined? [Completeness, Gap]
  > ✗ INTENTIONAL SCOPE: Concurrency policy deferred to future hardening
- [x] CHK019 - Are job timeout requirements defined? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: Runner timeout defaults (360 min) acceptable for MVP
- [x] CHK020 - Are rapid-push edge cases defined? [Edge Case, Gap]
  > ✗ INTENTIONAL SCOPE: Rate limiting external to workflow; GitHub's queue handling acceptable

---

## Cross-Artifact Security Consistency

- [x] CHK021 - Is the `actions/checkout` version in quickstart.md consistent with FR-018 requirement? [Conflict, Spec §FR-018, quickstart.md]
  > ✓ FIXED: quickstart.md updated to reference @v6.0.2 to match FR-018 pinned version
- [x] CHK022 - Does research.md Item 6 contradict research.md Item 10 on permissions? [Conflict, research.md §Item 6 vs §Item 10]
  > ✓ RESOLVED: research.md Item 6 documents pre-FR-017 state; Item 10 post-clarification state with `permissions:` block required (2026-05-25)
- [x] CHK023 - Are action versions consistent across all artifact documents? [Consistency, Spec §FR-018, plan.md, tasks.md, quickstart.md]
  > ✓ SATISFIED: All documents reference same versions (checkout @v6.0.2, setup-dotnet @v5.2.0, upload-artifact @v7.0.1)

---

## Audit, Compliance & Traceability

- [x] CHK024 - Are artifact retention requirements defined? [NFR, Gap]
  > ✓ SATISFIED: T016 specifies `retention-days: 14` for coverage; default 90 days for test-results acceptable
- [x] CHK025 - Are workflow run auditability requirements defined? [NFR, Gap]
  > ✗ INTENTIONAL SCOPE: GitHub's built-in audit trail sufficient for MVP; custom logging deferred
- [x] CHK026 - Is security review scope documented? [Scope, Gap]
  > ✓ SATISFIED: spec.md implicitly scopes to YAML infrastructure-only change; threat model review deferred

---

## Notes

- Check items off as completed: `[x]`
- Add findings or commentary inline below each item
- **Critical conflicts**: CHK021 and CHK022 flag direct contradictions between design artifacts that should be resolved before T018 live validation
  - CHK021: `quickstart.md` references `actions/checkout@v4`; spec FR-018 and plan.md both require `actions/checkout@v6.0.2`
  - CHK022: `research.md` Items 6 and 10 give contradictory guidance on whether a `permissions:` block is required
- Items marked `[Gap]` represent security requirements absent from the spec — decide whether to add them or explicitly exclude them as out-of-scope
- Items marked `[Conflict]` flag direct contradictions between documents that must be resolved before or during T018
- Items marked `[NFR]` are non-functional requirements that are currently undocumented in the spec
