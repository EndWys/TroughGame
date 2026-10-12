# Stage Deliverables

## Purpose

Use this reference for the developer-facing handoffs that most often lose essential information when the full detail is stored in feature artifacts. `response-style.md` decides presentation; this reference decides the minimum factual content. Do not substitute a link to an artifact for the required summary.

## Requirements approval

Use `requirements-contract.md` as the single content template. Show its complete applicable contract in chat before requesting approval; `requirements.md` preserves the same approved contract for resumption.

## Research and feature-plan approval

Use `project-research.md` as the single content template. Show its complete applicable Research Brief, integration map, proposed initial feature plan and actual boundaries or decisions in chat before requesting approval; `research.md` preserves the same approved result for resumption.

## Local architecture approval

Use `subtask-architecture.md` as the single content template. Before the approval request, show its same applicable sections in chat: scope and outcome; change inventory; data/state/lifecycle; Unity/DI/serialized integration; design choices; risks, decisions and scope conflicts; validation. Omit only sections that are genuinely inapplicable; do not write empty negative placeholders.

For an existing system, state its relevant behavior **before → after**. For a new system, state the resulting observable behavior. The execution chain uses discovered `Type.Member(...)` names from trigger to observable outcome. The developer must be able to approve the plan without opening `subtasks/FEAT-XX.md`; that artifact is the durable record of the same plan.

## Implementation handoff

Before AI audit, provide a compact review handoff for the completed approved diff:

| Area / files | What changed | Review focus |
| --- | --- | --- |
| `Assets/.../EnemyBehaviour*.cs` | `<concise domain behavior>` | `<state transition or invalidation>` |
| `Assets/.../Prefab_Enemy.prefab` | `<required components or references>` | `<serialized binding>` |

- Show every material changed area. Group companion files only when they share one reviewable responsibility; do not hide a public contract, Unity-managed asset, configuration change or known limit inside a group.
- State automated validation with the exact executed command or tool, result and relevant evidence path.
- State a visible limitation or unavailable check when applicable; never imply that source changes alone prove behavior.
- Do not repeat the approved architecture, enumerate every method or provide final feature traceability. The complete diff is the source for line-level review.
- The next step is AI audit; after it, hand the same reviewed diff to the developer for accept/change review, then request Play Mode verification only after acceptance.

## AI code audit

Use `ai-code-audit.md` as the single content template. Show the reviewed scope and either its actionable findings table or the explicit no-actionable-findings result before proceeding to developer review.

## Bug diagnosis inside the current subtask

Use this handoff only for a defect found during the active feature subtask's review or Play Mode loop. It does not create a new subtask.

```markdown
## BUG DIAGNOSIS — `FEAT-01`

**Status:** `<confirmed cause / primary hypothesis / evidence insufficient>`.

### Observed behavior

| Expected | Actual | Evidence |
| --- | --- | --- |
| `<expected result>` | `<actual result>` | `<Play Mode steps, Console error, screenshot or state>` |

### Root-cause analysis

| Location | Evidence | Conclusion |
| --- | --- | --- |
| `Assets/.../Type.cs:Type.Member(...)` | `<concrete code or log observation>` | `<confirmed cause or falsifiable hypothesis>` |

### Proposed correction

| Scope | Change | Validation |
| --- | --- | --- |
| `<current subtask files or objects>` | `<smallest correction>` | `<automated check and repeated Play Mode scenario>` |
```

Show a scope-expansion request only when the correction actually exceeds approved scope. Request approval for the correction, or ask for one targeted diagnostic when evidence is insufficient. After an approved correction, repeat applicable validation and AI audit, then developer review and Play Mode verification. Do not repeat the complete architecture plan or create a new subtask.

## Scope expansion or architecture revision

Use this handoff only when a concrete discovered fact requires a change to the current approved subtask boundary or technical design. It does not create a new subtask.

```markdown
## SCOPE EXPANSION / ARCHITECTURE REVISION REQUIRED — `FEAT-01`

**Reason:** `<discovered fact that invalidates the current boundary or design>`.

### What changed

| Approved plan | Discovered fact | Effect |
| --- | --- | --- |
| `<current scope, owner or contract>` | `<concrete code, Unity state or validation evidence>` | `<why work cannot safely continue as approved>` |

### Requested revision

| Type | Affected scope | Proposed change | Why it is required |
| --- | --- | --- | --- |
| `Scope expansion` or `Architecture revision` | `<files, systems, Unity objects or contracts>` | `<smallest change>` | `<direct effect of the discovered fact>` |

### Validation impact

`<only new or changed validation when applicable>`
```

`Scope expansion` adds a file, system, Unity object or integration boundary outside approved scope. `Architecture revision` changes the real owner, contract, lifecycle or dependency inside the same scope. Show only the delta to the approved plan; do not use this handoff for a mechanical correction that preserves the approved design.

## Plan change

Use this handoff only after an explicit developer request to change the feature plan. If there is no such request, record an unchanged plan briefly rather than proposing a new subtask or plan revision.

```markdown
## PLAN CHANGE REQUIRED — `<Feature Name>`

**Trigger:** `<explicit developer request or approved direction>`.

### Basis

| New fact | Evidence | Affected plan area |
| --- | --- | --- |
| `<observed dependency, verified risk or requested priority change>` | `<research, validation or developer decision>` | `<existing FEAT IDs or plan boundary>` |

### Proposed plan delta

| Action | Affected items | Proposed change | Reason |
| --- | --- | --- | --- |
| `Add`, `remove`, `split`, `merge`, `reorder` or `resize scope` | `<FEAT IDs>` | `<precise plan change>` | `<direct effect of the fact>` |

### Delivery impact

`<only changed dependency, scope, validation or expected outcome>`
```

Show only the plan delta. Do not rewrite completed subtasks or use this handoff for a bug fix or review correction inside the current subtask. A requirements change is handled through the requirements contract, not this handoff.

## Final verification

Use this handoff after every approved subtask has completed its review and Play Mode loop. It verifies the feature's approved product contract as a whole; it is not another code review.

```markdown
## FINAL VERIFICATION — `<Feature Name>`

**Result:** `<N> Passed / <N> Failed / <N> Not tested`.

### Requirements traceability

| Contract | Implementation | Verification / evidence | Status |
| --- | --- | --- | --- |
| `RQ-01` | `Assets/.../Type.Method(...)` | `<automated result, Play Mode scenario or Inspector evidence>` | **Passed** |

### Known gaps

`<only Failed or Not tested item, consequence and missing evidence>`

**Next:** Workflow audit. Final readiness is not requested until its result is also reported.
```

Include every approved requirement and AC. `Implementation` names a concrete type/member, Unity object or asset without repeating the architecture plan. Evidence is an actual result, not merely the existence of code. Omit `Known gaps` when every item is `Passed`.

If any item is `Failed` or `Not tested`, do not proceed to final readiness. Return to the relevant existing subtask or request the missing evidence; do not create a new subtask.

## Workflow audit

Use this handoff after final verification and before final readiness. It audits process completeness and evidence; it is not another code audit.

```markdown
## WORKFLOW AUDIT — `<Feature Name>`

**Result:** **Passed** / **Attention required** / **Not available**.

### Evidence

- **Command:** `Tools/AI/workflow-audit.ps1 -Feature <feature>`.
- **Report:** `<Markdown or JSON evidence path>`.

### Findings

| Process point | Evidence | Effect | Recommended action |
| --- | --- | --- | --- |
| `<gate, state record, validation or Git history>` | `<concrete audit observation>` | `<missing or weakened process assurance>` | `<smallest workflow improvement or developer decision>` |
```

When there are no actionable findings, state `**Findings:** No actionable findings.` rather than showing an empty table. Do not repeat code-audit findings, requirements traceability or architecture. Do not change workflow knowledge automatically from an audit finding.

Persist final traceability in `final-verification.md`, record the audit status and evidence paths in state, and mark both reports developer-visible only after they are sent. `Passed` proceeds to final readiness. `Attention required` is visible input for final readiness and retrospective. `Not available` states the exact missing condition; it does not silently skip the gate.

## Retrospective

Use this handoff after final readiness. It draws only on workflow audit and observed feature-process evidence; it does not automatically modify workflow knowledge.

```markdown
## WORKFLOW RETROSPECTIVE — `<Feature Name>`

**Result:** `<N>` proposed improvements / **No actionable improvements**.

### Evidence-based proposals

| Observation | Evidence | Proposed change | Target | Expected effect |
| --- | --- | --- | --- | --- |
| `<repeated process weakness>` | `<audit finding, iteration count, failed check or scope correction>` | `<small repeatable rule or tool change>` | `<specific workflow doc, skill, script or project guidance>` | `<reliability or speed improvement>` |
```

Propose at most three improvements. Do not repeat raw workflow-audit findings: state the evidence-based conclusion and smallest reusable change. Do not promote a one-off bug or feature-specific gameplay rule into project policy. When there are no useful proposals, state `**Result:** No actionable improvements.` without an empty table. Every proposed change requires separate developer approval before it modifies persistent workflow knowledge.

## Knowledge promotion

Use this handoff only after final readiness and retrospective when there is an eligible candidate for permanent project knowledge. It is not a request to reopen the feature or create a new subtask.

```markdown
## KNOWLEDGE PROMOTION — `KNP-001`

**Decision required:** Approve / Reject / Request changes.

| Observed fact | Evidence | Proposed rule | Applies when | Canonical target |
| --- | --- | --- | --- | --- |
| `<fact, not conclusion>` | `<feature record or explicit decision>` | `<testable rule>` | `<boundary>` | `<one path>` |

### Consistency check

`<existing related rule, duplicate or conflict; or no material overlap found>`

### Effect and execution

- **Expected effect:** `<future risk or manual work reduced>`.
- **On approval:** `<bounded target change, target-appropriate validation and separate commit>`.
```

Show only eligible candidates, at most three for the feature. The developer's approval authorizes the exact promoted change and its separate commit; a changed target or rule requires a revised candidate and renewed approval. Record rejection or no-applicable-candidate result in feature state.
