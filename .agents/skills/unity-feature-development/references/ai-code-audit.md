# Code Audit

## Purpose

Perform a separate review after implementation/checks, before developer review.
Compare all relevant code/Unity changes with approved architecture, scope,
results, `docs/ARCHITECTURE_RULES.md`, and `docs/CONVENTIONS.md`.

## Review Areas

Inspect concrete risks: correctness/state/null/errors; ownership/DI; readiness/
order/subscriptions/async/cancellation/disposal; Fusion authority/sync/serialization;
scene/prefab/Inspector/config bindings; CPU/GC/lifetime; maintainability/extension;
scope/design compliance. Omit empty items, subjective style preferences, and
speculation without an exact location and consequence.

## Severity

| Level | Meaning | Action |
| --- | --- | --- |
| `P0` | Crash, broken gameplay, data/network corruption, severe lifecycle/binding failure, or unreliable validation | Fix before review |
| `P1` | Reliability/architecture violation, material performance risk, or convention breach affecting maintenance | Fix, repeat checks/audit |
| `P2` | Supported extension/clarity/optimization improvement with correct behavior | Present; no unapproved refactoring |

Do not create P3; omit trivial preferences without consequences.

## Result and Transitions

The code-audit response shows required/recommended finding counts, reviewed diff,
approved architecture, and actually inspected areas.

| ID | Path / Type.Member / Unity Object | Category | Level | Evidence | Consequence | Minimal Fix |
| --- | --- | --- | --- | --- | --- | --- |
| AUD-01 | Verified location | Lifecycle/architecture/allocations | P0/P1/P2 | Fact | Effect | Approved scope or proposed revision |

If none, explicitly state no actionable findings, reviewed scope, and next review.
Audit does not permit scope expansion. Record status, finding IDs, and time in
the subtask. P0/P1 return to correction/checks/audit; P2 remain visible to the
developer. Correct defects within the current subtask and its existing gates.
Do not invoke `unity-bugfix` unless directly commanded by the developer.
