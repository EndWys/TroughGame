# Requirements Contract

## Purpose

Before research/design, prepare an agreed behavior and validation contract.
Exclude classes, files, API, Unity objects, and implementation options. Save it
in `.ai/features/<feature>/requirements.md`; record status/reference in state.
Present all applicable content in chat.
For selected Notion sources at `requirements`, explicitly load
`../../notion-feature-requirements/SKILL.md` and dispatch its isolated worker for
source-backed requirements, task IDs and proposed AC. Page status is not approval.
Reuse the accepted local contract on resume; no automatic source refresh. A material
gap goes to the developer; another intake needs an explicit requirements request.

## Contract Content

### Goal

One or two source-backed sentences about the desired player/game outcome.
Describe current → required behavior only when the source defines it.

### Requirements

| ID | Observable Behavior | Authoritative Source | Constraints |
| --- | --- | --- | --- |
| `RQ-01` | Concrete outcome | Specification section/page or accepted decision | Explicit constraints only |

Do not turn implementation assumptions, anticipated edge cases, or likely patterns
into new requirements.

### Acceptance Criteria (AC)

| ID | Verifiable Condition | Requirement |
| --- | --- | --- |
| `AC-01` | Action/state → observable result | `RQ-01` |

Criteria prove behavior, not the existence of a class, prefab, or source file.

### Scope and Accepted Decisions

Include only explicit exclusions/preserved behavior and material source-backed
boundaries. Decisions have ID, content, and approval source. Omit empty sections.

## Blocking Questions

Re-read sources before asking. Ask only when absence, contradiction, or ambiguity
changes behavior, AC, or validation. Do not repeat defined requirements or ask
questions resolvable by research.

| Checked Source | Missing / Conflicting Fact | Blocking Impact | Question |
| --- | --- | --- | --- |
| Exact section/decision | Actual gap | Behavior/validation | Minimal choice |

## Approval and Result

The requirements handoff shows goal, requirements, AC, applicable scope/decisions,
and the exact approve/change request. Use the developer's language for the response.
Do not start research/decomposition until explicit approval and resolution of blockers.
Follow the common documentation/response standard.
