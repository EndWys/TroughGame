# Requirements Contract

## Purpose

Use this reference before requirements approval for an eligible new feature. Its purpose is to create an approved, source-traceable product contract before research or technical design. It contains behavior and verification only; do not introduce files, classes, APIs, Unity objects, patterns or implementation options.

Record the full contract in `.ai/features/<feature>/requirements.md` and its compact status/reference in `state.yaml`. The developer-facing response shows the same applicable content; the artifact is the durable record, not a substitute for review.

## Required contract

### 1. Goal

State the desired player or game outcome in one or two source-backed sentences. For a change to existing behavior, include **current → desired behavior** only when the authoritative source defines that change.

### 2. Requirements

| ID | Behavioral requirement | Authoritative source | Constraints / notes |
| --- | --- | --- |
| `RQ-01` | `<observable game behavior>` | `<GDD section/page or approved decision>` | `<explicit restriction only>` |

Each requirement is observable and traceable to a source. Do not turn an implementation assumption, an inferred edge case or a likely architecture choice into a requirement.

### 3. Acceptance Criteria

| ID | Verifiable condition | Related requirement |
| --- | --- | --- |
| `AC-01` | `<given action or state → expected observable result>` | `RQ-01` |

An AC proves behavior, not the existence of source code, a class, a prefab or an asset.

### 4. Explicit boundaries

Include only an explicitly excluded behavior, an explicitly preserved existing behavior, or another source-backed boundary that materially affects the feature. Omit this section when none exists.

### 5. Confirmed decisions

| ID | Decision | Source |
| --- | --- | --- |
| `DEC-01` | `<developer-confirmed clarification>` | `<decision record or message>` |

Include only actual developer decisions. Omit this section when none exists.

## Blocking questions

Re-read the authoritative source before asking a question. Ask only when a missing, contradictory or genuinely ambiguous product rule changes behavior, acceptance criteria or verification.

Use this format for each independent blocking question:

```markdown
## DEVELOPER DECISION REQUIRED

**Q1 — `<decision>`:** `<smallest concrete question>`

| Checked authority | Finding | Why it blocks |
| --- | --- | --- |
| `<document section/page or decision>` | `<what is absent or contradictory>` | `<effect on behavior or verification>` |
```

Do not ask the developer to repeat a rule that is directly stated in an authoritative document. Do not add speculative questions that can be resolved safely during research.

## Approval handoff

Present the complete applicable contract in chat, then request approval:

```markdown
## REQUIREMENTS APPROVAL — `<Feature Name>`

**Decision required:** Approve the requirements contract for `<Feature Name>`?

### Goal

<source-backed outcome>

### Requirements

<requirements table>

### Acceptance Criteria

<AC table>

### Explicit boundaries

<only when applicable>

### Confirmed decisions

<only when applicable>

**Decision:** Approve to begin project research and decomposition, or request changes to the contract.
```

Do not proceed to research or decomposition until the developer explicitly approves the contract and all blocking questions are resolved.
