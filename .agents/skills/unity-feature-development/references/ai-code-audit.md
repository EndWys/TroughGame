# AI Code Audit

## Purpose

Use this reference after implementation and relevant automated validation, before developer diff review. The audit independently searches for concrete risks in the approved subtask diff and relevant Unity assets; it is not a routine confirmation that code looks acceptable.

Read `docs/ARCHITECTURE_RULES.md` and `docs/CONVENTIONS.md` before auditing handwritten C#. Compare the diff against the approved local architecture, approved scope and validation evidence.

## Audit focus

Inspect only risk areas that can materially affect the approved change:

- behavior correctness, state consistency, null and error paths;
- ownership, dependency direction, composition, OOP/SOLID and project architecture;
- lifecycle readiness, initialization order, subscriptions, async/cancellation, disposal and resource release;
- Fusion/network authority, synchronization and serialization;
- Unity prefab, scene, Inspector, ScriptableObject and configuration bindings;
- CPU cost, allocation/GC pressure and resource lifetime;
- readability, extension boundaries and code-style violations that create a maintenance or correctness risk;
- scope compliance and agreement with the approved local architecture and validation plan.

Do not produce empty checklist rows. Do not report style preference, speculative risk or generic advice without a concrete location and consequence.

## Criticality

| Level | Meaning | Workflow effect |
| --- | --- | --- |
| `P0 — Blocker` | Crash, broken gameplay, data or network corruption, severe lifecycle failure, invalid Unity binding, or a feature that cannot be reliably verified. | Return to implementation; do not begin developer review. |
| `P1 — Required correction` | Architecture or reliability violation with a concrete consequence, substantial performance risk, or a project-convention violation that compromises maintainability. | Return to implementation; repeat applicable validation and audit. |
| `P2 — Recommendation` | Concrete extensibility, readability or optimization improvement while the approved behavior remains correct. | Show during developer review; do not refactor it without approval. |

Do not create a `P3` category. Omit trivial, subjective or consequence-free comments.

## Findings

Every actionable finding uses this table:

| ID | Risk point | Type | Criticality | Evidence / rationale | Consequence | Smallest recommended correction |
| --- | --- | --- | --- | --- | --- | --- |
| `AUD-01` | `Assets/.../Type.cs:Type.Member(...)` | `<lifecycle / architecture / allocation / readability / ...>` | `P0`, `P1` or `P2` | `<concrete observed condition>` | `<observable or maintenance effect>` | `<smallest in-scope correction>` |

The risk point names a path and concrete type/member or Unity object. `Type` describes the risk category, not a vague quality label. A correction that expands scope remains a recommendation or scope-expansion decision; the audit does not authorize the expansion.

## Developer-facing audit result

Start with the result and reviewed scope:

```markdown
## AI CODE AUDIT — `FEAT-01`

**Result:** `<N>` required corrections, `<N>` recommendations.

### Reviewed scope

- **Diff:** `<approved paths and Unity assets>`.
- **Compared against:** approved local architecture, `docs/ARCHITECTURE_RULES.md`, `docs/CONVENTIONS.md`.
- **Relevant risk areas:** `<only areas actually examined>`.

### Findings

<findings table>

### Required next step

<workflow action dictated by the highest finding criticality>
```

When there are no actionable findings, do not show an empty table. State `**Result:** No actionable findings.`, identify the reviewed scope and relevant analysis areas, then proceed to developer diff review.

## State and progression

Record the audit status, finding IDs and timestamp in the current subtask state. `P0` or `P1` returns to implementation, then requires relevant validation and a new AI audit. `P2` findings remain visible in developer review; the developer decides whether they become approved changes.
