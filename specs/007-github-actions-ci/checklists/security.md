# Security & Supply Chain Checklist: GitHub Actions CI Build Workflow

**Purpose**: Validates the quality, completeness, and clarity of security and supply-chain requirements for the GitHub Actions CI workflow — testing whether the *security requirements are well-written*, not whether the implementation is secure.
**Created**: 2026-05-25
**Feature**: [spec.md](../spec.md) · [plan.md](../plan.md) · [research.md](../research.md) · [quickstart.md](../quickstart.md)

---

## Action Version Pinning & Supply Chain

- [ ] CHK001 - Is the decision to pin third-party actions (`dorny/test-reporter@v3.0.0`, `danielpalme/ReportGenerator-GitHub-Action@5.5.10`) to mutable version tags rather than immutable commit SHAs documented as an intentional security scope decision in spec? [Scope, Gap]
- [ ] CHK002 - Are requirements defined for an action version update policy — i.e., how and by whom action versions (`checkout@v6.0.2`, `setup-dotnet@v5.2.0`, `upload-artifact@v7.0.1`, `cache@v4`) should be reviewed and bumped over time (e.g., Dependabot, manual policy)? [Completeness, Gap]
- [ ] CHK003 - Is the risk of a compromised third-party action release (supply-chain attack) acknowledged and addressed in requirements — either by SHA pinning, required version review process, or an explicit acceptance of the risk? [NFR, Gap]
- [ ] CHK004 - Are requirements defined for who is authorised to approve action version changes in `.github/workflows/build.yml`, and is this enforced via a branch protection or CODEOWNERS rule? [Completeness, Gap]
- [ ] CHK005 - Is the `actions/cache@v4` version pinned only at the major version (not a patch version) — is this inconsistency with the patch-pinned first-party actions (`checkout@v6.0.2`, `setup-dotnet@v5.2.0`, `upload-artifact@v7.0.1`) documented as intentional in spec or plan? [Consistency, plan.md §Primary Dependencies]

---

## GITHUB_TOKEN Permission Scoping

- [ ] CHK006 - Are the minimum required `GITHUB_TOKEN` permission scopes (`contents: read`, `checks: write`, `actions: read`) documented as requirements in spec rather than left entirely to plan.md as an implementation detail? [Completeness, Spec §FR-017 vs plan.md §Constraints]
- [ ] CHK007 - Is the rationale for each declared permission scope individually justified and traceable to a specific workflow step — i.e., is it documented *which step* requires `checks: write` and *which step* requires `actions: read`? [Clarity, plan.md §Constraints]
- [ ] CHK008 - Are requirements defined for workflow behaviour when an organisation's GitHub Actions permission policy overrides the declared `permissions:` block and reduces effective scopes below `checks: write`? [Edge Case, Gap]
- [ ] CHK009 - Is the job-level `permissions:` block placement (rather than workflow-level) documented as an intentional, minimal-privilege scoping decision in spec or plan? [Clarity, Gap]
- [ ] CHK010 - Are requirements defined for explicitly prohibiting permissions the workflow does NOT need — e.g., `contents: write`, `deployments`, `packages`, `pull-requests: write` — as a positive security boundary, rather than relying on GitHub's default deny? [NFR, Gap]

---

## Fork PR Security Model

- [ ] CHK011 - Is the fork PR security constraint (read-only `GITHUB_TOKEN` regardless of declared permissions) documented as a named spec requirement or assumption rather than only appearing in plan.md and research.md as an implementation detail? [Completeness, Spec §FR-017 vs plan.md §Constraints]
- [ ] CHK012 - Is the explicit rejection of `pull_request_target` as a trigger (which would grant elevated permissions to untrusted fork code) documented as a security decision in spec, not merely as an implementation choice in research.md? [NFR, research.md §Item 10 Alternatives]
- [ ] CHK013 - Are requirements defined for what "publication is unsuccessful" means for a fork PR (where Check Run creation fails by design) — does FR-017's "fail the workflow if publication is unsuccessful" apply to permission-denied Check Run failures, or only to test-result-driven failures? [Clarity, Spec §FR-017]
- [ ] CHK014 - Is the `dorny/test-reporter@v3.0.0` Job Summary fallback for fork PRs documented in spec as an acceptable fulfilment of FR-017's "for all pull requests" requirement, rather than being silently relied upon as a v3 implementation detail? [Completeness, Spec §FR-017 vs research.md §Item 10]

---

## Secrets & Sensitive Data

- [ ] CHK015 - Are requirements explicitly stating that this workflow neither requires nor permits the use of any secrets, environment variables containing credentials, or sensitive tokens beyond the automatic `GITHUB_TOKEN`? [Completeness, Gap]
- [ ] CHK016 - Are requirements defined for how a future contributor should handle a scenario where a new CI step does require a secret — i.e., is there a documented constraint that prevents secret introduction without a spec review? [NFR, Gap]
- [ ] CHK017 - Is the NuGet cache poisoning risk — where a tampered cache entry could substitute malicious packages — acknowledged in spec or plan, and are requirements defined for cache integrity validation or an accepted risk statement? [NFR, Gap]

---

## Concurrency & Denial-of-Service Behaviour

- [ ] CHK018 - Are requirements defined for concurrent workflow run behaviour — e.g., should a new push to `main` cancel an already in-progress workflow run, or queue behind it? [Completeness, Gap]
- [ ] CHK019 - Are requirements defined for maximum workflow job timeout to prevent indefinitely stalled runs from consuming runner minutes or blocking branch protection checks? [NFR, Gap]
- [ ] CHK020 - Are requirements defined for workflow behaviour under rapid, repeated pushes to `main` (e.g., 10 commits in 60 seconds) — is there a throttling, queuing, or cancellation policy? [Edge Case, Gap]

---

## Cross-Artifact Security Consistency

- [ ] CHK021 - Is the `actions/checkout` version referenced in quickstart.md (`@v4`, line 122 of quickstart.md) consistent with the version required by FR-018 (`@v6.0.2`) and specified in plan.md? The quickstart.md YAML block appears to reflect a pre-FR-018 version of the workflow. [Conflict, Spec §FR-018 vs quickstart.md §Workflow Reference]
- [ ] CHK022 - Does research.md Item 6 ("no `permissions:` block is required; the default read-only token is sufficient") directly contradict research.md Item 10 ("job-level `permissions:` block must be added with `checks: write` and `actions: read`")? Is this pre/post-FR-017 evolution explicitly resolved in spec or plan rather than left as a silent contradiction in research.md? [Conflict, research.md §Item 6 vs §Item 10]
- [ ] CHK023 - Are the security-relevant action versions in plan.md (checkout `@v6.0.2`, setup-dotnet `@v5.2.0`, upload-artifact `@v7.0.1`) consistent across all four artifact documents — spec, plan, tasks, and quickstart — or are any references in quickstart.md or tasks.md lagging behind the final pinned versions? [Consistency, Spec §FR-018, FR-003, FR-014 vs quickstart.md vs tasks.md]

---

## Audit, Compliance & Traceability

- [ ] CHK024 - Are requirements defined for artifact retention duration — is GitHub Actions' default 90-day retention acceptable, or do compliance requirements mandate a different retention period for `test-results` and `coverage-report` artifacts? [NFR, Gap]
- [ ] CHK025 - Are requirements defined for the auditability of workflow runs — e.g., is it a requirement that the committer identity, trigger event, and workflow run URL be traceable from a test failure back to the originating commit? [NFR, Gap]
- [ ] CHK026 - Is the security review scope of this feature explicitly documented — i.e., does the spec state that no threat model review, penetration test, or security sign-off is required given the infrastructure-only (YAML config) nature of the change? [Scope, Gap]

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
